using System.Collections.Generic;
using UnityEngine;

public enum Difficulty
{
    Easy,
    Hard
}

public class DifficultySettings
{
    public float dayDuration;
    public float nightDuration;
    public float fullDayLength => dayDuration + nightDuration;
}

public class InGameConstant
{
    [SerializeField] private DifficultySettings easySettings;
    [SerializeField] private DifficultySettings hardSettings;

    public static Dictionary<Difficulty, DifficultySettings> Settings = new()
    {
        { Difficulty.Easy, new() {
            dayDuration = 4 * 60.0f,
            nightDuration = 6 * 60.0f
        }},
        { Difficulty.Hard, new() {
            dayDuration = 4 * 60.0f,
            nightDuration = 6 * 60.0f
        }}
    };
}