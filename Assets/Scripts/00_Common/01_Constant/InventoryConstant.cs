using System.Collections.Generic;

public static class InventoryConstant
{
    public static int MaxQuickSlotCount = 6;
    public const float DefaultMaxCarryWeight = 100f;
    public static readonly Dictionary<EquipmentPart, List<EquipmentSlot>> AllowedEquipmentSlotsPerPart = new()
    {
        { EquipmentPart.Head,  new(){ EquipmentSlot.Head } },
        { EquipmentPart.Body,  new(){ EquipmentSlot.Body } },
        { EquipmentPart.Arms,  new(){ EquipmentSlot.LeftArm, EquipmentSlot.RightArm } },
        { EquipmentPart.Hands, new(){ EquipmentSlot.LeftHand, EquipmentSlot.RightHand } },
        { EquipmentPart.Bag,   new(){ EquipmentSlot.Bag } },
        { EquipmentPart.Knees, new(){ EquipmentSlot.LeftKnee, EquipmentSlot.RightKnee } },
        { EquipmentPart.Feet,  new(){ EquipmentSlot.LeftFoot, EquipmentSlot.RightFoot } },
    };
}

public enum EquipmentPart
{
    Head,
    Body,
    Arms,
    Hands,
    Bag,
    Knees,
    Feet,
}

public enum EquipmentSlot
{
    Head,
    Body,
    LeftArm,
    RightArm,
    LeftHand,
    RightHand,
    Bag,
    LeftKnee,
    RightKnee,
    LeftFoot,
    RightFoot
}