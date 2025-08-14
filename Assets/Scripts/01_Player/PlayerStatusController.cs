using System;

public class PlayerStatusController
{
    private PlayerStatus playerStatus;

    public void Initialize()
    {
        playerStatus = new();
        playerStatus.Initialize();

        foreach (var type in Player.constant.maxStatus.Keys)
        {
            playerStatus.SetMaxStatus(type, Player.constant.maxStatus[type]);
            playerStatus.SetCurrentStatus(type, Player.constant.maxStatus[type]);
        }
    }

    private float thirstDecayRate => Player.constant.thirstDecayRate;
    private float healthDecayRateByThirst => Player.constant.healthDecayRateByThirst;
    private float wellnessDecayRate => Player.constant.GetWellnessDecayRate(GetCurrentStatus(Status.Wellness));
    private float healthDecayRateByWellnessRate
    => Player.constant.GetHealthDecayRateByWellnessRate(GetCurrentStatus(Status.Wellness) / GetMaxStatus(Status.Wellness) * 100);

    public void UpdatePlayerStatus()
    {
        float thirst = ChangeCurrentStatus(Status.Thirst, -thirstDecayRate);
        if (thirst <= 0) ChangeCurrentStatus(Status.HP, -healthDecayRateByThirst);

        ChangeCurrentStatus(Status.Wellness, -wellnessDecayRate);
        ChangeCurrentStatus(Status.HP, -healthDecayRateByWellnessRate);
    }

    // PlayerStatus Methods
    public void AddOnStatChangedEvent(Action<Status, float, float> OnStatChanged) => playerStatus.AddOnStatChangedEvent(OnStatChanged);
    public void RemoveOnStatChangedEvent(Action<Status, float, float> OnStatChanged) => playerStatus.RemoveOnStatChangedEvent(OnStatChanged);
    public float GetCurrentStatus(Status status) => playerStatus.GetCurrentStatus(status);
    public float GetMaxStatus(Status status) => playerStatus.GetMaxStatus(status);
    public float SetMaxStatus(Status status, float value) => playerStatus.SetMaxStatus(status, value);
    public float SetCurrentStatus(Status status, float value) => playerStatus.SetCurrentStatus(status, value);
    public float ChangeCurrentStatus(Status status, float change) => playerStatus.ChangeCurrentStatus(status, change);
}