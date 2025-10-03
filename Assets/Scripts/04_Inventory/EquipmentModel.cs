using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentModel
{
    private readonly Dictionary<EquipmentSlot, ItemEquipment> equipped = new();

    public event Action OnChanged;
    private void Notify() => OnChanged?.Invoke();

    public ItemEquipment Get(EquipmentSlot slot)
        => equipped.TryGetValue(slot, out var it) ? it : null;

    public bool IsEquipped(EquipmentSlot slot) => Get(slot) != null;

    public bool TryEquip(EquipmentSlot slot, ItemEquipment item, out ItemEquipment previous)
    {
        previous = null;
        if (item == null) return false;

        // 부위 호환성 체크
        if (!InventoryConstant.AllowedEquipmentSlotsPerPart.TryGetValue(item.param.equipPart, out var list)
            || !list.Contains(slot))
            return false;

        // 기존 장비 있으면 반환용으로 확보
        if (equipped.TryGetValue(slot, out var old))
            previous = old;

        // 장착 갱신
        equipped[slot] = item;

        // 효과 적용: 너희 ItemEquipment가 처리하도록 위임
        previous?.UnEquip();
        item.Equip();

        Notify();
        return true;
    }

    public ItemEquipment Unequip(EquipmentSlot slot)
    {
        if (!equipped.TryGetValue(slot, out var it)) return null;

        equipped.Remove(slot);

        // 효과 회수
        it.UnEquip();

        Notify();
        return it;
    }

    // ─ UI 바인딩용 뷰 ─
    public EquipView GetView(EquipmentSlot slot)
    {
        var it = Get(slot);
        if (it == null) return default;

        // 네 프로젝트: ItemInfo.name 이 표시 이름
        string display = it.info?.name ?? it.param.itemName.ToString();

        bool showDur = (it.param.maxDurability > 0) && (it.durability < it.param.maxDurability);
        float dur01 = 0f;
        if (showDur)
            dur01 = Mathf.Clamp01((float)it.durability / Mathf.Max(1, it.param.maxDurability));

        return new EquipView
        {
            icon = it.info?.image,
            displayName = display,
            showDurability = showDur,
            durability01 = dur01,
        };
    }

    public struct EquipView
    {
        public Sprite icon;
        public string displayName;
        public bool showDurability;
        public float durability01; // 0..1
    }
}
