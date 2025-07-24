using System.Collections.Generic;
using UnityEngine;

public enum PlayerAction
{
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    Attack,
    Interaction,
    Inventory,
    UseItem,
    Escape
}

public static class KeySetting
{
    private static readonly Dictionary<PlayerAction, KeyCode> actionKeys = new();
    private static readonly List<KeyCode> quickSlotKeys = new();

    public static KeyCode GetKey(PlayerAction action)
    {
        if (!actionKeys.TryGetValue(action, out var key))
        {
            Debug.LogError($"Key for {action} is not set.");
            return KeyCode.None;
        }

        return key;
    }

    public static KeyCode GetQuickSlotKey(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= quickSlotKeys.Count)
        {
            Debug.LogError($"QuickSlot index {slotIndex} out of range.");
            return KeyCode.None;
        }

        return quickSlotKeys[slotIndex];
    }

    public static void SetKey(PlayerAction action, KeyCode key)
    {
        if (actionKeys.ContainsKey(action)) actionKeys[action] = key;
        else actionKeys.Add(action, key);
    }

    public static void SetQuickSlotKey(int slotIndex, KeyCode key)
    {
        while (quickSlotKeys.Count <= slotIndex) quickSlotKeys.Add(KeyCode.None);
        quickSlotKeys[slotIndex] = key;
    }
}

public class KeyManager : SingletonObject<KeyManager>
{
    private readonly KeyCode[] defaultActionKeys ={
        KeyCode.W,
        KeyCode.S,
        KeyCode.A,
        KeyCode.D,
        KeyCode.Mouse0,
        KeyCode.F,
        KeyCode.Tab,
        KeyCode.Mouse0,
        KeyCode.Escape
    };

    private readonly KeyCode[] defaultQuickSlotKeys = {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
    };

    public KeyCode[] GetActionKeyCodes() => defaultActionKeys;
    public KeyCode[] GetQuickSlotKeyCodes() => defaultQuickSlotKeys;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < defaultActionKeys.Length; i++)
            KeySetting.SetKey((PlayerAction)i, defaultActionKeys[i]);

        for (int i = 0; i < defaultQuickSlotKeys.Length; i++)
            KeySetting.SetQuickSlotKey(i, defaultQuickSlotKeys[i]);
    }
}