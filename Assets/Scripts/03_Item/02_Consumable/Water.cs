public class Water : ItemConsumable
{
    public float quality;
    public Water(ItemName itemName, float value) : base(itemName, Status.Thirst, value) { }
}