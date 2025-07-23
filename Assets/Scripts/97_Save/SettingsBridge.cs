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

    // ---- 편의 함수 ----
    // 바로 시스템에 적용
    public static void ApplyToSystems(UserSettingsRuntimeData r)
    {
        // // 볼륨
        // foreach (var kv in r.volumes)
        //     AudioManager.Instance.SetAudioVolume(kv.Key, kv.Value, updateUI: true);

        // 키
        foreach (var kv in r.actionKeys) KeySetting.SetKey(kv.Key, kv.Value);
        for (int i = 0; i < r.quickSlotKeys.Count; i++) KeySetting.SetQuickSlotKey(i, r.quickSlotKeys[i]);
    }

    // 현재 상태 캡처 (AudioManager/KeyManager에서 값 얻기)
    public static UserSettingsRuntimeData CaptureRuntime(
        int master, int bgm, int sfx,
        KeyCode[] actionKeyCodes,
        KeyCode[] quickSlots)
    {
        var r = new UserSettingsRuntimeData
        {
            volumes = new(){
                { AudioType.Master, master },
                { AudioType.BGM, bgm },
                { AudioType.SFX, sfx }
            },
            actionKeys = new(),
            quickSlotKeys = new()
        };

        for (int i = 0; i < actionKeyCodes.Length; i++) r.actionKeys[(PlayerAction)i] = actionKeyCodes[i];
        for (int i = 0; i < quickSlots.Length; i++) r.quickSlotKeys.Add(quickSlots[i]);

        return r;
    }
}
