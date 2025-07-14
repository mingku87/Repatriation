using System;
using UnityEngine;

public class PlayerStatusController : MonoBehaviour
{
    private PlayerStatus playerStatus;
    private event Action<PlayerStatusType, float, float> OnStatChanged;
    public void AddOnStatChangedEvent(Action<PlayerStatusType, float, float> OnStatChanged) { this.OnStatChanged += OnStatChanged; }
    public void RemoveOnStatChangedEvent(Action<PlayerStatusType, float, float> OnStatChanged) { this.OnStatChanged -= OnStatChanged; }

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
}