using System.Collections.Generic;
using UnityEngine;

/// TSV에서 읽은 '표시 이름/설명/아이콘'을 id로 보관
public static class ItemPresentationDB
{
    public class Row
    {
        public int id;
        public string displayName;
        public string description;
        public Sprite icon;
    }

    static readonly Dictionary<int, Row> _byId = new();

    public static void Register(int id, string displayName, string description, string iconPath)
    {
        // 아이콘 자동 해석(시트:slice 또는 단일 스프라이트)
        Sprite icon = SpriteSheetIconResolver.Resolve(iconPath, id, "");
        _byId[id] = new Row { id = id, displayName = displayName ?? "", description = description ?? "", icon = icon };
    }

    public static Row Get(int id) => _byId.TryGetValue(id, out var r) ? r : null;
}