using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Header("Refs")]
    public EquipmentModel equipmentModel;     // Bind에서 주입 (없으면 그대로 둬도 됨)
    public Inventory inventory;               // Bind에서 주입 (없으면 컨트롤러에서 가져옴)

    [Header("Slot Part")]
    [Tooltip("매니저에서 슬롯 부위를 전달하지 않을 때 사용할 기본값")]
    public EquipmentPart defaultPart = EquipmentPart.Body;

    [Tooltip("인스펙터에서 설정하는 슬롯 종류 (예: LeftArm, RightHand 등)")]
    [SerializeField]
    private EquipmentSlot slotType = EquipmentSlot.Body;

    // 런타임에 실제로 사용할 부위 & 슬롯
    private EquipmentPart currentPart;
    private EquipmentSlot currentSlot;

    [Header("UI")]
    public Image icon;
    public GameObject durabilityRoot;
    public Image durabilityFill;
    public Image highlightFrame;

    [Header("Options")]
    public bool showDurability = false;
    public EquipmentPart Part => currentPart;
    public EquipmentSlot slot;

    public EquipmentSlot InspectorSlot => slotType;

    public bool TryGetSlotFromObjectName(out EquipmentSlot resolved)
    {
        var objectName = gameObject != null ? gameObject.name : string.Empty;
        if (string.IsNullOrEmpty(objectName))
        {
            resolved = default;
            return false;
        }

        objectName = objectName.Replace("Slot_", string.Empty);
        return Enum.TryParse(objectName, out resolved);
    }
    // 이벤트 구독 핸들러 보관(중복 구독 방지)
    Action _modelChangedHandler;
    Action _invChangedHandler;

    // ===== Unity lifecycle =====
    void Awake()
    {
        currentPart = defaultPart;
        currentSlot = slotType;

        if (icon) icon.enabled = false;
        if (durabilityRoot) durabilityRoot.SetActive(false);
        if (highlightFrame) highlightFrame.enabled = false;
    }

    void OnEnable()
    {
        if (inventory == null)
            inventory = InventoryController.Instance?.inventory;

        Subscribe();
        TryRefresh();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    // ===== Public API =====

    /// <summary>모델/부위만 바인딩 (슬롯은 인스펙터 값 사용)</summary>
    public void Bind(EquipmentModel model, EquipmentPart part, Inventory inv = null)
    {
        Bind(model, part, slotType, inv);
    }

    /// <summary>모델/부위/슬롯을 모두 명시적으로 바인딩</summary>
    public void Bind(EquipmentModel model, EquipmentPart part, EquipmentSlot slot, Inventory inv = null)
    {
        Unsubscribe();

        equipmentModel = model ?? equipmentModel;
        currentPart = part;
        currentSlot = slot;
        inventory = inv ?? inventory ?? InventoryController.Instance?.inventory;

        Subscribe();
        TryRefresh();
    }

    public void Refresh() => TryRefresh();

    public void CancelExternalDrag()
    {
        DragContext.End();
        if (highlightFrame) highlightFrame.enabled = false;
    }

    // ===== Drag & Drop =====
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.pointerDrag != gameObject)
        {
            Debug.Log("[EquipmentSlotUI] OnBeginDrag 무시됨 (pointerDrag != this)");
            return;
        }

        // ✅ currentSlot 사용
        DragContext.BeginFromEquipment(currentSlot);

        if (highlightFrame) highlightFrame.enabled = true;

        if (icon != null)
        {
            var c = icon.color;
            c.a = 0.4f;
            icon.color = c;
        }

        var item = equipmentModel?.Get(currentSlot);
        if (item != null)
        {
            var sprite = item.info?.image;
            InventoryDragHandler.Instance?.BeginDrag(sprite, -1);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // optional
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragContext.End();
        if (highlightFrame) highlightFrame.enabled = false;

        if (icon != null)
        {
            var c = icon.color;
            c.a = 1f;
            icon.color = c;
        }

        InventoryDragHandler.Instance?.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        var drag = DragContext.Current;
        if (drag == null || drag.source != DragSource.Inventory)
            return;

        var inv = InventoryController.Instance?.inventory;
        var equip = InventoryController.Instance?.equipment;
        if (inv == null || equip == null) return;
        if (!inv.IsValidIndex(drag.inventoryIndex)) return;

        var item = inv.PeekAt(drag.inventoryIndex);
        var pres = ItemPresentationDB.Get(item.id); 
        var enumName = item?.param?.itemName.ToString() ?? "(이름 없음)";
        Debug.Log($"[EquipmentSlotUI] 장착 시도: {enumName}, 슬롯: {slot}");

        Debug.Log($"🟦 [OnDrop] 드래그: {enumName} (id: {item.id}), 대상 슬롯: {currentSlot}");

        if (equip.EquipFromInventory(drag.inventoryIndex, currentSlot, out string reason))
        {
            Debug.Log($"✅ [OnDrop] 장착 성공 → {currentSlot} ← {enumName}");
            inv.RaiseChanged();
            Refresh(); // 슬롯 UI 갱신
        }
        else
        {
            Debug.LogWarning($"❌ [OnDrop] 장착 실패: {reason} | 아이템: {enumName}, 슬롯: {currentSlot}");
        }

        DragContext.End();
        InventoryDragHandler.Instance?.EndDrag();
    }


    // ===== 우클릭 → 해제 =====
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (equipmentModel == null) return;
        if (inventory == null)
            inventory = InventoryController.Instance?.inventory;
        if (inventory == null) return;

        int empty = -1;
        for (int i = 0; i < inventory.ActiveSlotCount; i++)
        {
            if (inventory.slots[i].IsEmpty) { empty = i; break; }
        }

        if (empty < 0)
        {
            Debug.LogWarning("[EquipmentSlotUI] Unequip failed: inventory full");
            return;
        }

        bool ok = equipmentModel.UnequipToInventoryAt(currentSlot, empty, out string reason);
        if (!ok) Debug.LogWarning($"[EquipmentSlotUI] Unequip fail: {reason}");
        TryRefresh();
    }

    // ===== Internal =====
    void Subscribe()
    {
        if (equipmentModel != null)
        {
            _modelChangedHandler ??= TryRefresh;
            equipmentModel.OnChanged -= _modelChangedHandler;
            equipmentModel.OnChanged += _modelChangedHandler;
        }

        if (inventory != null)
        {
            _invChangedHandler ??= TryRefresh;
            inventory.OnChanged -= _invChangedHandler;
            inventory.OnChanged += _invChangedHandler;
        }
    }

    void Unsubscribe()
    {
        if (equipmentModel != null && _modelChangedHandler != null)
            equipmentModel.OnChanged -= _modelChangedHandler;
        if (inventory != null && _invChangedHandler != null)
            inventory.OnChanged -= _invChangedHandler;
    }

    public void TryRefresh()
    {
        if (equipmentModel == null)
        {
            if (icon) { icon.sprite = null; icon.enabled = false; }
            if (durabilityRoot) durabilityRoot.SetActive(false);
            return;
        }

        var item = equipmentModel.Get(currentSlot);
        if (item == null)
        {
            if (icon) { icon.sprite = null; icon.enabled = false; }
            if (durabilityRoot) durabilityRoot.SetActive(false);
            return;
        }

        if (icon)
        {
            icon.sprite = item.info != null ? item.info.image : null;
            icon.enabled = icon.sprite != null;
        }

        if (showDurability && durabilityRoot && durabilityFill)
        {
            var eq = item as ItemEquipment;
            if (eq != null && eq.param != null && eq.param.maxDurability > 0 && eq.durability < eq.param.maxDurability)
            {
                float v = Mathf.Clamp01((float)eq.durability / Mathf.Max(1, eq.param.maxDurability));
                durabilityRoot.SetActive(true);
                durabilityFill.fillAmount = v;
            }
            else
            {
                durabilityRoot.SetActive(false);
            }
        }
    }
}
