using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Header("Tables (auto-assigned by bootstrap)")]
    public LocalizationTable tableKo;
    public LocalizationTable tableEn;

    [Header("Current Language")]
    public SystemLanguage language = SystemLanguage.Korean;

    private readonly Dictionary<string, string> _map = new();

    public delegate void LanguageChanged();
    public event LanguageChanged OnLanguageChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ���̺��� ���� ����� �� ������(��Ʈ��Ʈ������ ä����) ������� �ϴ� �õ�
        if (_map.Count == 0) LoadLanguage(language);
    }

    public void LoadLanguage(SystemLanguage lang)
    {
        language = lang;

        LocalizationTable table = null;
        if (lang == SystemLanguage.English && tableEn != null) table = tableEn;
        else if (tableKo != null) table = tableKo;

        _map.Clear();
        if (table != null)
        {
            foreach (var e in table.entries)
            {
                if (string.IsNullOrWhiteSpace(e.key)) continue;
                var k = e.key.Trim();              // �ʿ��ϸ� ToUpperInvariant() ����
                var v = e.value ?? "";
                _map[k] = v;
            }
        }

        OnLanguageChanged?.Invoke();
    }

    public string GetOrKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return key;
        var k = key.Trim(); // �ʿ��ϸ� ToUpperInvariant() ����
        return _map.TryGetValue(k, out var v) ? v : key;
    }
}
