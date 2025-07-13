using UnityEngine;

public class TimerUI : Singleton<TimerUI>
{
    [SerializeField] private GameObject timerHand;

    public void UpdateTimerHand(float playTime)
    {
        if (timerHand == null) return;
        timerHand.transform.localRotation = Quaternion.Euler(0, 0, -playTime / GameConstant.fullDayLength * 360);
    }
}