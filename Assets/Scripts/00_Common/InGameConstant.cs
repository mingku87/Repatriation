using System.Collections.Generic;
using UnityEngine;

public enum Difficulty
{
    Easy,
    Hard
}

[System.Serializable]
public class DifficultySettings
{
    public float dayDuration;
    public float nightDuration;
    public float fullDayLength => dayDuration + nightDuration;
}

public class InGameConstant : Singleton<InGameConstant>
{
    [SerializeField] private DifficultySettings easySettings;
    [SerializeField] private DifficultySettings hardSettings;

    public static Dictionary<Difficulty, DifficultySettings> Settings;

    protected override void Awake()
    {
        base.Awake();

        Settings = new Dictionary<Difficulty, DifficultySettings>
        {
            { Difficulty.Easy, easySettings },
            { Difficulty.Hard, hardSettings }
        };
    }
}