using System;
using System.Collections.Generic;
using UnityEngine;

public static class ItemPresentationDB
{
    // �� �������� ǥ������(�̸�/����/������)
    public sealed class Row
    {
        public int id;
        public string name;        // �� �ҹ��� name/description/icon �� ����մϴ�.
        public string description;
        public Sprite icon;
    }

    static readonly Dictionary<int, Row> map = new();

    public static Row Get(int id)
        => map.TryGetValue(id, out var r) ? r : null;

    /// <summary>
    /// TSV �δ����� ȣ���մϴ�.
    /// iconPath ����:
    ///  - "Images/ItemIcon/Item_Icon_Hat:10012"  (��������Ʈ ��Ʈ + ���� �̸�/ID)
    ///  - "Images/ItemIcon/Item_Icon_Hat"        (��Ʈ �ϳ���, ù ���� �Ǵ� id�� ���� �̸� ��Ī)
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

    // Resources���� �������� ã�ƿɴϴ�. (�����̽� ��Ʈ ����)
    static Sprite LoadIcon(string iconPath, int id)
    {
        if (string.IsNullOrWhiteSpace(iconPath))
            return null;

        Sprite found = null;

        // "path:10012" ���� ó��
        var parts = iconPath.Split(':');
        var basePath = parts[0].Trim();
        var desiredName = (parts.Length > 1) ? parts[1].Trim() : null;

        // ��������Ʈ ��Ʈ/���� ��������Ʈ ��� ����
        var all = Resources.LoadAll<Sprite>(basePath);
        if (all != null && all.Length > 0)
        {
            // 1) "path:�����̸�" ���� �� �켱 ��Ī (���� �̸��� ���ڷ� �ᵵ �˴ϴ�)
            if (!string.IsNullOrEmpty(desiredName))
            {
                found = Array.Find(all, s =>
                    string.Equals(s.name, desiredName, StringComparison.OrdinalIgnoreCase));
            }

            // 2) �� ã������ id�� ���� �̸��� ������ �õ� (�����̽����� 10001,10002 ó�� �̸��� ���)
            if (found == null)
                found = Array.Find(all, s => s.name == id.ToString());

            // 3) �׷��� ������ ù ����
            if (found == null)
                found = all[0];
        }
        else
        {
            // ���� ��������Ʈ(�����̽� ����) �ε�
            found = Resources.Load<Sprite>(basePath);
        }

        return found;
    }
}
