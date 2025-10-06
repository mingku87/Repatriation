using UnityEngine;

public class EquipmentUIManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EquipmentModel equipmentModel;   // 인스펙터에서 할당
    [SerializeField] private EquipmentSlotUI[] slots;         // 인스펙터에서 슬롯들 할당

    private void Awake()
    {
        if (equipmentModel == null)
            equipmentModel = new EquipmentModel();
    }

    private void Start()
    {
        BindAll();
        RefreshAll();
    }

    public void BindAll()
    {
        var inv = InventoryController.Instance?.inventory;

        var partSlotUsage = new System.Collections.Generic.Dictionary<EquipmentPart, int>();

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            // 슬롯 이름 → EquipmentSlot 자동 추론
            var part = slots[i].defaultPart;
            var configuredSlot = slots[i].InspectorSlot;
            if (!InventoryConstant.AllowedEquipmentSlotsPerPart.TryGetValue(part, out var allowed) || allowed == null || allowed.Count == 0)
            {
                Debug.LogWarning($"[EquipmentUIManager] {part} 파트에 대한 허용 슬롯이 정의되지 않았습니다.");
                configuredSlot = EquipmentSlot.Body;
            }
            else if (!allowed.Contains(configuredSlot))
            {
                if (slots[i].TryGetSlotFromObjectName(out var resolvedFromName) && allowed.Contains(resolvedFromName))
                {
                    configuredSlot = resolvedFromName;
                }
                else
                {
                    var usedCount = partSlotUsage.TryGetValue(part, out var count) ? count : 0;
                    configuredSlot = allowed[Mathf.Clamp(usedCount, 0, allowed.Count - 1)];
                }
            }

            partSlotUsage[part] = partSlotUsage.TryGetValue(part, out var existing) ? existing + 1 : 1;

            slots[i].Bind(equipmentModel, part, configuredSlot, inv);
        }
    }

    private bool TryResolveSlotFromPart(EquipmentPart part, out EquipmentSlot slot)
    {
        slot = default;
        if (!InventoryConstant.AllowedEquipmentSlotsPerPart.TryGetValue(part, out var allowed) || allowed == null || allowed.Count == 0)
            return false;

        slot = allowed[0];
        return true;
    }

    public void RefreshAll()
    {
        foreach (var s in slots)
            if (s != null) s.Refresh();
    }

    public void Initialize()
    {
        if (equipmentModel == null)
            equipmentModel = new EquipmentModel();
        BindAll();
    }

    // ✅ slotOrder 제거: 더 이상 필요 없음
    public void Initialize(EquipmentModel model)
    {
        equipmentModel = model ?? equipmentModel ?? new EquipmentModel();
        BindAll();
    }

    public void Refresh()
    {
        RefreshAll();
    }
}
