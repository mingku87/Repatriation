using System;
using System.Collections.Generic;
using UnityEngine;

public enum Status
{
    HP,
    Thirst,
    Symptom,
    Atk,
    AtkRange,
    Def,
    Speed,
    SightRange,
}

public class PlayerStatus
{
    private Dictionary<Status, float> currentStatus = new();
    private Dictionary<Status, float> maxStatus = new();

    private event Action<Status, float, float> OnStatChanged;
    public void AddOnStatChangedEvent(Action<Status, float, float> OnStatChanged) => this.OnStatChanged += OnStatChanged;
    public void RemoveOnStatChangedEvent(Action<Status, float, float> OnStatChanged) => this.OnStatChanged -= OnStatChanged;
    public float GetCurrentStatus(Status status) => currentStatus[status];
    public float GetMaxStatus(Status status) => maxStatus[status];

    public void Initialize()
    {
        foreach (Status type in Enum.GetValues(typeof(Status)))
        {
            currentStatus[type] = 0f;
            maxStatus[type] = 0f;
        }
    }

    public float SetMaxStatus(Status status, float value)
    {
        if (maxStatus[status] == value) return maxStatus[status];

        maxStatus[status] = value;
        if (currentStatus[status] > maxStatus[status]) { currentStatus[status] = maxStatus[status]; }
        OnStatChanged?.Invoke(status, currentStatus[status], maxStatus[status]);
        return maxStatus[status];
    }

    public float SetCurrentStatus(Status status, float value)
    {
        value = Mathf.Clamp(value, 0.0f, maxStatus[status]);
        if (currentStatus[status] == value) return currentStatus[status];

        currentStatus[status] = value;
        OnStatChanged?.Invoke(status, currentStatus[status], maxStatus[status]);
        return currentStatus[status];
    }

    public float ChangeCurrentStatus(Status status, float change)
    {
        float value = Mathf.Clamp(currentStatus[status] + change, 0.0f, maxStatus[status]);
        if (currentStatus[status] == value) return currentStatus[status];

        currentStatus[status] = value;
        OnStatChanged?.Invoke(status, currentStatus[status], maxStatus[status]);
        return currentStatus[status];
    }
}