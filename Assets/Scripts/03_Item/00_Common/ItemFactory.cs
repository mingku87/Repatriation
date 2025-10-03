using System;
using UnityEngine;

public static class ItemFactory
{
    public static Item Create(int id, int value = -1)
    {
        var stat = ItemParameterList.GetItemStat(id);
        if (stat == null)
        {
            Debug.LogError($"[ItemFactory] Unknown item id: {id}");
            return null;
        }

        Item item;
        switch (stat.type)
        {
            case ItemType.Equipment: item = new ItemEquipment(id, value); break;
            case ItemType.Consumable: item = new ItemConsumable(id); break;
            case ItemType.Water: item = new ItemConsumableWater(id); break;
            default:
                Debug.LogError($"[ItemFactory] Unsupported type {stat.type} for id {id}");
                return null;
        }

        // TSV기반 표시정보 주입
        var row = ItemPresentationDB.Get(id);
        if (row != null)
        {
            item.info ??= new ItemInfo();
            item.info.name = string.IsNullOrEmpty(row.name) ? stat.itemName.ToString() : row.name;
            item.info.description = row.description ?? string.Empty;
            item.info.image = row.icon;
        }
        else
        {
            // row가 없더라도 최소한의 기본값
            item.info ??= new ItemInfo { name = stat.itemName.ToString(), description = "" };
        }

        return item;
    }
}
