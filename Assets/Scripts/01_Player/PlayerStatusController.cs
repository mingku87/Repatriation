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

        playerStatus.SetMaxStatus(PlayerStatusType.HP, Player.settings.maxHP);
        playerStatus.SetMaxStatus(PlayerStatusType.Thirst, Player.settings.maxThirst);
        playerStatus.SetMaxStatus(PlayerStatusType.Symptom, Player.settings.maxSymptom);

        playerStatus.SetCurrentStatus(PlayerStatusType.HP, Player.settings.maxHP);
        playerStatus.SetCurrentStatus(PlayerStatusType.Thirst, Player.settings.maxThirst);
        playerStatus.SetCurrentStatus(PlayerStatusType.Symptom, Player.settings.maxSymptom);
    }

    public float ChangeStatus(PlayerStatusType statusType, float change)
    {
        return playerStatus.ChangeCurrentStatus(statusType, change);
    }

    private void UpdatePlayerStatus()
    {
    }
}