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

    public override void Use()
    {
        durability -= param.durabilityDecayRate;
    }
}