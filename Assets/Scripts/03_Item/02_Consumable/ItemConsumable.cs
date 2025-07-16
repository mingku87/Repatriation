public abstract class ItemConsumable : Item
{
    public Status status;
    public float value;

    public ItemConsumable(ItemInfo itemInfo, Status status, float value)
    {
        this.itemInfo = itemInfo;
        this.status = status;
        this.value = value;
    }
}