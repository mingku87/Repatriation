using System;
using System.Collections.Generic;
using UnityEngine;

// 프로젝트에 이미 존재하는 enum으로 가정
// public enum EquipmentPart { Head, Body, Hands, Arms, Knees, Feet, Bag }

public class EquipmentModel
{
    private readonly Dictionary<EquipmentSlot, Item> _equippedBySlot = new Dictionary<EquipmentSlot, Item>();
    public event Action OnChanged;

    public Item Get(EquipmentPart part)
    {
        if (!TryGetAllowedSlots(part, out var slots))
            return null;

        foreach (var slot in slots)
        {
            var item = Get(slot);
            if (item != null)
                return item;
        }

        return null;
    }

    public Item Get(EquipmentSlot slot)
    {
        _equippedBySlot.TryGetValue(slot, out var it);
        return it;
    }

    public IEnumerable<Item> EnumerateEquipped()
    {
        foreach (var kv in _equippedBySlot)
            if (kv.Value != null)
                yield return kv.Value;
    }

    private void RaiseChanged()
    {
        try { OnChanged?.Invoke(); }
        catch (Exception e) { Debug.LogException(e); }
    }

    // ------------------------------------------------------------------
    //  Equipping
    // ------------------------------------------------------------------

    // 기존: Part 기준
    public bool EquipFromInventory(Guid guid, EquipmentPart part, out string reason)
    {
        reason = null;
        var inv = InventoryController.Instance?.inventory;
        if (inv == null) { reason = "inventory null"; return false; }

        if (!inv.TryFindByGuid(guid, out var idx, out var item))
        {
            reason = "item moved/invalid guid";
            return false;
        }
        return EquipFromInventory(idx, part, out reason);
    }

    public bool EquipFromInventory(int inventoryIndex, EquipmentPart part, out string reason)
    {
        reason = null;
        var inv = InventoryController.Instance?.inventory;
        if (inv == null) { reason = "inventory null"; return false; }
        if (!inv.IsValidIndex(inventoryIndex)) { reason = "invalid inventory index"; return false; }

        var item = inv.PeekAt(inventoryIndex);
        if (item == null) { reason = "no item at index"; return false; }

        // 타입 검사 (프로젝트 규칙에 맞게 조정)
        var eqParam = item.param as ItemParameterEquipment;
        if (eqParam == null)
        {
            reason = "not an equipment";
            return false;
        }

        item.param.equipment = eqParam;

        if (eqParam.equipPart != part)
        {
            reason = $"이 장비는 {eqParam.equipPart}용입니다. 이 슬롯({part})에는 장착할 수 없습니다.";
            return false;
        }

        if (!TryGetAllowedSlots(part, out var slots) || slots.Count == 0)
        {
            reason = $"{part} 부위를 위한 슬롯이 정의되지 않았습니다.";
            return false;
        }

        // 파트 기반 호출은 비어 있는 슬롯을 우선 사용하고, 없으면 첫 슬롯을 덮어쓴다.
        EquipmentSlot targetSlot = slots[0];
        foreach (var candidate in slots)
        {
            if (!_equippedBySlot.TryGetValue(candidate, out var equipped) || equipped == null)
            {
                targetSlot = candidate;
                break;
            }
        }

        return EquipFromInventoryInternal(inventoryIndex, targetSlot, item, inv, out reason);
    }

    // 🔥 신규: Slot 기준 장착 (좌우 슬롯 지원)
    public bool EquipFromInventory(int inventoryIndex, EquipmentSlot slot, out string reason)
    {
        reason = null;
        var inv = InventoryController.Instance?.inventory;
        if (inv == null) { reason = "Inventory not found"; return false; }
        if (!inv.IsValidIndex(inventoryIndex)) { reason = "Invalid inventory index"; return false; }

        var item = inv.PeekAt(inventoryIndex);
        if (item == null) { reason = "No item at index"; return false; }

        // 이 아이템이 해당 슬롯에 장착 가능한지 검사
        if (!CanEquip(slot, item))
        {
            reason = "이 슬롯에는 이 아이템을 장착할 수 없습니다.";
            return false;
        }

        var eqParam = item.param as ItemParameterEquipment;
        if (eqParam == null)
        {
            reason = "이 슬롯에는 이 아이템을 장착할 수 없습니다.";
            return false;
        }

        item.param.equipment = eqParam;

        return EquipFromInventoryInternal(inventoryIndex, slot, item, inv, out reason);
    }

