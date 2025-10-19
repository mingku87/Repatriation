using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public Transform player;
    public LayerMask interactableMask;
    public float radiusX = 2.0f;  // 좌우 범위
    public float radiusY = 1.2f;  // 상하 범위
    public int maxHits = 24;
    public string[] validTags = { "NPC", "Button", "Box", "Door" };

    Camera _cam;
    Collider2D[] _hits;

    void Awake()
    {
        _cam = Camera.main;
        _hits = new Collider2D[maxHits];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            TryEllipticalInteract();

        if (Input.GetMouseButtonDown(0))
            TryClickInteract();
    }

    void TryEllipticalInteract()
    {
        if (player == null) return;

        // 대략적인 후보만 원형으로 가져오고, 실제로는 타원으로 필터
        int count = Physics2D.OverlapCircleNonAlloc(
            player.position, Mathf.Max(radiusX, radiusY), _hits,
            interactableMask.value == 0 ? Physics2D.AllLayers : interactableMask
        );

        Collider2D bestCol = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var col = _hits[i];
            if (col == null) continue;
            if (!IsValidTag(col.tag)) continue;

            Vector2 delta = col.bounds.center - player.position;
            // 타원 거리 정규화
            float nx = delta.x / radiusX;
            float ny = delta.y / radiusY;
            float ellipDistSq = nx * nx + ny * ny;

            // 타원 내부인지 확인 (1 이하)
            if (ellipDistSq > 1f) continue;

            if (ellipDistSq < bestScore)
            {
                bestScore = ellipDistSq;
                bestCol = col;
            }
        }

        if (bestCol != null)
            DispatchByTag(bestCol, player);
    }

    void TryClickInteract()
    {
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        Vector3 mw = _cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p = new Vector2(mw.x, mw.y);

        var col = Physics2D.OverlapPoint(p, interactableMask.value == 0 ? Physics2D.AllLayers : interactableMask);
        if (col == null) return;
        if (!IsValidTag(col.tag)) return;

        DispatchByTag(col, player);
    }

    bool IsValidTag(string tag)
    {
        for (int i = 0; i < validTags.Length; i++)
            if (tag == validTags[i]) return true;
        return false;
    }

    void DispatchByTag(Collider2D target, Transform interactor)
    {
        switch (target.tag)
        {
            case "NPC":
                target.GetComponentInParent<NPCInteractable>()?.Interact(interactor);
                break;
            case "Button":
                target.GetComponentInParent<ButtonInteractable>()?.Interact(interactor);
                break;
            case "Box":
                target.GetComponentInParent<BoxInteractable>()?.Interact(interactor);
                break;
            case "Door":
                target.GetComponentInParent<DoorInteractable>()?.Interact(interactor);
                break;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.cyan;

        // 타원 모양 Gizmo
        int steps = 40;
        Vector3 prev = Vector3.zero;
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps * Mathf.PI * 2f;
            Vector3 pos = player.position + new Vector3(Mathf.Cos(t) * radiusX, Mathf.Sin(t) * radiusY, 0);
            if (i > 0) Gizmos.DrawLine(prev, pos);
            prev = pos;
        }
    }
#endif
}
