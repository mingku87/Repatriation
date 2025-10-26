using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Header("Tables (primary)")]
    public LocalizationTable tableKo;
    public LocalizationTable tableEn;

    [Header("Tables (extra, merged to primary)")]
    public LocalizationTable[] extraKo;   // ← npc, shopnpc 같은 추가 테이블을 여기에
    public LocalizationTable[] extraEn;

    [Header("Current Language")]
    public SystemLanguage language = SystemLanguage.Korean;

    // 내부 맵은 '대문자 키'로 통일하여 대/소문자 무시
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

        // 1) 사용할 테이블 집합 구성
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

        // 2) 맵 재구축 (키는 모두 대문자 정규화)
        _map.Clear();
        foreach (var table in list)
        {
            if (table == null || table.entries == null) continue;
            foreach (var e in table.entries)
            {
                if (string.IsNullOrWhiteSpace(e.key)) continue;
                var k = NormalizeKey(e.key);
                var v = e.value ?? "";
                _map[k] = v; // 뒤에 오는 테이블이 같은 키면 덮어씀
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

    // 공통 정규화: Trim + 대문자 통일
    private static string NormalizeKey(string s) => s.Trim().ToUpperInvariant();
}
