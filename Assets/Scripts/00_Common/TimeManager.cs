using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum TimeState
{
    Day,
    Twilight,
    Night,
    Dawn
}

public class TimeManager : SingletonObject<TimeManager>
{
    [SerializeField] private Light2D globalLight;

    private static TimeState _timeState = TimeState.Day;
    public static TimeState TimeState
    {
        get => _timeState;
        set
        {
            if (_timeState == value) return;
            _timeState = value;
            LightManager.ChangeToDayOrNight(_timeState);
        }
    }

    private static float _playTime = 0.0f;
    public static float PlayTime
    {
        get => _playTime;
        set
        {
            if (_playTime == value) return;
            _playTime = value;
            TimeState = PlayTime % fullDayLength < dayDuration ? TimeState.Day : TimeState.Night;
            TimerUI.Instance.UpdateTimerHand(_playTime);
        }
    }
    private static float dayDuration => InGameManager.settings.dayDuration;
    private static float nightDuration => InGameManager.settings.nightDuration;
    private static float fullDayLength => InGameManager.settings.fullDayLength;

    public static float AddPlayTime(float time)
    {
        PlayTime += time;
        return PlayTime;
    }

    public static float SetPlayTime(int day, float time)
    {
        PlayTime = fullDayLength * day + time;
        return PlayTime;
    }

    public static TimeState GetTimeState()
    {
        return PlayTime % fullDayLength < dayDuration ? TimeState.Day : TimeState.Night;
    }

    public static void SkipTime(TimeState timeState)
    {
        if (timeState != GetTimeState()) return;

        int day = GetPlayTime().Item1;
        if (timeState == TimeState.Day) SetPlayTime(day, dayDuration);
        else SetPlayTime(day + 1, 0.0f);
    }

    public static (int, float) GetPlayTime()
    {
        return ((int)(PlayTime / fullDayLength), PlayTime % fullDayLength);
    }
}