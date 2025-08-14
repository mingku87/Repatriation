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
        AudioManager.Instance.SetAudioVolumes(r.volumes);
        KeyManager.SetKeys(r.actionKeys);
        KeyManager.SetQuickSlotKeys(r.quickSlotKeys);
    }

    public static void SaveSettings()
    {
        UserSettingsRuntimeData runtimeData = CaptureRuntime();
        UserSettingsSaveData saveData = ToSave(runtimeData);
        UserSettingsIO.Save(saveData);
    }

    public static UserSettingsRuntimeData CaptureRuntime()
    {
        return new UserSettingsRuntimeData
        {
            volumes = AudioManager.GetVolumes(),
            actionKeys = KeyManager.GetKeys(),
            quickSlotKeys = KeyManager.GetQuickSlotKeys()
        };
    }
}