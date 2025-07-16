public class TimeManager : SingletonObject<TimeManager>
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
        PlayTime = InGameManager.settings.fullDayLength * day + time;
        return PlayTime;
    }

    public static TimeState GetTimeState()
    {
        return PlayTime % InGameManager.settings.fullDayLength < InGameManager.settings.dayDuration ? TimeState.Day : TimeState.Night;
    }

    public static void SkipTime(TimeState timeState)
    {
        if (timeState != GetTimeState()) return;

        int day = GetPlayTime().Item1;
        if (timeState == TimeState.Day) SetPlayTime(day, InGameManager.settings.dayDuration);
        else SetPlayTime(day + 1, 0.0f);
    }

    public static (int, float) GetPlayTime()
    {
        return ((int)(PlayTime / InGameManager.settings.fullDayLength), PlayTime % InGameManager.settings.fullDayLength);
    }
}