public abstract class Item
{
    public int id;
    public ItemParameter param;
    public ItemInfo info;

    public Item(int id)
    {
        this.id = id;

        param = ItemStatList.GetItemStat(id);
        info = ItemInfoSO.GetItemInfo(param.itemName);
    }

    public abstract void Use();
}