public class ItemEquipment : Item
{
    public new ItemParameterEquipment param => (ItemParameterEquipment)base.param;
    public int durability;

    public ItemEquipment(int id, int durability = -1) : base(id)
    {
        if (durability == -1) this.durability = param.maxDurability;
        else this.durability = durability;
    }

    public virtual void Equip()
    {
        foreach (Status status in param.equipStatus.Keys)
        {
            Player.SetMaxStatus(status, param.equipStatus[status]);
            Player.SetCurrentStatus(status, param.equipStatus[status]);
        }
    }
    public virtual void UnEquip()
    {
        foreach (Status status in param.equipStatus.Keys)
        {
            Player.SetMaxStatus(status, 0);
            Player.SetCurrentStatus(status, 0);
        }
    }

    public override void Use()
    {
        durability -= param.durabilityDecayRate;
    }
}