using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
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

    bool busy, portalLock;

    public void TryGoThroughFixedRay(Portal portal, Transform who)
    {
        // 플레이어 찾기 보정
        var playerTf = (Player.Instance != null) ? Player.Instance.transform : player;
        if (playerTf == null)
        {
            Debug.LogError("[Loader] player reference missing");
            return;
        }
        // 동일성 대신 태그 보정
        if (who != playerTf && !who.CompareTag("Player")) return;

        if (busy || portalLock) { Debug.Log("[Loader] blocked: busy/lock"); return; }
        if (portal.Owner != currentChunk) { Debug.Log("[Loader] blocked: not current chunk"); return; }
        if (!portal.nextMapPrefab) { Debug.LogError("[Loader] nextMapPrefab not set"); return; }

        StartCoroutine(CoGoThroughFixedRay(portal));
    }

    void Start()
    {
        if (currentChunk == null)
        {
            var first = FindObjectOfType<MapChunk>();
            if (first) currentChunk = first;
            else Debug.LogError("[Loader] currentChunk not assigned at start");
        }
    }

    IEnumerator CoGoThroughFixedRay(Portal portal)
    {
        busy = true;
        portalLock = true;

        // 대상/컴포넌트 캐시
        Transform p = (Player.Instance != null) ? Player.Instance.transform : player;
        Animator anim = null;                // ← 초기화 (CS0165 방지)
        MoveCamera cam = null;               // ← 초기화
        System.Action restoreNoClip = null;  // ← 초기화

        if (p == null)
        {
            Debug.LogError("[Loader] player transform is null");
            goto CLEANUP_EARLY;
        }

        anim = p.GetComponent<Animator>();
        cam = Camera.main ? Camera.main.GetComponent<MoveCamera>() : null;

        // ==== 입력 차단 & 러닝 애니 시작 ====
        if (Player.Instance != null) Player.Instance.SetInputBlocked(true);
        if (anim) anim.SetBool("IsRunning", true);

        // ==== 노클립 ON (전환 동안만) ====
        restoreNoClip = EnablePlayerNoClip();

        GameObject nextGO = null;
        MapChunk nextChunk = null;

        // ==== 1) 진행 방향(각도) 계산 ====
        float rayAngle = portal.customRayAngle;
        if (Mathf.Approximately(rayAngle, 0f))
            rayAngle = DirUtil.ToRayAngleDeg(portal.direction); // 60/120/240/300

        Vector2 dir2 = new Vector2(Mathf.Cos(rayAngle * Mathf.Deg2Rad),
                                   Mathf.Sin(rayAngle * Mathf.Deg2Rad)).normalized;
        Vector3 dir3 = new Vector3(dir2.x, dir2.y, 0f);

        if (anim) { anim.SetFloat("MoveX", dir2.x); anim.SetFloat("MoveY", dir2.y); }

        // ==== 2) 레이 목표점 ====
        Vector3 startPos = p.position;
        Vector3 endPos = startPos + dir3 * rayDistance;
        Debug.DrawLine(startPos, endPos, Color.cyan, 1.5f);

        // ==== 3) 다음 맵 생성 & 엔트리 포탈 확보 ====
        nextGO = Instantiate(portal.nextMapPrefab);
        nextChunk = nextGO ? nextGO.GetComponent<MapChunk>() : null;
        if (!nextChunk) { Debug.LogError("[Loader] nextMapPrefab has no MapChunk"); if (nextGO) Destroy(nextGO); goto CLEANUP; }

        var entry = nextChunk.FindPortal(portal.entryDirectionOnNext);
        if (!entry) { Debug.LogError($"[Loader] Missing entry portal {portal.entryDirectionOnNext}"); Destroy(nextGO); goto CLEANUP; }

        // ==== 4) 절대 스냅: 엔트리 앵커 == endPos ====
        nextChunk.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        nextChunk.transform.localScale = Vector3.one;
        Vector3 entryAtZero = entry.anchor.position; // nextChunk=(0,0,0) 기준
        nextChunk.transform.position = endPos - entryAtZero;

        // ==== 5) 카메라 '도착 포탈'에 고정 ====
        if (cam) cam.LockTo(entry.anchor, snap: true);

        // ==== 6) 플레이어 20m 지점까지 이동 ====
        yield return MoveTo(p, endPos, runSpeed);

        // ==== 7) 살짝 안쪽으로 한 걸음 ====
        p.position = entry.anchor.position + dir3 * 0.01f;
        Vector3 inTarget = entry.anchor.position + dir3 * 1.2f;
        yield return MoveTo(p, inTarget, runSpeed);

        // ==== 8) 현재맵 교체 & 정리 ====
        {
            var prev = currentChunk;
            currentChunk = nextChunk;
            if (prev) Destroy(prev.gameObject);
            CleanupOtherChunks(currentChunk); // 보험
        }

    CLEANUP:
        // ==== 9) 카메라를 다시 플레이어 추적으로 ====
        if (cam) cam.FollowPlayer(snap: false);

        CLEANUP_EARLY:
        // ==== 공통 마무리 ====
        restoreNoClip?.Invoke();                    // 노클립 OFF
        if (anim) anim.SetBool("IsRunning", false);
        if (Player.Instance != null) Player.Instance.SetInputBlocked(false);

        yield return new WaitForSeconds(0.1f);      // 재트리거 쿨다운
        portalLock = false;
        busy = false;
    }

    // 플레이어 노클립 ON/OFF 헬퍼 (레이어 안 건드리는 방식)
    System.Action EnablePlayerNoClip()
    {
        // ★ 삼항 연산자 두 피연산자 모두 Transform 로 통일 (CS0173 방지)
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
                cols[i].isTrigger = true; // 벽/타일 통과
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
            if (!go.scene.IsValid() || !go.scene.isLoaded) continue;
            if (c != keep) Destroy(go);
        }
    }

    
    public void TryDoorToDoor(DoorPortal from, DoorPortal to, float angleDeg)
    {
        const string TAG = "[Loader/Door↔Door]";
        int step = 0;

        if (!from || !to)
        {
            Debug.LogError($"{TAG} S{++step} Null door ref");
            return;
        }

        if (busy || portalLock)
        {
            Debug.Log($"{TAG} S{++step} blocked: busy/lock");
            return;
        }

        // 맵이 다르면 프리팹 인스턴스 교체
        if (currentChunk != to.Owner)
        {
            // 필요한 경우 프리팹 교체
            if (to.Owner == null && to.targetMapPrefab != null)
            {
                var nextGO = Instantiate(to.targetMapPrefab);
                to.Owner = nextGO.GetComponent<MapChunk>();
            }

            if (to.Owner)
            {
                var prev = currentChunk;
                currentChunk = to.Owner;
                if (prev) Destroy(prev.gameObject);
                CleanupOtherChunks(currentChunk);
            }
        }

        StartCoroutine(CoDoorToDoor(from, to, angleDeg));
    }

    public void TryDoorById(DoorPortal from, string targetDoorId, GameObject targetMapPrefab, float angleDeg)
    {
        const string TAG = "[Loader/DoorID]";
        if (busy || portalLock) { Debug.Log($"{TAG} busy/lock"); return; }
        if (!from) { Debug.LogError($"{TAG} from is null"); return; }

        // 1) 현재 맵에서 먼저 도착 문 찾기
        DoorPortal to = currentChunk ? currentChunk.FindDoorById(targetDoorId) : null;

        // 2) 없으면 프리팹 로드
        if (!to && targetMapPrefab)
        {
            var nextGO = Instantiate(targetMapPrefab);
            var nextChunk = nextGO.GetComponent<MapChunk>();
            if (!nextChunk)
            {
                Debug.LogError($"{TAG} targetMapPrefab에 MapChunk 없음");
                Destroy(nextGO);
                return;
            }

            var prev = currentChunk;
            currentChunk = nextChunk;
            if (prev) Destroy(prev.gameObject);
            CleanupOtherChunks(currentChunk);

            to = currentChunk.FindDoorById(targetDoorId);
        }

        if (!to)
        {
            Debug.LogError($"{TAG} targetDoorId='{targetDoorId}' 문을 찾지 못했습니다.");
            return;
        }

        StartCoroutine(CoDoorToDoor(from, to, angleDeg));
    }

    private IEnumerator CoDoorToDoor(DoorPortal from, DoorPortal to, float angleDeg)
    {
        const string TAG = "[Loader/Door↔Door]";
        int step = 0;

        busy = true; portalLock = true;
        if (to.verboseLogs)
            Debug.Log($"{TAG} S{++step} ENTER: from={from.name} → to={to.name}");

        Transform p = (Player.Instance != null) ? Player.Instance.transform : player;
        if (!p) yield break;

        Animator anim = p.GetComponent<Animator>();
        MoveCamera cam = Camera.main ? Camera.main.GetComponent<MoveCamera>() : null;

        if (Player.Instance != null) Player.Instance.SetInputBlocked(true);
        if (anim) anim.SetBool("IsRunning", true);

        var restoreNoClip = EnablePlayerNoClip();

        Vector2 dir2 = new(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
        Vector3 dir3 = new(dir2.x, dir2.y, 0);

        Vector3 worldFrom = from.anchor ? from.anchor.position : p.position;
        Vector3 worldTo = to.anchor ? to.anchor.position : p.position;

        // 한 걸음 들어가기
        Vector3 inTarget = worldFrom + dir3 * from.stepDistance;
        yield return MoveTo(p, inTarget, runSpeed);

        // 위치 이동
        p.position = worldTo + dir3 * 0.05f;
        if (cam) cam.LockTo(to.anchor, snap: true);
        yield return MoveTo(p, worldTo + dir3 * to.stepDistance, runSpeed);

        if (cam) cam.FollowPlayer(snap: false);
        restoreNoClip?.Invoke();
        if (anim) anim.SetBool("IsRunning", false);
        if (Player.Instance != null) Player.Instance.SetInputBlocked(false);

        yield return new WaitForSeconds(0.08f);
        portalLock = false; busy = false;
        if (to.verboseLogs)
            Debug.Log($"{TAG} S{++step} EXIT: busy={busy} lock={portalLock}");
    }
}
