using System.Collections.Generic;
using UnityEngine;

/// TSV���� ���� 'ǥ�� �̸�/����/������'�� id�� ����
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
        // ������ �ڵ� �ؼ�(��Ʈ:slice �Ǵ� ���� ��������Ʈ)
        Sprite icon = SpriteSheetIconResolver.Resolve(iconPath, id, "");
        _byId[id] = new Row { id = id, displayName = displayName ?? "", description = description ?? "", icon = icon };
    }

    public static Row Get(int id) => _byId.TryGetValue(id, out var r) ? r : null;
}