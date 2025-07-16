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
}