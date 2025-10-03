using System.Collections.Generic;
using UnityEngine;

public static class SpriteSheetIconResolver
{
    // ��Ʈ ��� -> �����̽� �迭 ĳ��
    private static readonly Dictionary<string, Sprite[]> _sheetCache = new();
    // ���� ��������Ʈ ��� ĳ��
    private static readonly Dictionary<string, Sprite> _spriteCache = new();

    /// <summary>
    /// iconPath: 
    ///  - "sheetPath:sliceName"
    ///  - "sheetPath" (sliceName�� itemId/enumName���� ����)
    ///  - "singleSpritePath" (���� ��������Ʈ)
    /// </summary>
    public static Sprite Resolve(string iconPath, int itemId, string enumName)
    {
        iconPath = (iconPath ?? "").Replace("\\", "/").Trim();
        if (string.IsNullOrEmpty(iconPath)) return null;

        // 1) "sheet:slice" ����
        int colon = iconPath.IndexOf(':');
        if (colon >= 0)
        {
            var sheet = iconPath.Substring(0, colon);
            var slice = iconPath.Substring(colon + 1);
            var sp = FindInSheet(sheet, slice);
            if (sp != null) return sp;
            // ���н� �ļ����� ���� ��������Ʈ �ε� �õ�
        }
        else
        {
            // 2) "sheet" �� �� ���: itemId -> enumName ������ �����̽� Ž��
            var sp = FindInSheet(iconPath, itemId.ToString());
            if (sp != null) return sp;

            if (!string.IsNullOrEmpty(enumName))
            {
                sp = FindInSheet(iconPath, enumName);
                if (sp != null) return sp;
            }
        }

        // 3) ���� ��������Ʈ ��� �õ�
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
            // Multiple Sprites�� �����̽��� ��Ʈ �ε�
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
