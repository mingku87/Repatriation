using System.Runtime.InteropServices.WindowsRuntime;
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
            LightManager.ChangeTimeState(_timeState);
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
            SetTimeState();
            TimerUI.Instance.UpdateTimerHand(_playTime);
        }
    }
    private static float dayDuration => InGameManager.settings.dayDuration;
    private static float twilightDuration => InGameManager.settings.twilightDuration;
    private static float nightDuration => InGameManager.settings.nightDuration;
    private static float dawnDuration => InGameManager.settings.dawnDuration;
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

    public static void SetTimeState()
    {
        float time = GetPlayTime().Item2;
        if (time < dayDuration - twilightDuration) TimeState = TimeState.Day;
        else if (time < dayDuration) TimeState = TimeState.Twilight;
        else if (time < fullDayLength - dawnDuration) TimeState = TimeState.Night;
        else TimeState = TimeState.Dawn;
    }

    public static void SkipTime(TimeState timeState)
    {
        int day = GetPlayTime().Item1;
        if (timeState == TimeState.Day) SetPlayTime(day, dayDuration);
        else SetPlayTime(day + 1, 0.0f);
    }

    public static (int, float) GetPlayTime()
    {
        return ((int)(PlayTime / fullDayLength), PlayTime % fullDayLength);
    }
}