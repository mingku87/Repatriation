public class ItemParameter
{
    public int id;
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
    public Status status;
    public float value;
    public int maxDurability;

    public ItemParameterEquipment(int id, ItemName itemName, float weight, Status status, float value, int maxDurability = 100)
        : base(id, itemName, weight)
    {
        this.status = status;
        this.value = value;
        this.maxDurability = maxDurability;
    }
}

public class ItemParameterConsumable : ItemParameter
{
    public Status status;
    public float value;

    public ItemParameterConsumable(int id, ItemName itemName, float weight, Status status, float value)
        : base(id, itemName, weight)
    {
        this.status = status;
        this.value = value;
    }
}

public class ItemParameterFood : ItemParameterConsumable
{

    public ItemParameterFood(int id, ItemName itemName, float weight, float value)
        : base(id, itemName, weight, Status.HP, value) { }
}

public class ItemParameterWater : ItemParameterConsumable
{
    public int quality;

    public ItemParameterWater(int id, ItemName itemName, float weight, float value, int quality)
        : base(id, itemName, weight, Status.Thirst, value)
    {
        this.quality = quality;
    }
}

public class ItemParameterMedicine : ItemParameterConsumable
{
    public ItemParameterMedicine(int id, ItemName itemName, float weight, float value)
        : base(id, itemName, weight, Status.Symptom, value) { }
}