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
    private float symptomDecayRate => Player.constant.GetSymptomDecayRate(GetCurrentStatus(Status.Symptom));
    private float healthDecayRateBySymptomRate
    => Player.constant.GetHealthDecayRateBySymptomRate(GetCurrentStatus(Status.Symptom) / GetMaxStatus(Status.Symptom) * 100);

    public void UpdatePlayerStatus()
    {
        float thirst = ChangeCurrentStatus(Status.Thirst, -thirstDecayRate);
        if (thirst <= 0) ChangeCurrentStatus(Status.HP, -healthDecayRateByThirst);

        ChangeCurrentStatus(Status.Symptom, -symptomDecayRate);
        ChangeCurrentStatus(Status.HP, -healthDecayRateBySymptomRate);
    }

    // PlayerStatus Methods
    public void AddOnStatChangedEvent(Action<Status, float, float> OnStatChanged) { playerStatus.AddOnStatChangedEvent(OnStatChanged); }
    public void RemoveOnStatChangedEvent(Action<Status, float, float> OnStatChanged) { playerStatus.RemoveOnStatChangedEvent(OnStatChanged); }
    public float GetCurrentStatus(Status status) { return playerStatus.GetCurrentStatus(status); }
    public float GetMaxStatus(Status status) { return playerStatus.GetMaxStatus(status); }
    public float SetMaxStatus(Status status, float value) { return playerStatus.SetMaxStatus(status, value); }
    public float SetCurrentStatus(Status status, float value) { return playerStatus.SetCurrentStatus(status, value); }
    public float ChangeCurrentStatus(Status status, float change) { return playerStatus.ChangeCurrentStatus(status, change); }
}