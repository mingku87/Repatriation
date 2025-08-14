using UnityEngine;

[System.Serializable]
public class StatusEffectInfo
{
    public StatusEffectID id;
    public string name;
    [TextArea] public string description;
    public Sprite sprite;
}

public enum StatusEffectID
{
    // Stat loss
    HealthLoss,
    ThirstLoss,
    WellnessLoss,

    // Stat reduction
    Weakness,
    SevereThirst,
    PoorHealth,

    // Movement effects
    MovementSlow,
    Overstimulated,
    InvertedControls,

    // Ban effects
    FoodBan,
    WaterBan,
    MedicineBan,

    // Effect reductions
    FoodEffectReduction,
    WaterEffectReduction,
    MedicineEffectReduction
}