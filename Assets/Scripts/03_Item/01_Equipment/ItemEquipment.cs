public enum EquipPart
{
    Head,
    Body,
    Arms,
    Hands,
    Bag,
    Knees,
    Feet,
}

public class ItemEquipment : Item
{
    public new ItemParameterEquipment param => (ItemParameterEquipment)base.param;
    public int durability;

    public ItemEquipment(int id, int durability = -1) : base(id)
    {
        if (durability == -1) this.durability = (param as ItemParameterEquipment).maxDurability;
        else this.durability = durability;
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