using UnityEngine;
using UnityEngine.UI;

public class TimerUI : SingletonObject<TimerUI>
{
    [SerializeField] private Image dayImage;
    [SerializeField] private GameObject timerHand;

    public void Initialize()
    {
        dayImage.fillAmount = InGameManager.settings.dayDuration / InGameManager.settings.fullDayLength;
    }

    public void UpdateTimerHand(float playTime)
    {
        if (timerHand == null) return;
        timerHand.transform.localRotation = Quaternion.Euler(0, 0, -playTime / InGameManager.settings.fullDayLength * 360);
    }
}