    private bool EquipFromInventoryInternal(int inventoryIndex, EquipmentSlot slot, Item item, Inventory inv, out string reason)
    {
        reason = null;

        // 스왑 대상
        _equippedBySlot.TryGetValue(slot, out var prevEquipped);

        // 인벤토리 슬롯 비우기 (먼저 제거)
        inv.TakeAt(inventoryIndex);

        // 장착
        _equippedBySlot[slot] = item;

        // 스왑 아이템을 인벤토리에 되돌리기
        if (prevEquipped != null)
        {
            if (inv.IsValidIndex(inventoryIndex) && inv.PeekAt(inventoryIndex) == null)
                inv.SetAt(inventoryIndex, prevEquipped);
            else
                inv.Add(prevEquipped);
        }

        // 장비 보너스 재계산(가방 슬롯 등)
        RecomputeBagBonusToInventory();

        RaiseChanged();
        return true;
    }

    private void RecomputeBagBonusToInventory()
    {
        var inv = InventoryController.Instance?.inventory;
        if (inv == null) return;

        int slotBonus = 0;
        float weightBonus = 0f;

        foreach (var kv in _equippedBySlot)
        {
            if (kv.Value == null) continue;
            if (kv.Value.param is not ItemParameterEquipment ep) continue;

            slotBonus += Mathf.Max(0, ep.slotBonus);
            weightBonus += GetWeightBonus(ep.status, ep.value);

            if (ep.extraEffect.HasValue)
            {
                var extra = ep.extraEffect.Value;
                weightBonus += GetWeightBonus(extra.status, extra.value);
            }
        }

        inv.SetEquipmentBonusSlots(slotBonus);
        inv.SetEquipmentWeightBonus(weightBonus);
    }

    private static float GetWeightBonus(Status status, float value)
    {
        if (status == Status.WGH)
            return Mathf.Max(0f, value);

        return 0f;
    }

    public bool UnequipToInventoryAt(EquipmentPart part, int inventoryIndex, out string reason)
    {
        reason = null;
        var inv = InventoryController.Instance?.inventory;
        if (inv == null) { reason = "Inventory not found"; return false; }

        if (!TryGetAllowedSlots(part, out var slots) || slots.Count == 0)
        {
            reason = "No item equipped";
            return false;
        }

        string lastReason = "No item equipped";
        foreach (var slot in slots)
        {
            if (TryUnequipSlot(slot, inventoryIndex, inv, out var slotReason))
                return true;

            lastReason = slotReason;

            // 인벤토리가 가득 찬 경우 다른 슬롯을 시도해도 실패하므로 즉시 중단한다.
            if (slotReason == "Inventory full")
            {
                reason = slotReason;
                return false;
            }
        }

        reason = lastReason;
        return false;
    }

    public bool EquipFromInventoryAuto(int inventoryIndex, out string reason)
    {
        reason = null;
        var inv = InventoryController.Instance?.inventory;
        if (inv == null) { reason = "Inventory not found"; return false; }
        if (!inv.IsValidIndex(inventoryIndex)) { reason = "Invalid index"; return false; }

        var stack = inv.slots[inventoryIndex];
        if (stack.IsEmpty || stack.item == null) { reason = "Empty slot"; return false; }

        if (stack.item.param is ItemParameterEquipment eqParam)
            return EquipFromInventory(inventoryIndex, eqParam.equipPart, out reason);

        reason = "Not an equipment";
        return false;
    }

    public bool Discard(EquipmentSlot slot, out string reason)
    {
        if (!_equippedBySlot.TryGetValue(slot, out var equipped) || equipped == null)
        {
            reason = "Nothing equipped";
            return false;
        }

        _equippedBySlot.Remove(slot);
        RecomputeBagBonusToInventory();
        RaiseChanged();
        reason = null;
        return true;
    }

    public bool ConsumeDurability(EquipmentSlot slot, int amount = 1)
    {
        if (!_equippedBySlot.TryGetValue(slot, out var item) || item is not ItemEquipment eq)
            return false;

        return eq.ConsumeDurability(Mathf.Max(1, amount));
    }

    public bool ConsumeDurability(EquipmentPart part, int amount = 1)
    {
        if (!TryGetAllowedSlots(part, out var slots) || slots == null || slots.Count == 0)
            return false;

        foreach (var slot in slots)
        {
            if (ConsumeDurability(slot, amount))
                return true;
        }

        return false;
    }

    public bool OnWeaponAttack()
    {
        return ConsumeDurabilityByStatus(Status.Atk, 1);
    }

    public bool OnArmorHit(EquipmentSlot? slot = null)
    {
        if (slot.HasValue && ConsumeDurability(slot.Value, 1))
            return true;

        return ConsumeDurabilityByStatus(Status.Def, 1);
    }

