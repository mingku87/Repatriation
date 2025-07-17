using UnityEngine;

public class InventoryController : SingletonObject<InventoryController>
{
    private Inventory inventory;

    public void Initialize()
    {
        inventory = new();
        inventory.Initialize();
    }

    void Update()
    {
        UseItem();
        CheckHoldItem();
    }

    public void UseItem()
    {
        if (Input.GetKeyDown(KeySetting.GetKey(PlayerAction.UseItem)) == false) return;

        var holdItem = inventory.GetHoldItem();
        if (holdItem == null) return;
        holdItem.Use();
    }

    public void CheckHoldItem()
    {
        var quickSlots = inventory.GetQuickSlots();

        foreach (var kv in quickSlots)
        {
            if (Input.GetKeyDown(KeySetting.GetKey(kv.Key)))
            {
                inventory.SetHoldItem(kv.Value);
                break;
            }
        }
    }

    public void AddItem(Item item) { inventory.AddItem(item); }
    public void RemoveItem(Item item) { inventory.RemoveItem(item); }
}