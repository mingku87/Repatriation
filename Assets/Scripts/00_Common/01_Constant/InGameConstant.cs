using System.Collections.Generic;

public enum Difficulty
{
    Easy,
    Hard
}

public class DifficultySettings
{
    public float dayDuration;
    public float twilightDuration;
    public float nightDuration;
    public float dawnDuration;
    public float fullDayLength => dayDuration + nightDuration;
}

public class InGameConstant
{
    public static float dayLightIntensity = 0.5f;
    public static float nightLightIntensity = 0.03f;
    public static float lightTransitionDuration = 1.0f;

    public static float cameraSpeed = 3.0f;
    public static float cameraOffsetY = 0.0f;
    public static float cameraOffsetZ = -10.0f;

    public static Dictionary<Difficulty, DifficultySettings> Settings = new()
    {
        { Difficulty.Easy, new() {
            dayDuration = 4 * 60.0f,
            twilightDuration = 10.0f,
            nightDuration = 6 * 60.0f,
            dawnDuration = 10.0f
        }},
        { Difficulty.Hard, new() {
            dayDuration = 4 * 60.0f,
            twilightDuration = 10.0f,
            nightDuration = 6 * 60.0f,
            dawnDuration = 10.0f
        }}
    };
}