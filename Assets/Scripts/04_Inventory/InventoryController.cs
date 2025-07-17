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
        int slotCount = InventoryConstant.MaxQuickSlots;
        for (int i = 0; i < slotCount; i++)
        {
            if (Input.GetKeyDown(KeySetting.GetQuickSlotKey(i)) == false) continue;

            inventory.UseQuickSlotItem(i);
            break;
        }
    }

    public void AddItem(Item item) => inventory.AddItem(item);
    public void RemoveItem(Item item) => inventory.RemoveItem(item);
}