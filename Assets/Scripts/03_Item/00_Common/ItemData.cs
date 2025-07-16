public class ItemData : SingletonScriptableObject<ItemData>
{
    public ItemInfo[] itemInfos;

    public ItemInfo GetItemInfo(ItemName itemName)
    {
        foreach (var itemInfo in itemInfos)
            if (itemInfo.itemName == itemName) return itemInfo;
        return null;
    }
}