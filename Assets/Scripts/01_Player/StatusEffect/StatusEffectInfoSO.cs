using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/StatusEffectInfo")]
public class StatusEffectInfoSO : SingletonScriptableObject<StatusEffectInfoSO>
{
    [SerializeField] public List<StatusEffectInfo> statusEffectInfo;
    private static List<StatusEffectInfo> statusEffectInfos;

    void Awake()
    {
        statusEffectInfos = new();
        foreach (var info in statusEffectInfo) statusEffectInfo.Add(info);
    }

    public static StatusEffectInfo GetStatusEffectInfo(StatusEffectID id)
    {
        foreach (var effect in statusEffectInfos)
            if (effect.id == id) return effect;
        return null;
    }
}