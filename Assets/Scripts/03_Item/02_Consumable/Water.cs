public class Water : ItemConsumable
{
    public float quality;
    public Water(ItemName itemName, float value, float quality) : base(itemName, Status.Thirst, value)
    {
        this.quality = quality;
    }

    public override void Use()
    {
        base.Use();
        Player.ChangeCurrentStatus(Status.HP, quality - 100);
    }
}