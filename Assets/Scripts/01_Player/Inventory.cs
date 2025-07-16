using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private List<Item> items;

    public void Initialize()
    {
        items = new();
    }

    public void AddItem(Item item)
    {
        if (item == null) return;
        items.Add(item);
    }

    public void RemoveItem(Item item)
    {
        if (item == null || items.Contains(item) == false) return;
        items.Remove(item);
    }
}