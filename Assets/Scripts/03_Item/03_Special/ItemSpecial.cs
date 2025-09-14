public class ItemSpecial : Item
{
    public new ItemParameterSpecial param => (ItemParameterSpecial)base.param;
    public ItemSpecial(int id) : base(id) { }

    public override void Use() { }
}