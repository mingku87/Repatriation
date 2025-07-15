using System;
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
    private Dictionary<PlayerStatusType, float> currentStatus = new();
    private Dictionary<PlayerStatusType, float> maxStatus = new();

    private event Action<PlayerStatusType, float, float> OnStatChanged;
    public void AddOnStatChangedEvent(Action<PlayerStatusType, float, float> OnStatChanged) { this.OnStatChanged += OnStatChanged; }
    public void RemoveOnStatChangedEvent(Action<PlayerStatusType, float, float> OnStatChanged) { this.OnStatChanged -= OnStatChanged; }

    public float GetCurrentStatus(PlayerStatusType status) { return currentStatus[status]; }
    public float GetMaxStatus(PlayerStatusType status) { return maxStatus[status]; }

    private void Awake()
    {
        foreach (PlayerStatusType type in Enum.GetValues(typeof(PlayerStatusType)))
        {
            currentStatus[type] = 0f;
            maxStatus[type] = 0f;
        }
    }

    public float SetCurrentStatus(PlayerStatusType status, float value)
    {
        value = Mathf.Clamp(value, 0.0f, maxStatus[status]);
        if (currentStatus[status] == value) return currentStatus[status];

        currentStatus[status] = value;
        OnStatChanged?.Invoke(status, currentStatus[status], maxStatus[status]);
        return currentStatus[status];
    }

    public float SetMaxStatus(PlayerStatusType status, float value)
    {
        if (maxStatus[status] == value) return maxStatus[status];

        maxStatus[status] = value;
        OnStatChanged?.Invoke(status, currentStatus[status], maxStatus[status]);
        return maxStatus[status];
    }

    public float ChangeCurrentStatus(PlayerStatusType status, float change)
    {
        float value = Mathf.Clamp(currentStatus[status] + change, 0.0f, maxStatus[status]);
        if (currentStatus[status] == value) return currentStatus[status];

        currentStatus[status] = value;
        OnStatChanged?.Invoke(status, currentStatus[status], maxStatus[status]);
        return currentStatus[status];
    }
}