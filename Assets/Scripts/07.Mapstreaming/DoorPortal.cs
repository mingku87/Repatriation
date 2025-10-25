using System.Collections;
using UnityEngine;

public enum DoorQuadrant
{
    RightUp,
    RightDown,
    LeftUp,
    LeftDown
}

[RequireComponent(typeof(Collider2D))]
public class DoorPortal : MonoBehaviour
{
    private const string TAG = "[Door]";

    [Header("Door Linking by ID")]
    [Tooltip("이 문을 식별하는 고유 ID (맵 프리팹 내에서 유일)")]
    public string doorId;

    [Tooltip("도착할 문 ID (상대 맵 프리팹 안의 문 doorId)")]
    public string targetDoorId;

    [Tooltip("도착 문이 들어있는 맵 프리팹 (현재 씬에 없으면 Instantiate)")]
    public GameObject targetMapPrefab;

    [Header("1-Step 이동 설정")]
    public DoorQuadrant stepQuadrant = DoorQuadrant.RightUp;
    public float angleOffsetDeg = 0f;
    public float stepDistance = 1.0f;

    [Header("Anchors/Owner")]
    public Transform anchor;
    public MapChunk Owner;

    [Header("입력/쿨타임")]
    public bool useKey = true;
    public KeyCode interactKey = KeyCode.F;
    public float localCooldown = 0.2f;

    [Header("Debug")]
    public bool verboseLogs = true;

    bool inTrigger;
    float cooldownUntil = -1f;

    void Reset()
    {
        var c2d = GetComponent<Collider2D>();
        c2d.isTrigger = true;
        if (!anchor) anchor = transform;
        if (!Owner) Owner = GetComponentInParent<MapChunk>();
    }

    void Awake()
    {
        if (!anchor) anchor = transform;
        if (!Owner) Owner = GetComponentInParent<MapChunk>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        inTrigger = true;
        if (!useKey) TryActivate(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (useKey && Input.GetKeyDown(interactKey)) TryActivate(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        inTrigger = false;
    }

    void TryActivate(Collider2D other)
    {
        if (Time.unscaledTime < cooldownUntil) return;
        cooldownUntil = Time.unscaledTime + localCooldown;
        if (!MapLoader.Instance) return;

        if (MapLoader.Instance.currentChunk != Owner)
        {
            if (verboseLogs)
                Debug.LogWarning($"{TAG} blocked: currentChunk({MapLoader.Instance.currentChunk?.name}) != Owner({Owner?.name})");
            return;
        }

        if (string.IsNullOrEmpty(targetDoorId))
        {
            Debug.LogError($"{TAG} Missing targetDoorId on {name}");
            return;
        }

        float angle = GetFinalAngleDeg();
        if (verboseLogs)
            Debug.Log($"{TAG} Enter door {name} → targetDoorId={targetDoorId} angle={angle:F1}");

        MapLoader.Instance.TryDoorById(this, targetDoorId, targetMapPrefab, angle);
    }

    public float GetFinalAngleDeg()
    {
        float baseDeg = stepQuadrant switch
        {
            DoorQuadrant.RightUp => 45f,
            DoorQuadrant.RightDown => -45f,
            DoorQuadrant.LeftUp => 135f,
            _ => -135f,
        };
        return baseDeg + angleOffsetDeg;
    }
}
