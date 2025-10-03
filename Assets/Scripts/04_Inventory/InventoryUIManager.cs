// InventoryUIManager.cs
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [Header("인벤토리 슬롯 배열 (25칸)")]
    public InventorySlotUI[] inventorySlots;

    Inventory inv;

    public void Bind(Inventory inventory)
    {
        inv = inventory;
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        if (inv == null || inventorySlots == null) return;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].index = i;

            // 인벤토리 내 유효 슬롯
            Inventory.SlotView view = default;
            if (i < inv.ActiveSlotCount)
                view = inv.GetView(i);

            inventorySlots[i].Set(view);
        }
    }
}
