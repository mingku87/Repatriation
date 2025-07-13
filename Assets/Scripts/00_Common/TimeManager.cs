public class TimeManager : Singleton<TimeManager>
{
    public enum TimeState
    {
        Day,
        Night
    }
    private static float _playTime = 0.0f;
    public static float PlayTime
    {
        get => _playTime;
        set
        {
            if (_playTime == value) return;
            _playTime = value;
            TimerUI.Instance.UpdateTimerHand(_playTime);
        }
    }

    public static float AddPlayTime(float time)
    {
        PlayTime += time;
        return PlayTime;
    }

    public static float SetPlayTime(int day, float time)
    {
        PlayTime = GameConstant.fullDayLength * day + time;
        return PlayTime;
    }

    public static TimeState GetTimeState()
    {
        return PlayTime % GameConstant.fullDayLength < GameConstant.dayDuration ? TimeState.Day : TimeState.Night;
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
        return ((int)(PlayTime / GameConstant.fullDayLength), PlayTime % GameConstant.fullDayLength);
    }
}