using UnityEngine;
using UnityEngine.UI;

public class InventoryDragHandler : MonoBehaviour
{
    public static InventoryDragHandler Instance { get; private set; }

    [SerializeField] private Image dragIcon;
    [SerializeField] private Canvas dragCanvas;

    bool active;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (dragIcon != null)
        {
            dragIcon.enabled = false;
            dragIcon.raycastTarget = false;
        }

        if (dragCanvas == null && dragIcon != null)
            dragCanvas = dragIcon.canvas;
    }

    public void BeginDrag(Sprite sprite, int fromIndex)
    {
        BeginDrag(sprite, fromIndex, Input.mousePosition);
    }

    public void BeginDrag(Sprite sprite, int fromIndex, Vector2 pointerPosition)
    {

        if (dragIcon == null || sprite == null)
        {
            active = false;
            return;
        }

        dragIcon.sprite = sprite;
        dragIcon.enabled = true;
        dragIcon.raycastTarget = false;
        dragIcon.gameObject.SetActive(true);
        dragIcon.transform.SetAsLastSibling();

        UpdateIconPosition(pointerPosition);
        active = true;
    }

    public void EndDrag()
    {
        active = false;

        if (dragIcon != null)
        {
            dragIcon.gameObject.SetActive(false);
            dragIcon.sprite = null;
        }
    }

    void Update()
    {
        if (active && dragIcon != null)
            UpdateIconPosition(Input.mousePosition);
    }

    void UpdateIconPosition(Vector2 screenPosition)
    {
        if (dragIcon == null)
            return;

        if (dragCanvas == null)
            dragCanvas = dragIcon.canvas;

        if (dragCanvas != null && dragCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    dragCanvas.transform as RectTransform,
                    screenPosition,
                    dragCanvas.worldCamera,
                    out Vector3 worldPos))
            {
                dragIcon.rectTransform.position = worldPos;
            }
        }
        else
        {
            dragIcon.rectTransform.position = screenPosition;
        }
    }
}
