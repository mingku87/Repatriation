public abstract class ItemConsumable : Item
{
    public Status status;
    public float value;

    public ItemConsumable(ItemName itemName, Status status, float value)
    {
        this.itemName = itemName;
        this.status = status;
        this.value = value;
    }

    public override void Use()
    {
        Player.ChangeCurrentStatus(status, value);
    }
}