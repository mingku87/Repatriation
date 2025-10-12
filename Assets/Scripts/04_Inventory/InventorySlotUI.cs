using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Header("Slot Visuals")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private GameObject durabilityRoot;
    [SerializeField] private Image durabilityFill;

    [Header("Locked Slot")]
    [SerializeField] private Sprite lockedSprite;

    [Header("Drag Ghost (Local Image)")]
    [SerializeField] private Image dragGhost;

    [Tooltip("이 슬롯이 가리키는 인벤토리 인덱스")]
    public int index;

    // 드래그 상태(정적: 한 번에 하나의 슬롯만 드래그)
    private static int sDraggingFrom = -1;
    private static InventorySlotUI sDraggingSlot;

    private static readonly Color kNormal = Color.white;
    private static readonly Color kDragging = new Color(1f, 1f, 1f, 0.4f);

    private void Awake()
    {
        Clear();
    }

    private void OnEnable()
    {
        // 인벤토리 현재 해금 칸 수로 잠김 여부를 바로 계산
        var inv = InventoryController.Instance?.inventory;

        // 인벤이 아직 없거나, 이 슬롯이 해금 범위 바깥이면 → 잠금 스프라이트 표시
        if (inv == null || index >= inv.ActiveSlotCount)
        {
            if (icon) { icon.sprite = lockedSprite; icon.enabled = (lockedSprite != null); icon.color = Color.white; }
            if (countText) countText.gameObject.SetActive(false);
            if (durabilityRoot) durabilityRoot.SetActive(false);
            if (dragGhost) dragGhost.gameObject.SetActive(false);
            return;
        }

        // 해금된 칸이면 일단 클리어(빈칸) → 이후 UI 매니저에서 Refresh로 실제 아이템/빈칸 갱신
        Clear();
    }

    private void OnDisable()
    {
        Clear();
        // 정리
        if (sDraggingSlot == this)
        {
            sDraggingFrom = -1;
            sDraggingSlot = null;
        }
    }

    public void Clear()
    {
        if (icon) { icon.sprite = null; icon.enabled = false; icon.color = kNormal; }
        if (countText) countText.gameObject.SetActive(false);
        UpdateDurabilityGauge(visible: false, ratio: 1f);
        if (dragGhost) dragGhost.gameObject.SetActive(false);
    }

    public void Set(Inventory.SlotView v)
    {
        var inv = InventoryController.Instance?.inventory;
        bool locked = (inv != null && index >= inv.ActiveSlotCount);

        // 잠김
        if (locked)
        {
            if (icon) { icon.sprite = lockedSprite; icon.enabled = true; icon.color = kNormal; }
            if (countText) countText.gameObject.SetActive(false);
            if (durabilityRoot) durabilityRoot.SetActive(false);
            return;
        }

        // 빈칸
        if (v.icon == null)
        {
            Clear();
            return;
        }

        // 표시
        if (icon) { icon.sprite = v.icon; icon.enabled = true; icon.color = kNormal; }

        if (countText)
        {
            countText.gameObject.SetActive(v.showCount);
            if (v.showCount) countText.text = v.count.ToString();
        }

        float ratio = Mathf.Clamp01(v.durability01);
        UpdateDurabilityGauge(v.showDurability && ratio < 1f, ratio);
    }

    // ───────── Drag & Drop ─────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (index < 0)
        {
            Debug.Log($"[InventorySlotUI] index < 0 무시됨 (index: {index})");
            return;
        }

        if (eventData.pointerDrag != gameObject || eventData.pointerPress != gameObject)
        {
            Debug.Log($"[InventorySlotUI:{index}] 드래그 무시됨 (내가 드래그 대상 아님)");
            return;
        }

        var inv = InventoryController.Instance?.inventory;
        if (inv == null || !inv.IsValidIndex(index)) return;
        if (index >= inv.ActiveSlotCount) return;

        var stack = inv.slots[index];
        if (stack.item == null)
        {
            Debug.Log($"[InventorySlotUI:{index}] 아이템 없음 → 드래그 안 함");
            return;
        }

        // ⬇ 드래그 정상 시작
        Debug.Log($"[InventorySlotUI:{index}] 정상적으로 드래그 시작");

        var v = inv.GetView(index);

        Sprite sp =
            stack.item?.info?.image ??
            v.icon ??
            icon?.sprite;

        if (sp == null) return;

        sDraggingFrom = index;
        sDraggingSlot = this;

        InventoryDragHandler.Instance?.BeginDrag(sp, index, eventData.position);
        DragContext.BeginFromInventory(index);

        if (dragGhost != null)
        {
            dragGhost.sprite = sp;
            dragGhost.color = Color.white;
            dragGhost.raycastTarget = false;
            dragGhost.gameObject.SetActive(true);
            dragGhost.transform.SetAsLastSibling();
            dragGhost.transform.position = eventData.position;
        }

        if (icon) icon.color = kDragging;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhost != null && dragGhost.gameObject.activeSelf)
            dragGhost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        sDraggingFrom = -1;

        DragContext.End();
        InventoryDragHandler.Instance?.EndDrag();

        if (dragGhost != null)
        {
            dragGhost.gameObject.SetActive(false);
            dragGhost.sprite = null;
        }

        if (sDraggingSlot != null && sDraggingSlot.icon != null)
            sDraggingSlot.icon.color = kNormal;

        sDraggingSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var drag = DragContext.Current;

        Debug.Log($"[OnDrop] called. drag.source={drag?.source}, equipSlot={drag?.equipSlot}");

        if (drag != null && drag.source == DragSource.Equipment)
        {
            var ctrl = InventoryController.Instance;
            var inv = ctrl?.inventory;
            var eqModel = ctrl?.equipment;

            Debug.Log($"[OnDrop] UnequipToInventoryAt → index: {index}");

            if (inv != null && eqModel != null && index < inv.ActiveSlotCount)
            {
                // 🔥 이제 EquipmentSlot 버전 사용
                if (!eqModel.UnequipToInventoryAt(drag.equipSlot, index, out string reason))
                    Debug.LogWarning($"[OnDrop] Unequip failed: {reason}");
                else
                    Debug.Log($"[OnDrop] Unequip 성공 → 슬롯 {index}");
            }

            DragContext.End();
            InventoryDragHandler.Instance?.EndDrag();
            OnEndDrag(eventData);
            return;
        }

        // 인벤 → 인벤: Move/Swap
        if (sDraggingFrom < 0) return;

        var inv2 = InventoryController.Instance?.inventory;
        if (inv2 == null) return;
        if (index >= inv2.ActiveSlotCount) return;

        int from = sDraggingFrom;
        int to = index;

        if (from != to && inv2.IsValidIndex(from) && inv2.IsValidIndex(to))
            inv2.MoveOrSwap(from, to);

        OnEndDrag(eventData);
    }

    // 우클릭 → 자동 장착/교체
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;

        var inv = InventoryController.Instance?.inventory;
        var equipModel = InventoryController.Instance?.equipment;
        if (inv == null || equipModel == null) return;
        if (!inv.IsValidIndex(index)) return;

        var stack = inv.slots[index];
        if (stack.IsEmpty || stack.item is not ItemEquipment) return;

        if (!equipModel.EquipFromInventoryAuto(index, out string reason))
            Debug.Log($"[InventorySlotUI] Auto-equip failed: {reason}");
        else
        {
            // ✅ 장비 UI 수동 Refresh
            InventoryController.Instance.equipmentUI.Refresh();
        }
    }

    // Discard 등 외부에서 드래그 강제 종료
    public static void CancelExternalDrag()
    {
        if (sDraggingSlot != null)
        {
            if (sDraggingSlot.dragGhost != null)
            {
                sDraggingSlot.dragGhost.gameObject.SetActive(false);
                sDraggingSlot.dragGhost.sprite = null;
            }
            if (sDraggingSlot.icon != null)
                sDraggingSlot.icon.color = kNormal;
        }
        sDraggingFrom = -1;
        sDraggingSlot = null;
    }

    private void UpdateDurabilityGauge(bool visible, float ratio)
    {
        if (!durabilityRoot && !durabilityFill)
            return;

        if (durabilityRoot)
            durabilityRoot.SetActive(visible);

        if (!durabilityFill)
            return;

        EnsureHorizontalFill(durabilityFill);

        durabilityFill.gameObject.SetActive(visible);

        if (!visible)
        {
            durabilityFill.enabled = false;
            durabilityFill.fillAmount = 1f;
            durabilityFill.color = DurabilityColorUtility.GetColor(1f);
            return;
        }

        durabilityFill.enabled = true;
        durabilityFill.fillAmount = Mathf.Clamp01(ratio);
        durabilityFill.color = DurabilityColorUtility.GetColor(durabilityFill.fillAmount);
    }

    private static void EnsureHorizontalFill(Image fill)
    {
        if (fill == null)
            return;

        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
    }
}
