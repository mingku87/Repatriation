using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings
{
    public float speed;

    public float maxHP;
    public float maxThirst;
    public float maxSymptom;

    public float thirstDecayRate;
    public Dictionary<float, float> symptomDecayRate;

    public float healthDecayRateByThirst;
    public Dictionary<float, float> healthDecayRateBySymptomRate;

    public float GetSymptomDecayRate(float symptom)
    {
        foreach (var entry in symptomDecayRate)
            if (symptom <= entry.Key) return entry.Value;
        return 0.0f;
    }

    public float GetHealthDecayRateBySymptomRate(float symptomRate)
    {
        foreach (var entry in healthDecayRateBySymptomRate)
            if (symptomRate <= entry.Key) return entry.Value;
        return 0.0f;
    }
}

public class PlayerConstant
{
    public static string AnimatorFloatMoveX = "MoveX";
    public static string AnimatorFloatMoveY = "MoveY";

    public static Dictionary<Difficulty, PlayerSettings> Settings = new() {
        { Difficulty.Easy, new PlayerSettings() {
            speed = 3.0f,
            maxHP = 100.0f,
            maxThirst = 100.0f,
            maxSymptom = 100.0f,
            thirstDecayRate = 0.5f,
            symptomDecayRate = new(){{ 10, 1.0f }, { 20, 0.5f }, { 40, 0.33f }, { 60, 0.2f }, { 80, 0.12f }, { 100, 0.08f }},
            healthDecayRateByThirst = 3.0f,
            healthDecayRateBySymptomRate = new(){{ 0, 64 }, { 1, 32 }, { 5, 16 }, { 10, 8 }, { 20, 4 }, { 30, 2 }, { 60, 1 }, { 100, 0 }}
        }},
        { Difficulty.Hard, new PlayerSettings() {
            speed = 3.0f,
            maxHP = 80.0f,
            maxThirst = 80.0f,
            maxSymptom = 80.0f,
            thirstDecayRate = 0.5f,
            symptomDecayRate = new(){{ 10, 1.0f }, { 20, 0.5f }, { 40, 0.33f }, { 60, 0.2f }, { 80, 0.12f }, { 100, 0.08f }},
            healthDecayRateByThirst = 3.0f,
            healthDecayRateBySymptomRate = new(){{ 0, 64 }, { 1, 32 }, { 5, 16 }, { 10, 8 }, { 20, 4 }, { 30, 2 }, { 60, 1 }, { 100, 0 }}
        }}
    };
}