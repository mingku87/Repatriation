using System.Collections.Generic;
using UnityEngine;

public enum PlayerStatusType
{
    HP,
    Thirst,
    Symptom
}

public class PlayerStatus
{
    private Dictionary<PlayerStatusType, float> currentStatus = new()
    {
        { PlayerStatusType.HP, 0.0f },
        { PlayerStatusType.Thirst, 0.0f },
        { PlayerStatusType.Symptom, 0.0f }
    };

    private Dictionary<PlayerStatusType, float> maxStatus = new()
    {
        { PlayerStatusType.HP, 0.0f },
        { PlayerStatusType.Thirst, 0.0f },
        { PlayerStatusType.Symptom, 0.0f }
    };

    public float GetCurrentStatus(PlayerStatusType status) { return currentStatus[status]; }
    public float GetMaxStatus(PlayerStatusType status) { return maxStatus[status]; }

    public float SetCurrentStatus(PlayerStatusType status, float value)
    {
        currentStatus[status] = Mathf.Clamp(value, 0.0f, maxStatus[status]);
        return currentStatus[status];
    }

    public float SetMaxStatus(PlayerStatusType status, float value)
    {
        maxStatus[status] = value;
        return maxStatus[status];
    }

    public float ChangeCurrentStatus(PlayerStatusType status, float change)
    {
        currentStatus[status] = Mathf.Clamp(currentStatus[status] + change, 0.0f, maxStatus[status]);
        return currentStatus[status];
    }
}
