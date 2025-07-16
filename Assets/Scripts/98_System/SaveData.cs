using System.Collections.Generic;

[System.Serializable]
public class SettingsData
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public string keyBindingJson; // 키맵핑 같은 건 직접 JSON 직렬화해서 문자열로 넣을 수도 있어요
}

[System.Serializable]
public class GameSaveData
{
    public int currentLevel;
    public List<string> completedQuests;
    public int playerCoins;
    public Dictionary<string, int> inventory;
    // …필요한 필드들 추가
}
