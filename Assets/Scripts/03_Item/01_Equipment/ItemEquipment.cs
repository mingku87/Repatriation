using UnityEngine;

public class ItemEquipment : Item
{
    public new ItemParameterEquipment param => (ItemParameterEquipment)base.param;
    public int durability;

    public ItemEquipment(int id, int initialDurability = -1) : base(id)
    {
        int max = param?.maxDurability ?? 0;
        durability = (initialDurability >= 0) ? initialDurability : max;
    }

    public virtual void Equip()
    {
        Player.SetMaxStatus(param.status, param.value);
        Player.SetCurrentStatus(param.status, param.value);
    }
    public virtual void UnEquip()
    {
        Player.SetMaxStatus(param.status, 0);
        Player.SetCurrentStatus(param.status, 0);
    }

    public bool ConsumeDurability(int amount = 1)
    {
        if (param == null || param.maxDurability <= 0)
            return false;

        amount = Mathf.Max(1, amount);
        if (durability <= 0)
            return false;

        durability = Mathf.Max(0, durability - amount);

        var controller = InventoryController.Instance;

        if (durability <= 0)
        {
            if (controller?.equipment != null && controller.equipment.HandleBrokenItem(this))
                return true;

            if (controller?.inventory != null && controller.inventory.RemoveItemReference(this))
                return true;

            return true;
        }

        bool notified = false;

        if (controller?.equipment != null)
            notified |= controller.equipment.NotifyDurabilityChanged(this);

        if (controller?.inventory != null)
            notified |= controller.inventory.NotifyDurabilityChanged(this);

        return true;
    }

    public override void Use()
    {
        ConsumeDurability();
    }
}
