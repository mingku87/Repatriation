// DragContext.cs
using UnityEngine;

public enum DragSource { None, Inventory, Equipment }

public sealed class DragContext
{
    public static DragContext Current { get; private set; }

    public DragSource source = DragSource.None;
    public int inventoryIndex = -1;

    // ✅ 장비 슬롯 드래그에서 넘겨줄 실제 ‘부위’
    public EquipmentSlot equipSlot;

    public static void BeginFromInventory(int index)
    {
        Debug.Log($"[DragContext] BeginFromInventory({index})");
        Current = new DragContext { source = DragSource.Inventory, inventoryIndex = index };
    }

    public static void BeginFromEquipment(EquipmentSlot slot)
    {
        Debug.Log($"[DragContext] BeginFromEquipment({slot})");
        Current = new DragContext
        {
            source = DragSource.Equipment,
            equipSlot = slot
        };

    }
     public static void End()
    {
        Debug.Log("[DragContext] End()");
        Current = null;
    }
}
