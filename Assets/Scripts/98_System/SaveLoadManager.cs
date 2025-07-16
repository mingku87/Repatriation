using System.IO;
using UnityEngine;

public static class SaveLoadManager
{
    private static string settingsPath = Path.Combine(Application.persistentDataPath, "settings.json");
    private static string gameSavePath = Path.Combine(Application.persistentDataPath, "save1.json");

    public static void SaveSettings(SettingsData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(settingsPath, json);
    }

    public static SettingsData LoadSettings()
    {
        if (!File.Exists(settingsPath))
            return new SettingsData(); // 기본값 리턴

        string json = File.ReadAllText(settingsPath);
        return JsonUtility.FromJson<SettingsData>(json);
    }

    public static void SaveGame(GameSaveData data, string slot = "save1")
    {
        string path = Path.Combine(Application.persistentDataPath, slot + ".json");
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public static GameSaveData LoadGame(string slot = "save1")
    {
        string path = Path.Combine(Application.persistentDataPath, slot + ".json");
        if (!File.Exists(path))
            return new GameSaveData(); // 새 게임 데이터

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<GameSaveData>(json);
    }
}