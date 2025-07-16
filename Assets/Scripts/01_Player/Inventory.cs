using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private List<ItemInfo> items;

    public void Initialize()
    {
        items = new();
    }

    public void AddItem(ItemInfo item)
    {
        if (item != null && !items.Contains(item))
        {
            items.Add(item);
        }
    }

    public void RemoveItem(ItemInfo item)
    {
        if (item != null && items.Contains(item))
        {
            items.Remove(item);
        }
    }

    public List<ItemInfo> GetItems()
    {
        return new List<ItemInfo>(items);
    }
}