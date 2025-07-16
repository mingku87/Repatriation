using System;

public static class ItemFactory
{
    public static Item Create(int id, int value = -1)
    {
        var type = ItemParameterList.GetItemStat(id).type;

        switch (type)
        {
            case ItemType.Equipment:
                return new ItemEquipment(id, value);
            case ItemType.Consumable:
                return new ItemConsumable(id);
            case ItemType.Water:
                return new ItemConsumableWater(id);
        }

        throw new ArgumentException($"Unknown item stat type for id = {id}");
    }
}