using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalizationTable", menuName = "Localization/Localization Table")]
public class LocalizationTable : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public string key;
        [TextArea] public string value;
    }

    public SystemLanguage language = SystemLanguage.Korean;
    public List<Entry> entries = new();

    public bool TryGet(string key, out string val)
    {
        // 선형탐색이라 런타임 캐시를 쓰는 쪽이 좋음 (아래 Manager가 캐시)
        for (int i = 0; i < entries.Count; i++)
            if (string.Equals(entries[i].key, key, StringComparison.Ordinal))
            { val = entries[i].value; return true; }

        val = default;
        return false;
    }
}
