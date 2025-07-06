public class TimeManager : Singleton<TimeManager>
{
    public enum TimeState
    {
        Day,
        Night
    }

    public static float playTime = 0.0f;

    public static float AddPlayTime(float time)
    {
        playTime += time;
        return playTime;
    }

    public static float SetPlayTime(int day, float time)
    {
        playTime = GameConstant.fullDayLength * day + time;
        return playTime;
    }

    public static TimeState GetTimeState()
    {
        return playTime % GameConstant.fullDayLength < GameConstant.dayDuration ? TimeState.Day : TimeState.Night;
    }

    public static void SkipTime(TimeState timeState)
    {
        if (timeState != GetTimeState()) return;

        int day = GetPlayTime().Item1;
        if (timeState == TimeState.Day) SetPlayTime(day, GameConstant.dayDuration);
        else SetPlayTime(day + 1, 0.0f);
    }

    public static (int, float) GetPlayTime()
    {
        return ((int)(playTime / GameConstant.fullDayLength), playTime % GameConstant.fullDayLength);
    }
}
