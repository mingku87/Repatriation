using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public partial class MapLoader : MonoBehaviour
{
    public static MapLoader Instance { get; private set; }
    void Awake() => Instance = this;

    [Header("References")]
    public Transform player;
    public MapChunk currentChunk;

    [Header("Ray Settings")]
    [Tooltip("플레이어가 포탈을 통과할 때 이동할 거리 (미터 단위)")]
    [Range(1f, 50f)]
    public float rayDistance = 20f;   // ← 인스펙터에서 직접 조정 가능!

    [Header("Run Settings")]
    public float runSpeed = 6f;

    bool busy, portalLock;


    // MapLoader.cs
    public void TryGoThroughFixedRay(Portal portal, Transform who)
    {
        if (who != player) return;
        if (busy || portalLock) { Debug.Log("[Loader] blocked: busy/lock"); return; }
        if (portal.Owner != currentChunk) { Debug.Log("[Loader] blocked: not current chunk"); return; }
        if (!portal.nextMapPrefab) { Debug.LogError("[Loader] nextMapPrefab not set"); return; }

        StartCoroutine(CoGoThroughFixedRay(portal));
    }

    IEnumerator CoGoThroughFixedRay(Portal portal)
    {
        busy = true;
        portalLock = true;

        var input = player.GetComponent<PlayerInputBlocker>();
        if (input) input.SetBlocked(true);

        var anim = player.GetComponent<Animator>();
        if (anim) anim.SetBool("IsRunning", true);

        GameObject nextGO = null;
        MapChunk nextChunk = null;

        // --- 1) 진행 방향 계산 (각도 사용) ---
        float rayAngle = portal.customRayAngle;
        if (Mathf.Approximately(rayAngle, 0f))
            rayAngle = DirUtil.ToRayAngleDeg(portal.direction);   // 기본각 사용

        Vector2 dir2 = new Vector2(Mathf.Cos(rayAngle * Mathf.Deg2Rad),
                                   Mathf.Sin(rayAngle * Mathf.Deg2Rad)).normalized;
        Vector3 dir3 = new Vector3(dir2.x, dir2.y, 0f);

        if (anim) { anim.SetFloat("MoveX", dir2.x); anim.SetFloat("MoveY", dir2.y); }

        // --- 2) 20m 지점까지 이동 ---
        Vector3 startPos = player.position;
        Vector3 endPos = startPos + dir3 * rayDistance;   // rayDistance = 20f
        Debug.DrawLine(startPos, endPos, Color.cyan, 1.5f);

        yield return MoveTo(player, endPos, runSpeed);

        // --- 3) 다음 맵 생성/검증 ---
        nextGO = Instantiate(portal.nextMapPrefab);
        nextChunk = nextGO ? nextGO.GetComponent<MapChunk>() : null;
        if (!nextChunk)
        {
            Debug.LogError("[Loader] nextMapPrefab has no MapChunk");
            if (nextGO) Destroy(nextGO);
            goto CLEANUP;        // 실패 시 공통 정리로
        }

        var entry = nextChunk.FindPortal(portal.entryDirectionOnNext);
        if (!entry)
        {
            Debug.LogError($"[Loader] Missing entry portal {portal.entryDirectionOnNext}");
            Destroy(nextGO);
            goto CLEANUP;
        }

        // --- 4) 절대 스냅(엔트리 앵커 == endPos) ---
        nextChunk.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        nextChunk.transform.localScale = Vector3.one;
        Vector3 entryAtZero = entry.anchor.position;
        nextChunk.transform.position = endPos - entryAtZero;

        // --- 5) 살짝 더 들어가기 ---
        player.position = entry.anchor.position + dir3 * 0.01f;
        Vector3 inTarget = entry.anchor.position + dir3 * 1.2f;
        yield return MoveTo(player, inTarget, runSpeed);

        // --- 6) 맵 교체 & 정리 ---
        var prev = currentChunk;
        currentChunk = nextChunk;
        if (prev) Destroy(prev.gameObject);
        CleanupOtherChunks(currentChunk);

    CLEANUP:
        // 공통 마무리(여기는 yield 가능)
        if (anim) anim.SetBool("IsRunning", false);
        if (input) input.SetBlocked(false);

        yield return new WaitForSeconds(0.1f); // 재트리거 쿨다운
        portalLock = false;
        busy = false;
    }


    IEnumerator MoveTo(Transform t, Vector3 target, float speed)
    {
        while ((t.position - target).sqrMagnitude > 0.0004f)
        {
            t.position = Vector3.MoveTowards(t.position, target, speed * Time.deltaTime);
            yield return null;
        }
        t.position = target;
    }

    // 기존 하드닝 청소 루틴 유지
    void CleanupOtherChunks(MapChunk keep)
    {
#if UNITY_2021_3_OR_NEWER
        var all = Resources.FindObjectsOfTypeAll<MapChunk>();
#else
    var all = FindObjectsOfType<MapChunk>();
#endif
        foreach (var c in all)
        {
            if (!c) continue;
            var go = c.gameObject;
            if (!go.scene.IsValid() || !go.scene.isLoaded) continue;   // 씬 외/프리팹 에셋 제외
            if (c != keep) Destroy(go);
        }
    }
}
