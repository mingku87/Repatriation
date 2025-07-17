using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : SingletonObject<Inventory>
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

    void Update()
    {
        UseItem();
        CheckHoldItem();
    }

    public void UseItem()
    {
        if (Input.GetKeyDown(KeySetting.GetKey(PlayerAction.UseItem)) == false) return;
        if (holdItem == null) return;

        holdItem.Use();
    }

    public void CheckHoldItem()
    {
        foreach (var kv in quickSlots)
        {
            if (Input.GetKeyDown(KeySetting.GetKey(kv.Key)))
            {
                holdItem = kv.Value;
                break;
            }
        }
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