using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DoorPortal : MonoBehaviour
{
    [Header("Door ID Linking")]
    [Tooltip("이 문을 고유하게 식별하는 ID (다음 맵의 같은 ID 문과 연결됨)")]
    public string doorId;

    [Header("Door Settings")]
    public ExitDir direction = ExitDir.RightUp;
    [Tooltip("문이 쏘는 레이 각도 (도 단위, 0=우측, 90=상, 180=좌, 270=하)")]
    public float customRayAngle = 0f;
    [Tooltip("문이 쏘는 레이 거리 (m)")]
    public float rayDistance = 1f;

    [Header("Map Linking")]
    [Tooltip("이 문을 통해 들어갈 다음 맵 프리팹")]
    public GameObject nextMapPrefab;
    [Tooltip("다음 맵에서 진입할 포탈 방향 (필요시)")]
    public ExitDir entryDirectionOnNext = ExitDir.LeftDown;

    [Header("References")]
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
        if (!other.CompareTag("Player")) return;
        if (!MapLoader.Instance || MapLoader.Instance.currentChunk != Owner) return;

        var who = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;
        MapLoader.Instance.TryDoorRayTravel(this, who);
    }
}
