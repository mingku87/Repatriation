using System.Collections.Generic;

public abstract class StatusEffect
{
    public StatusEffectInfo info;
    public abstract void ApplyEffect();
    public abstract void RemoveEffect();
    protected virtual Dictionary<string, object> GetDescriptionValues() { return null; }
    public string GetDescription()
    {
        string description = StatusEffectInfoSO.GetStatusEffectInfo(info.id).description;
        var values = GetDescriptionValues();
        foreach (var pair in values) description = description.Replace($"{{{pair.Key}}}", pair.Value.ToString());
        return description;
    }
}