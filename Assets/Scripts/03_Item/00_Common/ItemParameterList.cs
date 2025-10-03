using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// TSV에서 아이템 파라미터를 읽어와 런타임 DB를 구성한다.
/// 수치(파라미터)는 itemStats에, 표시정보(이름/설명/아이콘)는 ItemPresentationDB에 등록.
/// </summary>
//#define ICON_TEST
public static class ItemParameterList
{
    // 런타임에서 참조할 파라미터 목록
    public static readonly List<ItemParameter> itemStats = new();
    public static readonly Dictionary<int, (Status status, float value)> effect2ById = new();

    /// <summary>
    /// StreamingAssets/<paramref name="fileName"/> (기본: Item_data.tsv)을 읽어 아이템을 로드한다.
    /// 시트 컬럼(0-base):
    /// 0:index  1:item_id  2:item_name  3:item_type  4:detail_type
    /// 5:effect_type1  6:effect_value1  7:effect_type2  8:effect_value2
    /// 9:maxstack  10:weight  11:quality  12:buy_price  13:sell_price
    /// 14:description  15:icon_path
    /// </summary>
    public static void LoadFromTSV(string fileName = "Item_data.tsv")
    {
        itemStats.Clear();
        effect2ById.Clear();

        var path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"[ItemParameterList] TSV not found: {path}");
            return;
        }

        var lines = File.ReadAllLines(path);
        if (lines.Length < 2) return; // header-only

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

            // ── 공통 파싱 ──────────────────────────────────────────
            int id = ParseInt(Safe(c, 1), -1);     // item_id
            if (id < 0) { Warn(i, "item_id"); continue; }

            string itemNameStr = Safe(c, 2);                // item_name(표시용 텍스트 또는 enum 키)
            string itemTypeStr = Safe(c, 3);                // equip/use/water/etc
            string detailStr = Safe(c, 4);                // body/elbow/glove/knee/shoes/bag/hat...
            string eff1Str = Safe(c, 5);                // hp/def/spd/wgh/slot/thirst/symptom...
            float eff1Val = ParseFloat(Safe(c, 6), 0f);
            string eff2Str = Safe(c, 7);
            float eff2Val = ParseFloat(Safe(c, 8), 0f);
            int maxstackTSV = ParseInt(Safe(c, 9), 1);
            float weight = ParseFloat(Safe(c, 10), 0f);
            int quality = ParseInt(Safe(c, 11), 0);
            string description = Safe(c, 14);
            string iconPath = (c.Length > 15) ? Safe(c, 15) : string.Empty;

            // 효과2(뷰어 표시용) 저장
            if (!string.IsNullOrEmpty(eff2Str))
            {
                var s2 = MapStatus(eff2Str);
                if (!s2.Equals(default(Status)))
                    effect2ById[id] = (s2, eff2Val);
            }

            // 표시정보(ID 기반) 등록: 아이콘은 시트/슬라이스 규칙으로 자동 로드
            ItemPresentationDB.Register(id, itemNameStr, description, iconPath);

#if ICON_TEST
            var icon = ItemPresentationDB.Get(id)?.icon;
            Debug.Log($"[TSV->Icon] id={id}, path='{iconPath}', sprite={(icon != null ? icon.name : "NULL")}");
#endif

            // ItemName enum 매핑(없으면 None 유지)
            ItemName itemName = TryEnum(itemNameStr, out ItemName parsedName) ? parsedName : ItemName.None;

            // 타입 분기
            var itemType = MapItemType(itemTypeStr);
            switch (itemType)
            {
                case ItemType.Equipment:
                    {
                        var part = MapEquipmentPart(detailStr);
                        var stat1 = MapStatus(eff1Str);

                        // (id, itemName, weight, EquipmentPart, Status, value, maxDurability=100, decay=1)
                        var p = new ItemParameterEquipment(id, itemName, weight, part, stat1, eff1Val);
                        p.type = ItemType.Equipment;
                        p.maxstack = 1;                                  // 장비는 고정 1
                                                                         // weight 는 생성자에서 이미 세팅됨

                        itemStats.Add(p);
                        break;
                    }

                case ItemType.Consumable:
                    {
                        var stat1 = MapStatus(eff1Str);
                        var p = new ItemParameterConsumable(id, itemName, weight, stat1, eff1Val);
                        p.type = ItemType.Consumable;
                        p.maxstack = Mathf.Max(1, maxstackTSV);          // TSV 스택 적용
                                                                         // weight 는 생성자에서 이미 세팅됨

                        itemStats.Add(p);
                        break;
                    }

                case ItemType.Water:
                    {
                        // (id, itemName, weight, value, quality)  ※ Status는 내부에서 Thirst 고정
                        var p = new ItemParameterWater(id, itemName, weight, eff1Val, quality);
                        p.type = ItemType.Water;
                        p.maxstack = Mathf.Max(1, maxstackTSV);
                        // weight 는 생성자에서 이미 세팅됨

                        itemStats.Add(p);
                        break;
                    }

                default:
                    {
                        // 기타(etc) → 목록 유지용(로직 없음)
                        var p = new ItemParameter(id, itemName, weight);
                        p.type = ItemType.None;
                        p.maxstack = Mathf.Max(1, maxstackTSV);

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

    // ───────────────────────── helpers ─────────────────────────

    static string Safe(string[] arr, int idx) => (idx < arr.Length) ? arr[idx].Trim() : "";

    static int ParseInt(string s, int fallback = 0)
        => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : fallback;

    static float ParseFloat(string s, float fallback = 0f)
        => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : fallback;

    static bool TryEnum<T>(string s, out T v) where T : struct
    {
        v = default;
        return !string.IsNullOrEmpty(s) && System.Enum.TryParse<T>(s, true, out v);
    }

    static void Warn(int rowIndex, string field)
        => Debug.LogWarning($"[ItemParameterList] line {rowIndex + 1}: invalid {field}");

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

    // detail_type → EquipmentPart 매핑(시트 용어에 맞춤)
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

    // effect_type → Status 매핑 (네 enum 이름에 맞춤)
    static Status MapStatus(string s)
    {
        if (TryEnum(s, out Status parsed)) return parsed;

        switch ((s ?? "").Trim().ToLowerInvariant())
        {
            case "hp": return Status.HP;
            case "thirst": return Status.Thirst;
            case "symptom":
            case "symptoms": return Status.Symptom;
            case "def": return Status.Def;     // 방어력
            case "spd": return Status.Speed;   // 이동속도
            case "wgh": return Status.WGH;     // 최대 하중(가방 등)
            case "slot": return Status.Slot;    // 인벤토리 칸 수
            default: return default;
        }
    }
}
