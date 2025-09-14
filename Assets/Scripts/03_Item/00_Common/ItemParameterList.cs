using System.Collections.Generic;

public class ItemParameterList
{
    public static List<ItemParameter> itemStats = new()
    {
        new ItemParameterEquipment(10001, ItemName.OldHelmet, 5, 200, 20, EquipmentPart.Head, new(){{Status.Def, 10}}),
        new ItemParameterEquipment(10002, ItemName.ConstructionSiteHelmet, 5, 400, 40, EquipmentPart.Head, new(){{Status.Def, 15}, {Status.Atk, 5}}),
    };

    public static ItemParameter GetItemStat(int id)
    {
        foreach (var itemStat in itemStats)
            if (itemStat.id == id) return itemStat;
        return null;
    }
}