using System.Collections.Generic;

public class ItemStatList
{
    public static List<ItemParameter> itemStats = new()
    {

    };

    public static ItemParameter GetItemStat(int id)
    {
        foreach (var itemStat in itemStats)
            if (itemStat.id == id) return itemStat;
        return null;
    }
}