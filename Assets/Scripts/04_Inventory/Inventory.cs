using System.Collections.Generic;

public class Inventory
{
    private List<Item> items;
    private List<Item> quickSlots;
    private Item holdItem;

    public void Initialize()
    {
        items = new();
        quickSlots = new();
        for (int i = 0; i < InventoryConstant.MaxQuickSlotCount; i++) quickSlots.Add(null);
        holdItem = null;
    }

    public List<Item> GetQuickSlots() { return quickSlots; }
    public Item GetHoldItem() { return holdItem; }
    public void SetHoldItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= quickSlots.Count) return;
        holdItem = quickSlots[slotIndex];
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
        quickSlots.Remove(item);
    }

    public void UseItem(Item item)
    {
        if (item == null) return;
        item.Use();
    }

    public void UseHoldItem()
    {
        if (holdItem == null) return;
        UseItem(holdItem);
    }

    public void UseQuickSlotItem(int slotIndex)
    {
        var item = quickSlots[slotIndex];
        if (slotIndex < 0 || slotIndex >= quickSlots.Count || item == null) return;
        UseItem(item);
    }

    public float GetTotalWeight()
    {
        float totalWeight = 0f;
        foreach (var item in items) totalWeight += item.param.weight;
        return totalWeight;
    }
}