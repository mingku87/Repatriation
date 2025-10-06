using System;
using System.Collections.Generic;
using UnityEngine;

public static class ItemPresentationDB
{
    // 한 아이템의 표시정보(이름/설명/아이콘)
    public sealed class Row
    {
        public int id;
        public string name;        // ← 소문자 name/description/icon 를 사용합니다.
        public string description;
        public Sprite icon;
    }

    static readonly Dictionary<int, Row> map = new();

    public static Row Get(int id)
        => map.TryGetValue(id, out var r) ? r : null;

    // ✅ 헬퍼 함수들 추가
    public static string GetName(int id)
        => map.TryGetValue(id, out var r) ? r.name : $"(이름없음:{id})";

    public static string GetDescription(int id)
        => map.TryGetValue(id, out var r) ? r.description : string.Empty;

    public static Sprite GetIcon(int id)
        => map.TryGetValue(id, out var r) ? r.icon : null;

    /// <summary>
    /// TSV 로더에서 호출합니다.
    /// iconPath 예시:
    ///  - "Images/ItemIcon/Item_Icon_Hat:10012"  (스프라이트 시트 + 조각 이름/ID)
    ///  - "Images/ItemIcon/Item_Icon_Hat"        (시트 하나만, 첫 조각 또는 id와 같은 이름 매칭)
    /// </summary>
    public static void Register(int id, string name, string description, string iconPath)
    {
        var row = new Row
        {
            id = id,
            name = name ?? string.Empty,
            description = description ?? string.Empty,
            icon = LoadIcon(iconPath, id)
        };
        map[id] = row;
    }

    // Resources에서 아이콘을 찾아옵니다. (슬라이스 시트 지원)
    static Sprite LoadIcon(string iconPath, int id)
    {
        if (string.IsNullOrWhiteSpace(iconPath))
            return null;

        Sprite found = null;

        // "path:10012" 형태 처리
        var parts = iconPath.Split(':');
        var basePath = parts[0].Trim();
        var desiredName = (parts.Length > 1) ? parts[1].Trim() : null;

        // 스프라이트 시트/단일 스프라이트 모두 지원
        var all = Resources.LoadAll<Sprite>(basePath);
        if (all != null && all.Length > 0)
        {
            // 1) "path:조각이름" 지정 시 우선 매칭
            if (!string.IsNullOrEmpty(desiredName))
            {
                found = Array.Find(all, s =>
                    string.Equals(s.name, desiredName, StringComparison.OrdinalIgnoreCase));
            }

            // 2) 못 찾았으면 id와 같은 이름의 조각을 시도
            if (found == null)
                found = Array.Find(all, s => s.name == id.ToString());

            // 3) 그래도 없으면 첫 조각
            if (found == null)
                found = all[0];
        }
        else
        {
            // 단일 스프라이트(슬라이스 없음) 로드
            found = Resources.Load<Sprite>(basePath);
        }

        return found;
    }
}
