public class Water : ItemConsumable
{
    public float quality;
    public Water(int id) : base(id, Status.Thirst)
    {
        quality = (param as ItemParameterWater).quality;
    }

    public override void Use()
    {
        base.Use();
        Player.ChangeCurrentStatus(Status.HP, quality - 100);
    }
}