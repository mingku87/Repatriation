using System;
using UnityEngine;

public class PlayerStatusController : MonoBehaviour
{
    private PlayerStatus playerStatus;
    public void AddOnStatChangedEvent(Action<PlayerStatusType, float, float> OnStatChanged) { playerStatus.AddOnStatChangedEvent(OnStatChanged); }
    public void RemoveOnStatChangedEvent(Action<PlayerStatusType, float, float> OnStatChanged) { playerStatus.RemoveOnStatChangedEvent(OnStatChanged); }

    public void Initialize()
    {
        playerStatus = new PlayerStatus();

        playerStatus.SetMaxStatus(PlayerStatusType.HP, Player.constant.maxHP);
        playerStatus.SetMaxStatus(PlayerStatusType.Thirst, Player.constant.maxThirst);
        playerStatus.SetMaxStatus(PlayerStatusType.Symptom, Player.constant.maxSymptom);

        playerStatus.SetCurrentStatus(PlayerStatusType.HP, Player.constant.maxHP);
        playerStatus.SetCurrentStatus(PlayerStatusType.Thirst, Player.constant.maxThirst);
        playerStatus.SetCurrentStatus(PlayerStatusType.Symptom, Player.constant.maxSymptom);
    }

    public float ChangeStatus(PlayerStatusType statusType, float change)
    {
        return playerStatus.ChangeCurrentStatus(statusType, change);
    }

    private void UpdatePlayerStatus()
    {
        float thirst = playerStatus.ChangeCurrentStatus(PlayerStatusType.Thirst, -Player.constant.thirstDecayRate);
        if (thirst <= 0) playerStatus.ChangeCurrentStatus(PlayerStatusType.HP, -Player.constant.healthDecayRateByThirst);

        float symptom = playerStatus.ChangeCurrentStatus(PlayerStatusType.Symptom,
            -Player.constant.GetSymptomDecayRate(playerStatus.GetCurrentStatus(PlayerStatusType.Symptom)));
        playerStatus.ChangeCurrentStatus(PlayerStatusType.HP,
            -Player.constant.GetHealthDecayRateBySymptomRate(symptom / playerStatus.GetMaxStatus(PlayerStatusType.Symptom) * 100));
    }
}