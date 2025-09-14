using System.Collections.Generic;

public class ItemParameter
{
    public int id;
    public ItemType type;
    public ItemName itemName;
    public float weight;
    public float priceBuy;
    public float priceSell;

    public ItemParameter(int id, ItemName itemName, float weight, float priceBuy, float priceSell)
    {
        this.id = id;
        this.itemName = itemName;
        this.weight = weight;
        this.priceBuy = priceBuy;
        this.priceSell = priceSell;
    }
}

public class ItemParameterEquipment : ItemParameter
{
    public EquipmentPart equipPart;
    public Dictionary<Status, float> equipStatus;
    public int maxDurability;
    public int durabilityDecayRate;

    public ItemParameterEquipment(int id, ItemName itemName, float weight, float priceBuy, float priceSell, EquipmentPart equipPart, Dictionary<Status, float> equipStatus, int maxDurability = 100, int durabilityDecayRate = 1)
        : base(id, itemName, weight, priceBuy, priceSell)
    {
        type = ItemType.Equipment;
        this.equipPart = equipPart;
        this.equipStatus = equipStatus;
        this.maxDurability = maxDurability;
        this.durabilityDecayRate = durabilityDecayRate;
    }
}

public class ItemParameterConsumable : ItemParameter
{
    public Status status;
    public float value;

    public ItemParameterConsumable(int id, ItemName itemName, float weight, float priceBuy, float priceSell, Status status, float value)
        : base(id, itemName, weight, priceBuy, priceSell)
    {
        type = ItemType.Consumable;
        this.status = status;
        this.value = value;
    }
}

public class ItemParameterWater : ItemParameterConsumable
{
    public int quality;

    public ItemParameterWater(int id, ItemName itemName, float weight, float priceBuy, float priceSell, float value, int quality)
        : base(id, itemName, weight, priceBuy, priceSell, Status.Thirst, value)
    {
        type = ItemType.Water;
        this.quality = quality;
    }
}

public class ItemParameterSpecial : ItemParameter
{
    public ItemParameterSpecial(int id, ItemName itemName, float weight, float priceBuy, float priceSell)
    : base(id, itemName, weight, priceBuy, priceSell)
    {
        type = ItemType.Special;
    }
}