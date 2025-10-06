using UnityEngine;
using UnityEngine.UI;

public class InventoryDragHandler : MonoBehaviour
{
    public static InventoryDragHandler Instance { get; private set; }

    [SerializeField] private Image dragIcon;

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
    }

    public void BeginDrag(Sprite sprite, int fromIndex)
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

        dragIcon.rectTransform.position = Input.mousePosition;
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
            dragIcon.rectTransform.position = Input.mousePosition;
    }
}
