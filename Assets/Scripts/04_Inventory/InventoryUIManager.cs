// InventoryUIManager.cs
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [Header("�κ��丮 ����(���� â)")]
    public InventorySlotUI[] inventorySlots; // �ν����Ϳ� 20ĭ ��� �巡��
    Inventory inv;

    public void Bind(Inventory inventory)
    {
        inv = inventory;
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        if (inv == null || inventorySlots == null) return;

        int active = inv.ActiveSlotCount;

        // Ȱ�� ĭ ����
        for (int i = 0; i < active && i < inventorySlots.Length; i++)
        {
            var v = inv.GetView(i);
            inventorySlots[i].Set(v); // �� ���� UI�� �� ����
        }

        // ��Ȱ��/���� ĭ�� Ŭ����
        for (int i = active; i < inventorySlots.Length; i++)
            inventorySlots[i].Clear();
    }
}
