using UnityEngine;
using TMPro;

public class PlayerStatsPanelUI : MonoBehaviour
{
    [Header("Text refs")]
    [SerializeField] TMP_Text hpMaxText;   // "HP 120"
    [SerializeField] TMP_Text defText;     // "DEF 15"
    [SerializeField] TMP_Text speedText;   // "SPD 4.2"
    [SerializeField] TMP_Text slotText;    // "슬롯 5 + 3"

    [Header("Sources")]
    [SerializeField] PlayerStatus player;  // ⚠️ 일반 C# 클래스라 인스펙터로 직접 주입 필요
    [SerializeField] int baseSlotCount = 5;

    InventoryController _ctrl;
    Inventory _inv;
    EquipmentModel _eq;

    void OnEnable()
    {
        _ctrl = InventoryController.Instance;

        // 인스펙터에서 비워졌다면, 컨트롤러에서 가져오는 폴백(있을 때만)
        if (player == null && _ctrl != null)
        {
            // 필드나 프로퍼티 이름이 playerStatus / PlayerStatus 중 하나라고 가정
            var t = _ctrl.GetType();
            var f = t.GetField("playerStatus") ?? t.GetField("PlayerStatus");
            var p = t.GetProperty("playerStatus") ?? t.GetProperty("PlayerStatus");
            object val = null;
            if (f != null) val = f.GetValue(_ctrl);
            else if (p != null) val = p.GetValue(_ctrl, null);
            player = val as PlayerStatus; // 일반 클래스여도 참조 가능
        }

        if (_ctrl != null)
        {
            _inv = _ctrl.inventory;
            _eq = _ctrl.equipment;

            if (_inv != null) _inv.OnChanged += Refresh;
            if (_eq != null) _eq.OnChanged += Refresh;
        }

        Refresh();
    }

    void OnDisable()
    {
        if (_inv != null) _inv.OnChanged -= Refresh;
        if (_eq != null) _eq.OnChanged -= Refresh;
    }

    public void Refresh()
    {
        // 1) HP Max
        int hpMax = 0;
        if (player != null)
        {
            hpMax = TryGetInt(player, "MaxHP")
                 ?? TryGetInt(player, "hpMax")
                 ?? TryGetInt(player, "healthMax")
                 ?? 0;
        }
        if (hpMaxText) hpMaxText.text = hpMax.ToString();   // ← "HP " 제거하고 숫자만

        // 2) DEF
        int def = 0;
        if (player != null)
            def = TryGetInt(player, "Def") ?? TryGetInt(player, "def") ?? 0;
        if (defText) defText.text = def.ToString();         // 숫자만

        // 3) SPD
        float spd = 0f;
        if (player != null)
        {
            float? s = TryGetFloat(player, "Speed") ?? TryGetFloat(player, "spd");
            spd = s.HasValue ? s.Value : 0f;
        }
        if (speedText) speedText.text = spd.ToString("0.##");  // 숫자만

        // 4) 슬롯: 기본 + 해금
        int active = (_inv != null) ? _inv.ActiveSlotCount : baseSlotCount;
        int baseCount = Mathf.Clamp(baseSlotCount, 0, Inventory.HARD_MAX_SLOTS);
        int unlocked = Mathf.Max(0, active - baseCount);
        if (slotText) slotText.text = $"{baseCount + unlocked}"; // 합산된 숫자만
    }

    // ── 리플렉션 헬퍼 ──
    static int? TryGetInt(object src, string name)
    {
        var t = src.GetType();
        var p = t.GetProperty(name);
        if (p != null && (p.PropertyType == typeof(int) || p.PropertyType == typeof(short) || p.PropertyType == typeof(long)))
            return System.Convert.ToInt32(p.GetValue(src, null));
        var f = t.GetField(name);
        if (f != null && (f.FieldType == typeof(int) || f.FieldType == typeof(short) || f.FieldType == typeof(long)))
            return System.Convert.ToInt32(f.GetValue(src));
        return null;
    }

    static float? TryGetFloat(object src, string name)
    {
        var t = src.GetType();
        var p = t.GetProperty(name);
        if (p != null && (p.PropertyType == typeof(float) || p.PropertyType == typeof(double)))
            return System.Convert.ToSingle(p.GetValue(src, null));
        var f = t.GetField(name);
        if (f != null && (f.FieldType == typeof(float) || f.FieldType == typeof(double)))
            return System.Convert.ToSingle(f.GetValue(src));
        return null;
    }
}
