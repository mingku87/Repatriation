using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    public ExitDir direction;
    [Tooltip("이 포탈이 쏘는 레이캐스트 각도 (도 단위)")]
    public float customRayAngle = 60f;   // <-- Inspector에서 직접 조정 가능
    public GameObject nextMapPrefab;
    public ExitDir entryDirectionOnNext;
    public Transform anchor;
    public MapChunk Owner;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        if (!anchor) anchor = transform;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌한 객체의 루트 GameObject를 가져오기 (Rigidbody2D가 있으면 그것 기준)
        var who = other.attachedRigidbody ? other.attachedRigidbody.gameObject
                                          : other.transform.root.gameObject;

        // Player가 아니면 무시
        if (!who.CompareTag("Player")) return;

        // 현재 맵의 포탈만 반응 (다른 맵 잔여물 방지)
        if (MapLoader.Instance && MapLoader.Instance.currentChunk != Owner) return;

        // 레이 기반 이동 실행
        MapLoader.Instance?.TryGoThroughFixedRay(this, who.transform);

    }

    public void OnHitFromChild(Transform playerTf)
    {
        if (MapLoader.Instance == null)
        {
            Debug.LogError("[Portal] MapLoader.Instance null");
            return;
        }

        if (MapLoader.Instance.currentChunk != Owner)
        {
            Debug.Log("[Portal] blocked: not current chunk");
            return;
        }

        MapLoader.Instance?.TryGoThroughFixedRay(this, playerTf);
    }
}
