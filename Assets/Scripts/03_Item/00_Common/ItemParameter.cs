public class ItemParameter
{
    public int id;
    public ItemType type;
    public ItemName itemName;
    public float weight;

    public ItemParameter(int id, ItemName itemName, float weight)
    {
        this.id = id;
        this.itemName = itemName;
        this.weight = weight;
    }
}

public class ItemParameterEquipment : ItemParameter
{
    public EquipmentPart equipPart;
    public Status status;
    public float value;
    public int maxDurability;
    public int durabilityDecayRate;

    public ItemParameterEquipment(int id, ItemName itemName, float weight, EquipmentPart equipPart, Status status, float value, int maxDurability = 100, int durabilityDecayRate = 1)
        : base(id, itemName, weight)
    {
        type = ItemType.Equipment;
        this.equipPart = equipPart;
        this.status = status;
        this.value = value;
        this.maxDurability = maxDurability;
        this.durabilityDecayRate = durabilityDecayRate;
    }
}

public class ItemParameterConsumable : ItemParameter
{
    public Status status;
    public float value;

    public ItemParameterConsumable(int id, ItemName itemName, float weight, Status status, float value)
        : base(id, itemName, weight)
    {
        type = ItemType.Consumable;
        this.status = status;
        this.value = value;
    }
}

public class ItemParameterWater : ItemParameterConsumable
{
    public int quality;

    public ItemParameterWater(int id, ItemName itemName, float weight, float value, int quality)
        : base(id, itemName, weight, Status.Thirst, value)
    {
        type = ItemType.Water;
        this.quality = quality;
    }
}