// InventoryUIManager.cs
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [Header("�κ��丮 ���� �迭 (25ĭ)")]
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

            // �κ��丮 �� ��ȿ ����
            Inventory.SlotView view = default;
            if (i < inv.ActiveSlotCount)
                view = inv.GetView(i);

            inventorySlots[i].Set(view);
        }
    }
}
