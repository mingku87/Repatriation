// InventoryUIManager.cs
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [Header("인벤토리 슬롯(왼쪽 창)")]
    public InventorySlotUI[] inventorySlots; // 인스펙터에 20칸 모두 드래그
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

        // 활성 칸 갱신
        for (int i = 0; i < active && i < inventorySlots.Length; i++)
        {
            var v = inv.GetView(i);
            inventorySlots[i].Set(v); // ← 슬롯 UI에 뷰 전달
        }

        // 비활성/남는 칸은 클리어
        for (int i = active; i < inventorySlots.Length; i++)
            inventorySlots[i].Clear();
    }
}
