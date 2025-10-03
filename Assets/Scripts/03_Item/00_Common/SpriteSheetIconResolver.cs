using System.Collections.Generic;
using UnityEngine;

public static class SpriteSheetIconResolver
{
    // 시트 경로 -> 슬라이스 배열 캐시
    private static readonly Dictionary<string, Sprite[]> _sheetCache = new();
    // 개별 스프라이트 경로 캐시
    private static readonly Dictionary<string, Sprite> _spriteCache = new();

    /// <summary>
    /// iconPath: 
    ///  - "sheetPath:sliceName"
    ///  - "sheetPath" (sliceName은 itemId/enumName으로 유추)
    ///  - "singleSpritePath" (단일 스프라이트)
    /// </summary>
    public static Sprite Resolve(string iconPath, int itemId, string enumName)
    {
        iconPath = (iconPath ?? "").Replace("\\", "/").Trim();
        if (string.IsNullOrEmpty(iconPath)) return null;

        // 1) "sheet:slice" 포맷
        int colon = iconPath.IndexOf(':');
        if (colon >= 0)
        {
            var sheet = iconPath.Substring(0, colon);
            var slice = iconPath.Substring(colon + 1);
            var sp = FindInSheet(sheet, slice);
            if (sp != null) return sp;
            // 실패시 후순위로 단일 스프라이트 로드 시도
        }
        else
        {
            // 2) "sheet" 만 온 경우: itemId -> enumName 순으로 슬라이스 탐색
            var sp = FindInSheet(iconPath, itemId.ToString());
            if (sp != null) return sp;

            if (!string.IsNullOrEmpty(enumName))
            {
                sp = FindInSheet(iconPath, enumName);
                if (sp != null) return sp;
            }
        }

        // 3) 단일 스프라이트 경로 시도
        if (_spriteCache.TryGetValue(iconPath, out var single)) return single;
        var loaded = Resources.Load<Sprite>(NormalizeToResourcesKey(iconPath));
        if (loaded) _spriteCache[iconPath] = loaded;
        return loaded;
    }

    private static Sprite FindInSheet(string sheetPath, string sliceName)
    {
        if (string.IsNullOrEmpty(sheetPath) || string.IsNullOrEmpty(sliceName)) return null;

        if (!_sheetCache.TryGetValue(sheetPath, out var arr) || arr == null || arr.Length == 0)
        {
            // Multiple Sprites로 슬라이스된 시트 로드
            arr = Resources.LoadAll<Sprite>(NormalizeToResourcesKey(sheetPath));
            _sheetCache[sheetPath] = arr;
        }
        if (arr == null) return null;

        for (int i = 0; i < arr.Length; i++)
        {
            var s = arr[i];
            if (s != null && s.name == sliceName)
                return s;
        }
        return null;
    }

    private static string NormalizeToResourcesKey(string raw)
    {
        var p = (raw ?? "").Replace("\\", "/");
        if (p.StartsWith("Assets/Resources/")) p = p.Substring("Assets/Resources/".Length);
        if (p.EndsWith(".png")) p = p[..^4];
        return p;
    }
}
