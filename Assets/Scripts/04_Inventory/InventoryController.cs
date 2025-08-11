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
        if (Input.GetKeyDown(KeyManager.GetKey(PlayerAction.UseItem)) == false) return;
        inventory.UseHoldItem();
    }

    public void CheckHoldItem()
    {
        int slotCount = InventoryConstant.MaxQuickSlotCount;
        for (int i = 0; i < slotCount; i++)
        {
            if (Input.GetKeyDown(KeyManager.GetQuickSlotKey(i)) == false) continue;
            inventory.SetHoldItem(i);
            break;
        }
    }

    public void AddItem(Item item) => inventory.AddItem(item);
    public void RemoveItem(Item item) => inventory.RemoveItem(item);
}