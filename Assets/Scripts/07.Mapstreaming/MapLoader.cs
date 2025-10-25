using System.Collections;
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
    public float rayDistance = 10f;

    [Header("Run Settings")]
    public float runSpeed = 10f;

    [Header("Bootstrap")]
    public GameObject initialMapPrefab;

    bool busy, portalLock;

    void Start()
    {
        // 1) 씬에 이미 배치된 MapChunk가 있으면 우선 채택
        if (currentChunk == null)
        {
            var found = FindObjectOfType<MapChunk>();
            if (found) currentChunk = found;
        }

        // 2) 그래도 없으면 initialMapPrefab을 인스턴스해서 currentChunk 설정
        if (currentChunk == null)
        {
            if (initialMapPrefab != null)
            {
                var inst = Instantiate(initialMapPrefab);
                currentChunk = inst.GetComponent<MapChunk>();
            }
            if (currentChunk == null)
                Debug.LogError("[Loader] currentChunk not assigned/instantiated");
        }
    }

    // 외부(MapChunk.Awake 등)에서 자신을 등록할 수 있도록 공개
    public void TryAdoptChunk(MapChunk chunk)
    {
        if (!chunk) return;
        if (currentChunk == null) currentChunk = chunk;
    }

    public MapChunk GetOrFindCurrentChunk()
    {
        if (currentChunk) return currentChunk;
        var found = FindObjectOfType<MapChunk>();
        if (found) currentChunk = found;
        return currentChunk;
    }

    // ================================================================
    //  프리팹 스테이지 가드 / 안전삭제 유틸
    // ================================================================
#if UNITY_EDITOR
    private bool IsInPrefabStage()
    {
        return UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null;
    }
#else
    private bool IsInPrefabStage() => false;
#endif

    private void SafeDestroyChunk(MapChunk chunk)
    {
        if (!chunk) return;
        var go = chunk.gameObject;

        // 씬 인스턴스만 삭제
        if (!go.scene.IsValid() || !go.scene.isLoaded) return;

#if UNITY_EDITOR
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(go))
        {
            Debug.LogWarning("[Loader] Prevented destroying a prefab asset root (Prefab Mode?).");
            return;
        }
