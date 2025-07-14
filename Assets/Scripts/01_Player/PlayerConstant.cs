using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSettings
{
    public float speed;

    public float maxHP;
    public float maxThirst;
    public float maxSymptom;

    public float healthDecayRateWhenThirsty;
    public float thirstDecayRate;
    //public const float SymptomDecayRate;
}

public class PlayerConstant : Singleton<PlayerConstant>
{
    [SerializeField] private PlayerSettings easySettings;
    [SerializeField] private PlayerSettings hardSettings;

    public static Dictionary<Difficulty, PlayerSettings> Settings;

    protected override void Awake()
    {
        Settings = new Dictionary<Difficulty, PlayerSettings>
        {
            { Difficulty.Easy, easySettings },
            { Difficulty.Hard, hardSettings }
        };
    }
}