public abstract class ItemConsumable : Item
{
    public Status status;
    public float value;

    public ItemConsumable(int id, Status status) : base(id)
    {
        this.id = id;
        this.status = status;
        value = (param as ItemParameterConsumable).value;
    }

    public override void Use()
    {
        Player.ChangeCurrentStatus(status, value);
    }
}