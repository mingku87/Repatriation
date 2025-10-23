using UnityEngine;

[RequireComponent(typeof(Collider2D))]
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
        var c2d = GetComponent<Collider2D>();
        c2d.isTrigger = true;
        if (!anchor) anchor = transform;
        if (!Owner) Owner = GetComponentInParent<MapChunk>();
    }
    
    void OnValidate()
    {
        var c2d = GetComponent<Collider2D>();
        if (c2d && !c2d.isTrigger) c2d.isTrigger = true;
        if (!anchor) anchor = transform;
        if (!Owner) Owner = GetComponentInParent<MapChunk>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // NPC 콜라이더가 먼저 닿아도 걸러짐
        if (!other.CompareTag("Player")) return;

        // 현재 청크가 아니면 무시
        if (!MapLoader.Instance || MapLoader.Instance.currentChunk != Owner) return;

        // 플레이어 Transform 전달
        MapLoader.Instance.TryGoThroughFixedRay(this, other.attachedRigidbody ?
            other.attachedRigidbody.transform : other.transform);

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
