using UnityEngine;

public static class SettingsBridge
{
    public static UserSettingsRuntimeData ToRuntime(UserSettingsSaveData s)
    {
        var r = new UserSettingsRuntimeData
        {
            volumes = new(){
                { AudioType.Master, s.master },
                { AudioType.BGM, s.bgm },
                { AudioType.SFX, s.sfx }
            },
            actionKeys = new(),
            quickSlotKeys = new()
        };

        for (int i = 0; i < s.actionKeys.Length; i++) r.actionKeys[(PlayerAction)i] = (KeyCode)s.actionKeys[i];
        for (int i = 0; i < s.quickSlotKeys.Length; i++) r.quickSlotKeys.Add((KeyCode)s.quickSlotKeys[i]);

        return r;
    }

    public static UserSettingsSaveData ToSave(UserSettingsRuntimeData r)
    {
        var save = new UserSettingsSaveData
        {
            master = r.volumes[AudioType.Master],
            bgm = r.volumes[AudioType.BGM],
            sfx = r.volumes[AudioType.SFX],
            actionKeys = new int[Util.GetEnumLength<PlayerAction>()],
            quickSlotKeys = new int[r.quickSlotKeys.Count]
        };

        for (int i = 0; i < Util.GetEnumLength<PlayerAction>(); i++) save.actionKeys[i] = (int)r.actionKeys[(PlayerAction)i];
        for (int i = 0; i < r.quickSlotKeys.Count; i++) save.quickSlotKeys[i] = (int)r.quickSlotKeys[i];

        return save;
    }

    public static void ApplyToSystems(UserSettingsRuntimeData r)
    {
        AudioManager.Instance.SetAudioVolume(r.volumes);
        KeyManager.SetKeys(r.actionKeys);
        KeyManager.SetQuickSlotKeys(r.quickSlotKeys);
    }

    public static UserSettingsRuntimeData CaptureRuntime(
        KeyCode[] actionKeyCodes,
        KeyCode[] quickSlots)
    {
        var r = new UserSettingsRuntimeData
        {
            volumes = AudioManager.Instance.GetVolume(),
            actionKeys = new(),
            quickSlotKeys = new()
        };

        for (int i = 0; i < actionKeyCodes.Length; i++) r.actionKeys[(PlayerAction)i] = actionKeyCodes[i];
        for (int i = 0; i < quickSlots.Length; i++) r.quickSlotKeys.Add(quickSlots[i]);

        return r;
    }
}
