// Item.cs
using System;
using System.Data.Common;

public class Item
{
    public int id;
    public ItemParameter param;
    public ItemInfo info;


    // ✅ 고유 식별자
    public Guid guid = Guid.NewGuid();

    public Item(int id)
    {
        this.id = id;
        param = ItemParameterList.GetItemStat(id);
        info = info ?? new ItemInfo();
    }

    public bool IsEquipment()
    {
        return param != null && param.type == ItemType.Equipment;
    }

    public virtual void Use() { /* ... */ }
}
