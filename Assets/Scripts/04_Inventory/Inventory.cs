using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private List<Item> items;
    private Dictionary<PlayerAction, Item> quickSlots;
    private Item holdItem;

    public void Initialize()
    {
        items = new();
        quickSlots = new() {
            { PlayerAction.Slot1, null },
            { PlayerAction.Slot2, null },
            { PlayerAction.Slot3, null },
            { PlayerAction.Slot4, null },
            { PlayerAction.Slot5, null },
            { PlayerAction.Slot6, null }
        };
        holdItem = null;
    }

    public Dictionary<PlayerAction, Item> GetQuickSlots() { return quickSlots; }
    public Item GetHoldItem() { return holdItem; }
    public void SetHoldItem(Item item)
    {
        if (item == null) return;
        holdItem = item;
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