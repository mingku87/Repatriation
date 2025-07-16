public class ItemConsumableWater : ItemConsumable
{
    public new ItemParameterWater param => (ItemParameterWater)base.param;
    public ItemConsumableWater(int id) : base(id) { }

    public override void Use()
    {
        base.Use();
        Player.ChangeCurrentStatus(Status.HP, param.quality - 100);
    }
}