using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    public ExitDir direction;
    [Tooltip("�� ��Ż�� ��� ����ĳ��Ʈ ���� (�� ����)")]
    public float customRayAngle = 60f;   // <-- Inspector���� ���� ���� ����
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
        // NPC �ݶ��̴��� ���� ��Ƶ� �ɷ���
        if (!other.CompareTag("Player")) return;

        // ���� ûũ�� �ƴϸ� ����
        if (!MapLoader.Instance || MapLoader.Instance.currentChunk != Owner) return;

        // �÷��̾� Transform ����
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
