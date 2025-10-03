using UnityEngine;
using UnityEngine.UI;

public class InventoryDragHandler : MonoBehaviour
{
    public static InventoryDragHandler Instance;

    [SerializeField] private Image dragIcon;   // ���콺 ����ٴ� ������
    private Canvas canvas;

    public int fromIndex = -1;  // �巡�� ������ �κ��丮 �ε���

    void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
        dragIcon.gameObject.SetActive(false);
    }

    void Update()
    {
        if (dragIcon.gameObject.activeSelf)
            dragIcon.transform.position = Input.mousePosition;
    }

    public void BeginDrag(Sprite icon, int index)
    {
        if (icon == null) return;
        fromIndex = index;
        dragIcon.sprite = icon;
        dragIcon.gameObject.SetActive(true);
    }

    public void EndDrag()
    {
        fromIndex = -1;
        dragIcon.sprite = null;
        dragIcon.gameObject.SetActive(false);
    }
}
