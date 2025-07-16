public class ItemConsumable : Item
{
    public new ItemParameterConsumable param => (ItemParameterConsumable)base.param;
    public ItemConsumable(int id) : base(id) { }

    public override void Use()
    {
        Player.ChangeCurrentStatus(param.status, param.value);
    }
}