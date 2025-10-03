// InventorySlotUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Slot Visuals")]
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI countText;
    [SerializeField] GameObject durabilityRoot;
    [SerializeField] Image durabilityFill;

    [Header("Locked Slot")]
    [SerializeField] Sprite lockedSprite;   // 🔒 잠긴 상태에 보여줄 스프라이트

    [Header("Drag Ghost (UI 이미지)")]
    [SerializeField] Image dragGhost;       // 마우스 따라다니는 아이콘

    [Tooltip("이 슬롯이 가리키는 인벤토리 인덱스")]
    public int index;

    private static int sDraggingFrom = -1;
    private static InventorySlotUI sDraggingSlot;

    private Color normalColor = Color.white;
    private Color draggingColor = new Color(1f, 1f, 1f, 0.4f);

    public void Set(Inventory.SlotView v)
    {
        var inv = InventoryController.Instance?.inventory;
        bool locked = (inv != null && index >= inv.ActiveSlotCount);

        if (locked)
        {
            // 잠긴 칸 → 잠금 스프라이트만 표시
            if (icon != null)
            {
                icon.sprite = lockedSprite;
                icon.enabled = true;
                icon.color = Color.white;
            }
            if (countText != null) countText.gameObject.SetActive(false);
            if (durabilityRoot != null) durabilityRoot.SetActive(false);
            return;
        }

        // ── 일반 칸 표시 ──
        if (icon != null)
        {
            icon.sprite = v.icon;
            icon.enabled = v.icon != null;
            if (v.icon != null) icon.color = normalColor;
        }

        if (countText != null)
        {
            countText.gameObject.SetActive(v.showCount);
            if (v.showCount) countText.text = v.count.ToString();
        }

        if (durabilityRoot != null) durabilityRoot.SetActive(v.showDurability);
        if (v.showDurability && durabilityFill != null)
            durabilityFill.fillAmount = Mathf.Clamp01(v.durability01);
    }

    public void Clear()
    {
        if (icon != null) { icon.sprite = null; icon.enabled = false; }
        if (countText != null) countText.gameObject.SetActive(false);
        if (durabilityRoot != null) durabilityRoot.SetActive(false);
    }

    // ───────── Drag & Drop ─────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        var inv = InventoryController.Instance?.inventory;
        if (inv == null || !inv.IsValidIndex(index)) return;

        // 잠긴 칸은 드래그 불가
        if (index >= inv.ActiveSlotCount) return;

        var v = inv.GetView(index);
        if (v.icon == null) return;

        sDraggingFrom = index;
        sDraggingSlot = this;

        if (dragGhost != null)
        {
            dragGhost.sprite = v.icon;
            dragGhost.color = Color.white;
            dragGhost.raycastTarget = false;
            dragGhost.gameObject.SetActive(true);
            dragGhost.transform.position = eventData.position;
        }

        if (icon != null) icon.color = draggingColor;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhost != null && dragGhost.gameObject.activeSelf)
            dragGhost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        sDraggingFrom = -1;

        if (dragGhost != null)
        {
            dragGhost.gameObject.SetActive(false);
            dragGhost.sprite = null;
        }

        if (sDraggingSlot != null && sDraggingSlot.icon != null)
            sDraggingSlot.icon.color = normalColor;

        sDraggingSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (sDraggingFrom < 0) return;

        var inv = InventoryController.Instance?.inventory;
        if (inv == null) return;

        // 잠긴 칸에는 드롭 불가
        if (index >= inv.ActiveSlotCount) return;

        int from = sDraggingFrom;
        int to = index;

        if (from != to && inv.IsValidIndex(from) && inv.IsValidIndex(to))
        {
            inv.MoveOrSwap(from, to);
        }

        OnEndDrag(eventData);
    }
}
