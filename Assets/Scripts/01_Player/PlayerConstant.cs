using System.Collections.Generic;
using UnityEngine;

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

public class PlayerConstant
{
    [SerializeField] private PlayerSettings easySettings;
    [SerializeField] private PlayerSettings hardSettings;

    public static Dictionary<Difficulty, PlayerSettings> Settings = new() {
        { Difficulty.Easy, new PlayerSettings() {
            speed = 5.0f,
            maxHP = 100.0f,
            maxThirst = 100.0f,
            maxSymptom = 100.0f,
            healthDecayRateWhenThirsty = 3.0f,
            thirstDecayRate = 0.5f
        }},
        { Difficulty.Hard, new PlayerSettings() {
            speed = 4.0f,
            maxHP = 80.0f,
            maxThirst = 80.0f,
            maxSymptom = 80.0f,
            healthDecayRateWhenThirsty = 2.0f,
            thirstDecayRate = 2.0f
        }}
    };
}