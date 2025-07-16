public abstract class ItemEquipment : Item
{
    public Status status;
    public float value;
    public int durability;

    public ItemEquipment(int id, Status status, float value, int durability = -1) : base(id)
    {
        this.id = id;
        this.status = status;
        this.value = value;

        if (durability == -1) this.durability = (param as ItemParameterEquipment).maxDurability;
        else this.durability = durability;
    }

    public virtual void Equip()
    {
        Player.SetMaxStatus(status, value);
        Player.SetCurrentStatus(status, value);
    }
    public virtual void UnEquip()
    {
        Player.SetMaxStatus(status, 0);
        Player.SetCurrentStatus(status, 0);
    }
}