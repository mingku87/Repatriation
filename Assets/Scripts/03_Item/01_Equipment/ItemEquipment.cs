public abstract class ItemEquipment : Item
{
    public Status status;
    public float value;
    public float durability;

    public ItemEquipment(ItemInfo itemInfo, Status status, float value)
    {
        this.itemInfo = itemInfo;
        this.status = status;
        this.value = value;
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