#endif
        Destroy(go);
    }

    // ================================================================
    //  포탈 이동 (기존 RayTravel)
    // ================================================================
    public void TryGoThroughFixedRay(Portal portal, Transform who)
    {
        var playerTf = (Player.Instance != null) ? Player.Instance.transform : player;
        if (playerTf == null) { Debug.LogError("[Loader] player reference missing"); return; }
        if (who != playerTf && !who.CompareTag("Player")) return;

        if (busy || portalLock) { Debug.Log("[Loader] blocked: busy/lock"); return; }
        if (portal.Owner != currentChunk) { Debug.Log("[Loader] blocked: not current chunk"); return; }
        if (!portal.nextMapPrefab) { Debug.LogError("[Loader] nextMapPrefab not set"); return; }

        StartCoroutine(CoGoThroughFixedRay(portal));
    }

    IEnumerator CoGoThroughFixedRay(Portal portal)
    {
        if (IsInPrefabStage())
        {
            Debug.LogError("[Loader] Prefab Mode detected. Exit to a Scene before testing portal travel.");
            yield break;
        }

        busy = true;
        portalLock = true;

        Transform p = (Player.Instance != null) ? Player.Instance.transform : player;
        if (!p) { busy = portalLock = false; yield break; }

        Animator anim = p.GetComponent<Animator>();
        MoveCamera cam = Camera.main ? Camera.main.GetComponent<MoveCamera>() : null;
        System.Action restoreNoClip = null;

        if (Player.Instance != null) Player.Instance.SetInputBlocked(true);
        if (anim) anim.SetBool("IsRunning", true);

        restoreNoClip = EnablePlayerNoClip();

        // 1) 진행 방향 계산
        float rayAngle = portal.customRayAngle;
        if (Mathf.Approximately(rayAngle, 0f))
            rayAngle = DirUtil.ToRayAngleDeg(portal.direction);

        Vector2 dir2 = new(Mathf.Cos(rayAngle * Mathf.Deg2Rad), Mathf.Sin(rayAngle * Mathf.Deg2Rad));
        Vector3 dir3 = new(dir2.x, dir2.y, 0f);

        if (anim) { anim.SetFloat("MoveX", dir2.x); anim.SetFloat("MoveY", dir2.y); }

        Vector3 startPos = p.position;
        Vector3 endPos = startPos + dir3 * rayDistance;
        Debug.DrawLine(startPos, endPos, Color.cyan, 1.5f);

        // 2) 다음 맵 생성 (비활성화 후 위치 세팅)
        GameObject nextGO = Instantiate(portal.nextMapPrefab);
        nextGO.SetActive(false); // 깜빡임 방지
        MapChunk nextChunk = nextGO.GetComponent<MapChunk>();

        if (!nextChunk)
        {
            Debug.LogError("[Loader] nextMapPrefab has no MapChunk");
            Destroy(nextGO);
            goto CLEANUP;
        }

        var entry = nextChunk.FindPortal(portal.entryDirectionOnNext);
        if (!entry)
        {
            Debug.LogError($"[Loader] Missing entry portal {portal.entryDirectionOnNext}");
            Destroy(nextGO);
            goto CLEANUP;
        }

        nextChunk.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        nextChunk.transform.localScale = Vector3.one;
        Vector3 entryAtZero = entry.anchor.position; // nextChunk 원점 기준
        nextChunk.transform.position = endPos - entryAtZero;

        yield return null;             // 위치 세팅 프레임 통과
        nextGO.SetActive(true);        // 이후 프레임에만 보이게

        // 3) 카메라 잠깐 고정
        if (cam) cam.LockTo(entry.anchor, snap: true);

        // 4) 이동
        yield return MoveTo(p, endPos, runSpeed);
        p.position = entry.anchor.position + dir3 * 0.01f;
        Vector3 inTarget = entry.anchor.position + dir3 * 1.2f;
        yield return MoveTo(p, inTarget, runSpeed);

        // 5) 맵 교체
        {
            var prev = currentChunk;
            currentChunk = nextChunk;
            SafeDestroyChunk(prev);
            CleanupOtherChunks(currentChunk);
        }

    CLEANUP:
        if (cam) cam.FollowPlayer(snap: false);

        restoreNoClip?.Invoke();
        if (anim) anim.SetBool("IsRunning", false);
        if (Player.Instance != null) Player.Instance.SetInputBlocked(false);

        yield return new WaitForSeconds(0.1f);
        portalLock = false;
        busy = false;
    }

    // ================================================================
    //  문(레이) 이동 — 문(anchor) 기준 + 거리 0 허용
    // ================================================================
    public void TryDoorRayTravel(DoorPortal door, Transform who)
    {
        var playerTf = (Player.Instance != null) ? Player.Instance.transform : player;
        if (playerTf == null) { Debug.LogError("[Loader/DoorRay] player reference missing"); return; }
        if (who != playerTf && !who.CompareTag("Player")) return;
        if (busy || portalLock) { Debug.Log("[Loader/DoorRay] blocked: busy/lock"); return; }
        if (door.Owner != currentChunk) { Debug.Log("[Loader/DoorRay] blocked: not current chunk"); return; }
        if (!door.nextMapPrefab) { Debug.LogError("[Loader/DoorRay] nextMapPrefab not set"); return; }

        StartCoroutine(CoDoorRayTravel(door));
    }

    private IEnumerator CoDoorRayTravel(DoorPortal door)
    {
        if (IsInPrefabStage())
        {
            Debug.LogError("[Loader] Prefab Mode detected. Exit to a Scene before testing door travel.");
            yield break;
        }

        busy = true;
        portalLock = true;

        Transform p = (Player.Instance != null) ? Player.Instance.transform : player;
        if (!p) { busy = portalLock = false; yield break; }

        Animator anim = p.GetComponent<Animator>();
        MoveCamera cam = Camera.main ? Camera.main.GetComponent<MoveCamera>() : null;
        System.Action restoreNoClip = null;

        if (Player.Instance != null) Player.Instance.SetInputBlocked(true);
        if (anim) anim.SetBool("IsRunning", true);

        restoreNoClip = EnablePlayerNoClip();

        // 1) 방향 계산
        float rayAngle = door.customRayAngle;
        if (Mathf.Approximately(rayAngle, 0f))
            rayAngle = DirUtil.ToRayAngleDeg(door.direction);
        Vector2 dir2 = new(Mathf.Cos(rayAngle * Mathf.Deg2Rad), Mathf.Sin(rayAngle * Mathf.Deg2Rad));
        Vector3 dir3 = new(dir2.x, dir2.y, 0f);
        if (anim) { anim.SetFloat("MoveX", dir2.x); anim.SetFloat("MoveY", dir2.y); }

        // 2) 목표 위치(문 기준, 0 허용)
        Vector3 basePos = (door.anchor ? door.anchor.position : p.position);
        float dist = Mathf.Max(door.rayDistance, 0f); // 0 허용
        Vector3 endPos = basePos + dir3 * dist;
        Debug.DrawLine(basePos, endPos, Color.yellow, 1.5f);

        // 3) 새 맵 생성
        GameObject nextGO = Instantiate(door.nextMapPrefab);
        nextGO.SetActive(false);
        MapChunk nextChunk = nextGO.GetComponent<MapChunk>();

        if (!nextChunk)
        {
            Debug.LogError("[Loader/DoorRay] nextMapPrefab has no MapChunk");
            Destroy(nextGO);
            goto CLEANUP;
        }

        // 4) doorId 매칭 문 찾기 (없으면 entryDirectionOnNext 포탈)
        DoorPortal targetDoor = nextChunk.FindDoorById(door.doorId);
        if (!targetDoor)
            Debug.LogWarning($"[Loader/DoorRay] next map에 doorId '{door.doorId}' 문을 찾지 못했습니다. fallback 사용");

        nextChunk.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        nextChunk.transform.localScale = Vector3.one;

        Vector3 entryAtZero;
        if (targetDoor)
            entryAtZero = targetDoor.anchor ? targetDoor.anchor.position : Vector3.zero;
        else
        {
            var fallback = nextChunk.FindPortal(door.entryDirectionOnNext);
            entryAtZero = fallback ? fallback.anchor.position : Vector3.zero;
        }

        nextChunk.transform.position = endPos - entryAtZero;

        yield return null;       // 위치 세팅 프레임 통과
        nextGO.SetActive(true);  // 이후 프레임에만 보이게

        // 5) 카메라 스냅 (가능하면 타겟 도어 앵커로)
        if (cam)
        {
            if (targetDoor && targetDoor.anchor) cam.LockTo(targetDoor.anchor, snap: true);
        }

        // 6) 플레이어 이동 (문 앞 목표로)
        yield return MoveTo(p, endPos, runSpeed);

        // 7) 맵 교체
        {
            var prev = currentChunk;
            currentChunk = nextChunk;
            SafeDestroyChunk(prev);
            CleanupOtherChunks(currentChunk);
        }

    CLEANUP:
        if (cam) cam.FollowPlayer(snap: false);
        restoreNoClip?.Invoke();
        if (anim) anim.SetBool("IsRunning", false);
        if (Player.Instance != null) Player.Instance.SetInputBlocked(false);

        yield return new WaitForSeconds(0.1f);
        portalLock = false;
        busy = false;
    }

    // ================================================================
    //  공통 유틸
    // ================================================================
    System.Action EnablePlayerNoClip()
    {
        Transform t = (Player.Instance != null) ? Player.Instance.transform : player;

        var cols = t ? t.GetComponentsInChildren<Collider2D>(includeInactive: false) : null;
        var rb = t ? t.GetComponent<Rigidbody2D>() : null;

        bool[] prevTrigger = null;
        bool prevKinematic = false;

        if (cols != null && cols.Length > 0)
        {
            prevTrigger = new bool[cols.Length];
            for (int i = 0; i < cols.Length; i++)
            {
                prevTrigger[i] = cols[i].isTrigger;
                cols[i].isTrigger = true; // 벽 통과
            }
        }
        if (rb) { prevKinematic = rb.isKinematic; rb.isKinematic = true; }

        return () =>
        {
            if (cols != null)
                for (int i = 0; i < cols.Length; i++)
                    cols[i].isTrigger = prevTrigger != null ? prevTrigger[i] : cols[i].isTrigger;

            if (rb) rb.isKinematic = prevKinematic;
        };
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
            if (!go.scene.IsValid() || !go.scene.isLoaded) continue; // 에셋/프리팹 제외
            if (c != keep) Destroy(go);
        }
    }
}
