using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusController : MonoBehaviour
{
    private PlayerStatus playerStatus;

    public void Initialize()
    {
        playerStatus = new PlayerStatus();

        playerStatus.SetMaxStatus(PlayerStatusType.HP, PlayerConstant.MaxHP);
        playerStatus.SetMaxStatus(PlayerStatusType.Thirst, PlayerConstant.MaxThirst);
        playerStatus.SetMaxStatus(PlayerStatusType.Symptom, PlayerConstant.MaxSymptom);

        playerStatus.SetCurrentStatus(PlayerStatusType.HP, PlayerConstant.MaxHP);
        playerStatus.SetCurrentStatus(PlayerStatusType.Thirst, PlayerConstant.MaxThirst);
        playerStatus.SetCurrentStatus(PlayerStatusType.Symptom, PlayerConstant.MaxSymptom);
    }

    public float ChangeStatus(PlayerStatusType statusType, float change)
    {
        return playerStatus.ChangeCurrentStatus(statusType, change);
    }
}
