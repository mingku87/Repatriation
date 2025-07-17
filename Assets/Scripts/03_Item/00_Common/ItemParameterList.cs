using System.Collections.Generic;

public class ItemParameterList
{
    public static List<ItemParameter> itemStats = new()
    {
        new ItemParameterEquipment(10001, ItemName.OldHelmet, 5, EquipmentPart.Head, Status.Def, 10),
    };

    public static ItemParameter GetItemStat(int id)
    {
        foreach (var itemStat in itemStats)
            if (itemStat.id == id) return itemStat;
        return null;
    }
}