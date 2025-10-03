using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public static class ItemParameterList
{
    public static readonly List<ItemParameter> itemStats = new();

    public static void LoadFromTSV(string fileName = "Item_data.tsv")
    {
        itemStats.Clear();

        var path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"[ItemParameterList] TSV not found: {path}");
            return;
        }

        var lines = File.ReadAllLines(path);
        if (lines.Length < 2) return; // header만 존재

        // 컬럼 (네 시트 기준)
        // 0:index  1:item_id  2:item_name(표시용)  3:item_type  4:detail_type
        // 5:effect_type1  6:effect_value1  7:effect_type2  8:effect_value2
        // 9:maxstack  10:weight  11:quality  12:buy_price  13:sell_price
        // 14:description  15:icon_path

        for (int i = 1; i < lines.Length; i++)
        {
            var raw = lines[i];
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var c = raw.Split('\t');

            if (c.Length < 14)
            {
                Debug.LogWarning($"[ItemParameterList] skip line {i + 1}: col={c.Length}");
                continue;
            }

            // 필수
            int id = ParseInt(Safe(c, 1), -1);
            if (id < 0)
            {
                Debug.LogWarning($"[ItemParameterList] line {i + 1}: invalid item_id='{Safe(c, 1)}'");
                continue;
            }

            string itemTypeStr = Safe(c, 3); // equip/use/water/etc
            string detailStr = Safe(c, 4); // body/elbow/glove/knee/shoes/bag/hat...
            string eff1Str = Safe(c, 5); // hp/def/spd/wgh/thirst/slot/symptoms
            float eff1Val = ParseFloat(Safe(c, 6), 0f);
            // effect2는 현재 구조에선 미사용(필요하면 확장)
            // string eff2Str   = Safe(c, 7);
            // float  eff2Val   = ParseFloat(Safe(c, 8), 0f);

            int maxstack = ParseInt(Safe(c, 9), 1);
            float weight = ParseFloat(Safe(c, 10), 0f);
            int quality = ParseInt(Safe(c, 11), 0);
            // 가격/설명/아이콘은 파라미터가 아니라 UI/상점 쪽에서 사용
            // int buyPrice     = ParseInt(Safe(c,12), 0);
            // int sellPrice    = ParseInt(Safe(c,13), 0);
            // string desc      = Safe(c,14);
            // string iconPath  = Safe(c,15);

            // 현재 enum ItemName과 시트의 item_name(한글)이 매칭되지 않음 → 임시 None
            ItemName itemName = ItemName.None;

            var itemType = MapItemType(itemTypeStr);

            switch (itemType)
            {
                case ItemType.Equipment:
                    {
                        var part = MapEquipmentPart(detailStr);
                        var stat1 = MapStatus(eff1Str);

                        // 생성자: (id, itemName, weight, EquipmentPart, Status, value, maxDurability=100, decay=1)
                        var p = new ItemParameterEquipment(id, itemName, weight, part, stat1, eff1Val);
                        p.type = ItemType.Equipment;
                        // maxstack은 장비는 1로 고정이라 필요시 무시
                        itemStats.Add(p);
                        break;
                    }

                case ItemType.Consumable:
                    {
                        var stat1 = MapStatus(eff1Str);
                        // 생성자: (id, itemName, weight, Status, value)
                        var p = new ItemParameterConsumable(id, itemName, weight, stat1, eff1Val);
                        p.type = ItemType.Consumable;
                        // 필요하면 p.maxstack 같은 확장 필드 추가해서 보관
                        itemStats.Add(p);
                        break;
                    }

                case ItemType.Water:
                    {
                        // 생성자: (id, itemName, weight, value, quality)
                        // (Status는 내부에서 Thirst로 고정됨)
                        var p = new ItemParameterWater(id, itemName, weight, eff1Val, quality);
                        p.type = ItemType.Water;
                        itemStats.Add(p);
                        break;
                    }

                default:
                    {
                        // 기타(etc) → 기본 파라미터로 보관(로직 없음)
                        var p = new ItemParameter(id, itemName, weight);
                        p.type = ItemType.None;
                        itemStats.Add(p);
                        break;
                    }
            }
        }

        Debug.Log($"[ItemParameterList] Loaded {itemStats.Count} items from TSV");
    }

    public static ItemParameter GetItemStat(int id)
    {
        for (int i = 0; i < itemStats.Count; i++)
            if (itemStats[i].id == id) return itemStats[i];
        return null;
    }

    // ────────── helpers ──────────
    static string Safe(string[] arr, int idx) => (idx < arr.Length) ? arr[idx].Trim() : "";

    static int ParseInt(string s, int fallback = 0)
        => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : fallback;

    static float ParseFloat(string s, float fallback = 0f)
        => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : fallback;

    static ItemType MapItemType(string s)
    {
        switch ((s ?? "").Trim().ToLowerInvariant())
        {
            case "equip": return ItemType.Equipment;
            case "use": return ItemType.Consumable;
            case "water": return ItemType.Water;
            case "etc": return ItemType.None;
            default: return ItemType.None;
        }
    }

    // detail_type → EquipmentPart 매핑(시트 값 표준화)
    static EquipmentPart MapEquipmentPart(string s)
    {
        switch ((s ?? "").Trim().ToLowerInvariant())
        {
            case "hat": return EquipmentPart.Head;
            case "body": return EquipmentPart.Body;
            case "elbow": return EquipmentPart.Arms;
            case "glove": return EquipmentPart.Hands;
            case "knee": return EquipmentPart.Knees;
            case "shoes": return EquipmentPart.Feet;
            case "bag": return EquipmentPart.Bag;
            default: return EquipmentPart.Body; // 기본값
        }
    }

    // effect_type → Status 매핑(가능한 축약/소문자 대응)
    static Status MapStatus(string s)
    {
        var key = (s ?? "").Trim().ToLowerInvariant();
        // 먼저 enum 직접 파싱 시도(이미 정확한 값이면 통과)
        if (System.Enum.TryParse<Status>(s, true, out var parsed))
            return parsed;

        // 별칭/축약 대응
        switch (key)
        {
            case "hp": return Status.HP;
            case "def": return Status.Def;
            case "spd": return Status.Speed;
            case "wgh": return Status.WGH;     // 무게 제한 등
            case "thirst": return Status.Thirst;
            case "symptom": return Status.Symptom;
            case "slot": return Status.Slot;
            default: return default; // 정의 안 된 경우
        }
    }
}
