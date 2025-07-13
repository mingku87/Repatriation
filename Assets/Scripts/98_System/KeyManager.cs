using System.Collections.Generic;
using UnityEngine;

public enum PlayerAction
{
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    Interaction,
}

public static class KeySetting
{
    private static Dictionary<PlayerAction, KeyCode> keys = new Dictionary<PlayerAction, KeyCode>();

    public static KeyCode GetKey(PlayerAction action)
    {
        if (keys.ContainsKey(action))
        {
            return keys[action];
        }
        switch (action)
        {
            case PlayerAction.MoveUp:
                return GetKey(PlayerAction.MoveUp);
            case PlayerAction.MoveDown:
                return GetKey(PlayerAction.MoveDown);
            case PlayerAction.MoveLeft:
                return GetKey(PlayerAction.MoveLeft);
            case PlayerAction.Interaction:
                return GetKey(PlayerAction.Interaction);
            default:
                Debug.LogError("Key for " + action + " is not set.");
                return KeyCode.None;
        }
    }

    public static void SetKey(PlayerAction action, KeyCode key)
    {
        if (keys.ContainsKey(action))
        {
            keys[action] = key;
        }
        else
        {
            keys.Add(action, key);
        }
    }
}

public class KeyManager : Singleton<KeyManager>
{
    private KeyCode[] defaultKeys = new KeyCode[]
    { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.F };


    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < defaultKeys.Length; i++)
        {
            KeySetting.SetKey((PlayerAction)i, defaultKeys[i]);
        }
    }

    public KeyCode[] GetKeyCodes() { return defaultKeys; }
}