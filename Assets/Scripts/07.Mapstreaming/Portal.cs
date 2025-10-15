using UnityEngine;

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
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        if (!anchor) anchor = transform;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // �浹�� ��ü�� ��Ʈ GameObject�� �������� (Rigidbody2D�� ������ �װ� ����)
        var who = other.attachedRigidbody ? other.attachedRigidbody.gameObject
                                          : other.transform.root.gameObject;

        // Player�� �ƴϸ� ����
        if (!who.CompareTag("Player")) return;

        // ���� ���� ��Ż�� ���� (�ٸ� �� �ܿ��� ����)
        if (MapLoader.Instance && MapLoader.Instance.currentChunk != Owner) return;

        // ���� ��� �̵� ����
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
