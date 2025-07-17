using System.Collections.Generic;

public static class InventoryConstant
{
    public static int MaxQuickSlots = 6;
    public static readonly Dictionary<EquipPart, int> MaxPerSlot = new()
    {
        { EquipPart.Head, 1 },
        { EquipPart.Body, 1 },
        { EquipPart.Arms, 2 },
        { EquipPart.Hands, 2 },
        { EquipPart.Bag, 1 },
        { EquipPart.Knees, 2 },
        { EquipPart.Feet, 2 }
    };
}