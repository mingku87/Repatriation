using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Header("Tables (primary)")]
    public LocalizationTable tableKo;
    public LocalizationTable tableEn;

    [Header("Tables (extra, merged to primary)")]
    public LocalizationTable[] extraKo;   // �� npc, shopnpc ���� �߰� ���̺��� ���⿡
    public LocalizationTable[] extraEn;

    [Header("Current Language")]
    public SystemLanguage language = SystemLanguage.Korean;

    // ���� ���� '�빮�� Ű'�� �����Ͽ� ��/�ҹ��� ����
    private readonly Dictionary<string, string> _map = new();

    public delegate void LanguageChanged();
    public event LanguageChanged OnLanguageChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_map.Count == 0) LoadLanguage(language);
    }

    public void LoadLanguage(SystemLanguage lang)
    {
        language = lang;

        // 1) ����� ���̺� ���� ����
        var list = new List<LocalizationTable>();
        if (lang == SystemLanguage.English)
        {
            if (tableEn != null) list.Add(tableEn);
            if (extraEn != null) list.AddRange(extraEn);
        }
        else
        {
            if (tableKo != null) list.Add(tableKo);
            if (extraKo != null) list.AddRange(extraKo);
        }

        // 2) �� �籸�� (Ű�� ��� �빮�� ����ȭ)
        _map.Clear();
        foreach (var table in list)
        {
            if (table == null || table.entries == null) continue;
            foreach (var e in table.entries)
            {
                if (string.IsNullOrWhiteSpace(e.key)) continue;
                var k = NormalizeKey(e.key);
                var v = e.value ?? "";
                _map[k] = v; // �ڿ� ���� ���̺��� ���� Ű�� ���
            }
        }

        OnLanguageChanged?.Invoke();
    }

    public string GetOrKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return key;
        var k = NormalizeKey(key);
        return _map.TryGetValue(k, out var v) ? v : key;
    }

    // ���� ����ȭ: Trim + �빮�� ����
    private static string NormalizeKey(string s) => s.Trim().ToUpperInvariant();
}
