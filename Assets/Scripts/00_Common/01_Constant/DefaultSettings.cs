using System.Collections.Generic;
using UnityEngine;

public static class DefaultSettings
{
    public const int volumeMax = 100;
    public const int volumeMin = 0;

    public static readonly Dictionary<AudioType, int> defaultVolumes = new(){
        { AudioType.Master, 50 },
        { AudioType.BGM, 100 },
        { AudioType.SFX, 100 }
    };

    public static readonly Dictionary<PlayerAction, KeyCode> defaultActionKeys = new(){
        { PlayerAction.MoveUp, KeyCode.W },
        { PlayerAction.MoveDown, KeyCode.S },
        { PlayerAction.MoveLeft, KeyCode.A },
        { PlayerAction.MoveRight, KeyCode.D },
        { PlayerAction.Interaction, KeyCode.F },
        { PlayerAction.Inventory, KeyCode.Tab },
        { PlayerAction.UseItem, KeyCode.Mouse0 },
        { PlayerAction.Escape, KeyCode.Escape }
    };

    public static readonly List<KeyCode> defaultQuickSlotKeys = new(){
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6
    };
}