using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// TSV → 아이템 파라미터 로더
/// - effect_type1/2 모두 처리
/// - Water는 quality 읽음
/// - Bag(slot/slotcapacity)은 인벤토리 슬롯 보너스로 누적
/// - 표시 정보는 ItemPresentationDB.Register 로 등록
/// - 프로젝트 enum 이름(Arms/Hands/Knees/Feet)과 TSV 약어(elbow/glove/knee/shoes/weapon) 자동 매핑
/// - ItemType에 Etc가 없어도 동작하도록 “기타”를 enum 의존 없이 처리
/// </summary>
public class ItemParameterList : MonoBehaviour
{
    public static ItemParameterList Instance { get; private set; }

    [Header("TSV (StreamingAssets/)")]
    [SerializeField] string fileName = "ItemData.tsv";

    /// <summary>모든 아이템 파라미터</summary>
    public readonly List<ItemParameter> itemStats = new List<ItemParameter>();

    /// <summary>ID→파라미터 캐시</summary>
    private readonly Dictionary<int, ItemParameter> _byId = new Dictionary<int, ItemParameter>();

    public IReadOnlyList<ItemParameter> ItemStats => itemStats;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }
    }

    // ─────────────────────────────────────────────────────────────────────────────

    public void LoadFromTSV()
    {
        itemStats.Clear();
        _byId.Clear();

        var path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"[ItemParameterList] TSV not found: {path}");
            return;
        }

        var lines = File.ReadAllLines(path);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("[ItemParameterList] TSV is empty.");
            return;
        }

        var headers = ParseTSVLine(lines[0]);
        var col = BuildColumnIndex(headers);

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cells = ParseTSVLine(line);
            var row = RowDict(col, cells);

            int id = ParseInt(Get(row, "item_id"));
            string name = Get(row, "item_name");
            string typeStr = Get(row, "item_type");
            string detailStr = Get(row, "detail_type");

            float weight = ParseFloat(Get(row, "weight"), 0f);
            int maxstack = ParseInt(Get(row, "maxstack"), 1);
            int buy = ParseInt(Get(row, "buy_price"), 0);
            int sell = ParseInt(Get(row, "sell_price"), 0);
            string desc = Get(row, "description");
            string iconPath = Get(row, "icon_path");

            // 표시 DB 등록 (아이콘/툴팁 표시에 사용)
            ItemPresentationDB.Register(id, name, desc, iconPath);

            // 타입 판정 (Etc가 없어도 동작)
            var mappedType = TryMapItemType(typeStr);
            bool isEquip = IsEquipmentType(typeStr, mappedType);
            bool isCons = IsConsumableType(typeStr, mappedType);

            if (isEquip)
            {
                // 효과 2개 읽기
                string eff1Str = Get(row, "effect_type1");
                string eff2Str = Get(row, "effect_type2");
                float eff1Val = ParseFloat(Get(row, "effect_value1"));
                float eff2Val = ParseFloat(Get(row, "effect_value2"));

                int maxDur = ParseInt(Get(row, "maxdurability"), 0);
                int decay = ParseInt(Get(row, "durabilitydecayrate"), 0);

                // 장비 파트 매핑(프로젝트 enum에 맞춤: Arms/Hands/Knees/Feet 등)
                var part = MapEquipmentPart(detailStr);

                // 주효과
                Status stat1;
                bool has1 = TryParseStatus(eff1Str, out stat1);

                // 보조효과
                Status stat2;
                bool has2 = TryParseStatus(eff2Str, out stat2);

                var p = new ItemParameterEquipment(
                    id: id,
                    name: default,            // ItemName(string) 생성자 의존 없음
                    weight: weight,
                    part: part,
                    status: has1 ? stat1 : default,
                    value: has1 ? eff1Val : 0f,
                    maxDurability: maxDur,
                    decay: decay
                );

                p.displayNameKr = name;
                p.type = mappedType; // enum 있으면 들어감(없으면 default)
                p.maxstack = 1;
                p.buyPrice = buy;
                p.sellPrice = sell;
                p.description = desc;

                // 슬롯 보너스 누적 (slot / slotcapacity 같은 별칭도 허용)
                if (has1 && IsSlotLike(stat1)) p.slotBonus += Mathf.RoundToInt(eff1Val);
                if (has2 && IsSlotLike(stat2)) p.slotBonus += Mathf.RoundToInt(eff2Val);

                // 보조효과 저장(슬롯 보너스/빈 문자열 제외)
                if (has2 && !IsSlotLike(stat2))
                    p.extraEffect = (stat2, eff2Val);

                itemStats.Add(p);
                _byId[id] = p;
            }
            else if (isCons)
            {
                var effects = new List<(Status, float)>();

                string e1 = Get(row, "effect_type1");
                string e2 = Get(row, "effect_type2");
                float v1 = ParseFloat(Get(row, "effect_value1"));
                float v2 = ParseFloat(Get(row, "effect_value2"));

                Status s1; if (TryParseStatus(e1, out s1)) effects.Add((s1, v1));
                Status s2; if (TryParseStatus(e2, out s2)) effects.Add((s2, v2));

                var detail = MapConsumableDetail(detailStr);

                ItemParameter p;
                if (detail == ConsumableDetail.Water)
                {
                    int quality = ParseInt(Get(row, "quality"), 0);
                    p = new ItemParameterWater(id, default, weight, effects.ToArray(), quality);
                }
                else
                {
                    p = new ItemParameterConsumable(id, default, weight, detail, effects.ToArray());
                }

                p.displayNameKr = name;
                p.type = mappedType; // enum에 Consumable이 있으면 들어감
                p.maxstack = maxstack;
                p.buyPrice = buy;
                p.sellPrice = sell;
                p.description = desc;

                itemStats.Add(p);
                _byId[id] = p;
            }
            else
            {
                // 기타(Etc가 없어도 여기로 처리)
                var p = new ItemParameter(id, default, weight)
                {
                    displayNameKr = name,
                    type = mappedType,   // ItemType에 Etc가 없어도 안전
                    maxstack = maxstack,
                    buyPrice = buy,
                    sellPrice = sell,
                    description = desc
                };
                itemStats.Add(p);
                _byId[id] = p;
            }
        }

        Debug.Log($"[ItemParameterList] Loaded {itemStats.Count} items");
    }

    public static ItemParameter GetItemStat(int id)
    {
        if (Instance != null && Instance._byId.TryGetValue(id, out var p)) return p;
        return null;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 파싱/매핑 헬퍼
    // ─────────────────────────────────────────────────────────────────────────────

    static string[] ParseTSVLine(string line) => line.Split('\t');

    static Dictionary<string, int> BuildColumnIndex(string[] headers)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
        {
            var key = headers[i].Trim();
            if (!map.ContainsKey(key)) map.Add(key, i);
        }
        return map;
    }

    static Dictionary<string, string> RowDict(Dictionary<string, int> col, string[] cells)
    {
        var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in col)
        {
            int idx = kv.Value;
            d[kv.Key] = (idx >= 0 && idx < cells.Length) ? cells[idx] : string.Empty;
        }
        return d;
    }

    static string Get(Dictionary<string, string> row, string key)
    {
        string v; return row.TryGetValue(key, out v) ? v : string.Empty;
    }

    static int ParseInt(string s, int def = 0)
    {
        int n; return int.TryParse(s, out n) ? n : def;
    }

    static float ParseFloat(string s, float def = 0f)
    {
        float f; return float.TryParse(s, out f) ? f : def;
    }

    // ── ItemType: Etc가 없어도 동작 ─────────────────────────────────────

    static ItemType TryMapItemType(string s)
    {
        // 1) enum 이름 그대로 온 경우 (Equipment / Consumable 등)
        if (EnumTryParse<ItemType>(ToPascalCase(s), out var parsed))
            return parsed;

        // 2) 약어/별칭 대응
        var k = (s ?? "").Trim().ToLowerInvariant();
        if (k == "equip" || k == "equipment")
        {
            if (EnumTryParse<ItemType>("Equipment", out var e)) return e;
        }
        if (k == "use" || k == "consumable")
        {
            if (EnumTryParse<ItemType>("Consumable", out var c)) return c;
        }

        if (k == "etc" || k == "other")
        {
            if (EnumTryParse<ItemType>("None", out var n)) return n;
        }

        // 3) 그 외(Etc가 없어도 default)
        return default;
    }

    static bool IsEquipmentType(string raw, ItemType mapped)
    {
        var k = (raw ?? "").Trim().ToLowerInvariant();
        return mapped.ToString().Equals("Equipment", StringComparison.OrdinalIgnoreCase)
            || k == "equip" || k == "equipment";
    }

    static bool IsConsumableType(string raw, ItemType mapped)
    {
        var k = (raw ?? "").Trim().ToLowerInvariant();
        return mapped.ToString().Equals("Consumable", StringComparison.OrdinalIgnoreCase)
            || k == "use" || k == "consumable";
    }

    // ── EquipmentPart: 프로젝트 enum에 맞춰 교정 ─────────────────────────

    static EquipmentPart MapEquipmentPart(string s)
    {
        // TSV 약어/단어 → 프로젝트 실제 enum 이름(Head/Body/Arms/Hands/Bag/Knees/Feet …)
        s = (s ?? "").Trim().ToLowerInvariant();

        if (s == "hat") return ParsePartOrFallback("Head");
        if (s == "body") return ParsePartOrFallback("Body");

        // elbow → Arms
        if (s == "elbow" || s == "arms" || s == "arm")
            return ParsePartOrFallback("Arms");

        // glove → Hands
        if (s == "glove" || s == "hand" || s == "hands")
            return ParsePartOrFallback("Hands");

        // knee → Knees
        if (s == "knee" || s == "knees")
            return ParsePartOrFallback("Knees");

        // shoes → Feet
        if (s == "shoes" || s == "feet" || s == "foot")
            return ParsePartOrFallback("Feet");

        if (s == "bag") return ParsePartOrFallback("Bag");

        // weapon → Hands (무기 손에 든다고 가정)
        if (s == "weapon" || s == "wep")
            return ParsePartOrFallback("Hands");

        // 기본 Body
        return ParsePartOrFallback("Body");
    }

    static EquipmentPart ParsePartOrFallback(string name)
    {
        if (EnumTryParse<EquipmentPart>(name, out var value))
            return value;

        // 마지막 안전망: Body
        if (EnumTryParse<EquipmentPart>("Body", out var body))
            return body;

        return default;
    }

    // ── Consumable detail ────────────────────────────────────────────

    static ConsumableDetail MapConsumableDetail(string s)
    {
        s = (s ?? "").Trim().ToLowerInvariant();
        if (s == "food") return ConsumableDetail.Food;
        if (s == "water") return ConsumableDetail.Water;
        if (s == "drug") return ConsumableDetail.Drug;
        if (s == "medicine") return ConsumableDetail.Drug;
        if (s == "other") return ConsumableDetail.Food;
        return ConsumableDetail.Food;
    }

    /// <summary>TSV 문자열을 프로젝트의 Status enum으로 유연 매핑</summary>
    static bool TryParseStatus(string raw, out Status status)
    {
        status = default;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        string s = raw.Trim().ToLowerInvariant();

        // 1) 직접 Enum.TryParse (Speed/CarryCapacity/Symptom/SlotCapacity 등 실제 이름이 TSV에 온 경우)
        if (EnumTryParse<Status>(ToPascalCase(s), out status)) return true;

        // 2) 약어/별칭 매핑
        if (s == "atk" && EnumTryParse<Status>("Atk", out status)) return true;
        if (s == "def" && EnumTryParse<Status>("Def", out status)) return true;
        if (s == "spd" && EnumTryParse<Status>("Speed", out status)) return true;
        if (s == "wgh" && EnumTryParse<Status>("WGH", out status)) return true;
        if (s == "slot" && EnumTryParse<Status>("Slot", out status)) return true;

        if (s == "hp" && EnumTryParse<Status>("HP", out status)) return true;
        if (s == "thirst" && EnumTryParse<Status>("Thirst", out status)) return true;
        if (s == "symptom" && EnumTryParse<Status>("Symptom", out status)) return true;
        if (s == "symptoms" && EnumTryParse<Status>("Symptom", out status)) return true;

        return false;
    }

    /// <summary>해당 Status가 "슬롯 보너스" 성격인지 판단</summary>
    static bool IsSlotLike(Status st)
    {
        return st.ToString().Equals("SlotCapacity", StringComparison.OrdinalIgnoreCase)
            || st.ToString().Equals("Slot", StringComparison.OrdinalIgnoreCase);
    }

    // ── 공용 유틸 ─────────────────────────────────────

    static bool EnumTryParse<T>(string name, out T value) where T : struct
    {
        return Enum.TryParse<T>(name, ignoreCase: true, out value);
    }

    static string ToPascalCase(string lower)
    {
        if (string.IsNullOrEmpty(lower)) return lower;
        if (lower.Length == 1) return lower.ToUpperInvariant();

        var parts = lower.Replace('_', ' ').Split(' ');
        for (int i = 0; i < parts.Length; i++)
        {
            var p = parts[i];
            if (p.Length == 0) continue;
            parts[i] = char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p.Substring(1).ToLowerInvariant() : "");
        }
        return string.Join("", parts);
    }
}
