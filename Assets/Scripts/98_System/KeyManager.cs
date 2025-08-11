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

public class KeyManager : SingletonObject<KeyManager>
{
    private static Dictionary<PlayerAction, KeyCode> actionKeys = new();
    private static List<KeyCode> quickSlotKeys = new();
    [SerializeField] private List<KeySettingButton> keySettingButtons;

    public void Initialize() => keySettingButtons.ForEach(ksb => ksb.Initialize());
    public static void SetKeys(Dictionary<PlayerAction, KeyCode> keys) => actionKeys = new(keys);
    public static void SetQuickSlotKeys(List<KeyCode> keys) => quickSlotKeys = new(keys);
    public static void SetKey(PlayerAction action, KeyCode key) => actionKeys[action] = key;
    public static void SetQuickSlotKey(int slotIndex, KeyCode key) => quickSlotKeys[slotIndex] = key;
    public static Dictionary<PlayerAction, KeyCode> GetKeys() => actionKeys;
    public static List<KeyCode> GetQuickSlotKeys() => quickSlotKeys;

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
}