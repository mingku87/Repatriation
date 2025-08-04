using System.Collections.Generic;

public class PlayerSettings
{
    public Dictionary<Status, float> maxStatus = new();

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
    public static string AnimatorBoolIsMove = "isMove";

    public static Dictionary<Difficulty, PlayerSettings> Settings = new() {
        { Difficulty.Easy, new PlayerSettings(){
            maxStatus = new(){
                { Status.HP, 100.0f },
                { Status.Thirst, 100.0f },
                { Status.Symptom, 100.0f },
                { Status.Atk, 10.0f },
                { Status.AtkRange, 1.0f },
                { Status.Def, 5.0f },
                { Status.Speed, 3.0f },
                { Status.SightRange, 5.0f }
            },
            thirstDecayRate = 0.5f,
            symptomDecayRate = new(){
                { 10.0f, 1.0f },
                { 20.0f, 0.5f },
                { 40.0f, 0.33f },
                { 60.0f, 0.2f },
                { 80.0f, 0.12f },
                { 100.0f, 0.08f }
            },
            healthDecayRateByThirst = 3.0f,
            healthDecayRateBySymptomRate = new(){
                { 0, 64 },
                { 1, 32 },
                { 5, 16 },
                { 10, 8 },
                { 20, 4 },
                { 30, 2 },
                { 60, 1 },
                { 100, 0 }
            }
        }},
        { Difficulty.Hard, new PlayerSettings(){
            maxStatus = new(){
                { Status.HP, 100.0f },
                { Status.Thirst, 100.0f },
                { Status.Symptom, 100.0f },
                { Status.Atk, 10.0f },
                { Status.AtkRange, 1.0f },
                { Status.Def, 5.0f },
                { Status.Speed, 3.0f },
                { Status.SightRange, 5.0f }
            },
            thirstDecayRate = 0.5f,
            symptomDecayRate = new(){
                { 10.0f, 1.0f },
                { 20.0f, 0.5f },
                { 40.0f, 0.33f },
                { 60.0f, 0.2f },
                { 80.0f, 0.12f },
                { 100.0f, 0.08f }
            },
            healthDecayRateByThirst = 3.0f,
            healthDecayRateBySymptomRate = new(){
                { 0, 64 },
                { 1, 32 },
                { 5, 16 },
                { 10, 8 },
                { 20, 4 },
                { 30, 2 },
                { 60, 1 },
                { 100, 0 }
            }
        }}
    };
}