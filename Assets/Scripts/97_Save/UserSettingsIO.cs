using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserSettingsSaveData
{
    public int master;
    public int bgm;
    public int sfx;
    public int[] actionKeys;
    public int[] quickSlotKeys;
}

public class UserSettingsRuntimeData
{
    public Dictionary<AudioType, int> volumes;
    public Dictionary<PlayerAction, KeyCode> actionKeys;
    public List<KeyCode> quickSlotKeys;
}

public static class UserSettingsIO
{
    const string SaveKey = "UserSettings_v1";

    public static void Save(UserSettingsSaveData data)
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static bool Load(out UserSettingsSaveData data)
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            data = null;
            return false;
        }
        data = JsonUtility.FromJson<UserSettingsSaveData>(PlayerPrefs.GetString(SaveKey));
        return true;
    }

    public static void Delete() => PlayerPrefs.DeleteKey(SaveKey);
}