    private bool ConsumeDurabilityByStatus(Status status, int amount)
    {
        if (_equippedBySlot.Count == 0)
            return false;

        var orderedSlots = new List<EquipmentSlot>(_equippedBySlot.Keys);
        orderedSlots.Sort();

        foreach (var slot in orderedSlots)
        {
            if (_equippedBySlot.TryGetValue(slot, out var item) && item is ItemEquipment eq)
            {
                var eqParam = eq.param;
                if (eqParam == null) continue;

                if (eqParam.status == status)
                    return ConsumeDurability(slot, amount);

                if (eqParam.extraEffect.HasValue && eqParam.extraEffect.Value.status == status)
                    return ConsumeDurability(slot, amount);
            }
        }

        return false;
    }

    public bool CanEquip(EquipmentSlot slot, Item item)
    {
        if (item == null || !item.IsEquipment()) return false;

        var eqParam = item.param as ItemParameterEquipment;
        if (eqParam == null)
            return false;

        // Keep the legacy cache field populated for systems that still rely on it.
        item.param.equipment = eqParam;

        var part = eqParam.equipPart;

        if (!InventoryConstant.AllowedEquipmentSlotsPerPart.TryGetValue(part, out var allowed))
        {
            Debug.LogWarning($"[CanEquip] part {part} not found in AllowedEquipmentSlotsPerPart!");
            return false;
        }

        if (allowed.Contains(slot))
            return true;

        // 일부 UI는 EquipmentSlot 대신 EquipmentPart를 직접 전달한다.
        // 이 경우 정수 값을 Part로 간주하여 같은 부위면 허용한다.
        var slotValue = (int)slot;
        if (Enum.IsDefined(typeof(EquipmentPart), slotValue))
        {
            var slotPart = (EquipmentPart)slotValue;
            if (slotPart == part)
                return true;
        }

        Debug.Log($"[CanEquip] part: {part}, slot: {slot} rejected. allowed: {string.Join(", ", allowed)}");
        return false;
    }

    // 🔽 EquipmentSlot 기반으로 해제하는 함수 추가
    public bool UnequipToInventoryAt(EquipmentSlot slot, int inventoryIndex, out string reason)
    {
        return TryUnequipSlot(slot, inventoryIndex, InventoryController.Instance?.inventory, out reason);
    }

    private bool TryUnequipSlot(EquipmentSlot slot, int inventoryIndex, Inventory inv, out string reason)
    {
        if (inv == null)
        {
            reason = "Inventory not found";
            return false;
        }

        if (!_equippedBySlot.TryGetValue(slot, out var equipped) || equipped == null)
        {
            reason = "No item equipped";
            return false;
        }

        // 인벤토리 슬롯이 비어있으면 직접 배치
        if (inv.IsValidIndex(inventoryIndex) && inv.slots[inventoryIndex].IsEmpty)
        {
            inv.slots[inventoryIndex] = new Inventory.ItemStack { item = equipped, count = 1 };
            _equippedBySlot.Remove(slot);
            RecomputeBagBonusToInventory();
            inv.RaiseChanged();
            RaiseChanged();
            reason = null;
            return true;
        }

        // 빈칸 찾기
        int empty = -1;
        for (int i = 0; i < inv.ActiveSlotCount; i++)
        {
            if (inv.slots[i].IsEmpty) { empty = i; break; }
        }

        if (empty >= 0)
        {
            inv.slots[empty] = new Inventory.ItemStack { item = equipped, count = 1 };
            _equippedBySlot.Remove(slot);
            RecomputeBagBonusToInventory();
            inv.RaiseChanged();
            RaiseChanged();
            reason = null;
            return true;
        }

        reason = "Inventory full";
        return false;
    }

    public bool HandleBrokenItem(ItemEquipment eq)
    {
        if (eq == null)
            return false;

        if (!TryFindSlot(eq, out var slot))
            return false;

        _equippedBySlot.Remove(slot);
        RecomputeBagBonusToInventory();
        RaiseChanged();
        return true;
    }

    public bool NotifyDurabilityChanged(ItemEquipment eq)
    {
        if (eq == null)
            return false;

        if (!TryFindSlot(eq, out _))
            return false;

        RaiseChanged();
        return true;
    }

    public bool TryFindSlot(Item item, out EquipmentSlot slot)
    {
        foreach (var kv in _equippedBySlot)
        {
            if (ReferenceEquals(kv.Value, item))
            {
                slot = kv.Key;
                return true;
            }
        }

        slot = default;
        return false;
    }

    private static bool TryGetAllowedSlots(EquipmentPart part, out List<EquipmentSlot> slots)
    {
        if (!InventoryConstant.AllowedEquipmentSlotsPerPart.TryGetValue(part, out slots) || slots == null)
        {
            slots = null;
            return false;
        }

        return true;
    }

}
