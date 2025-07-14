using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusController : MonoBehaviour
{
    private PlayerStatus playerStatus;

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