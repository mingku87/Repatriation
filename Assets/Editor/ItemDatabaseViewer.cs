#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ItemDatabaseViewer : EditorWindow
{
    [MenuItem("Tools/Item Database Viewer")]
    public static void Open() => GetWindow<ItemDatabaseViewer>("Item DB");

    private Vector2 _scroll;
    private string _search = "";

    void OnEnable()
    {
        // 첫 오픈 시 비어 있으면 로드
        if (ItemParameterList.itemStats.Count == 0)
            SafeReload();
    }

    void OnGUI()
    {
        // ── Toolbar ───────────────────────────────────────────────
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Reload TSV", EditorStyles.toolbarButton, GUILayout.Width(100)))
                SafeReload();

            GUILayout.Space(8);
            GUILayout.Label("Search:", GUILayout.Width(50));
            _search = GUILayout.TextField(_search, EditorStyles.toolbarTextField, GUILayout.MinWidth(120));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Count: {ItemParameterList.itemStats.Count}", GUILayout.Width(100));
        }

        // ── Header ────────────────────────────────────────────────
        using (new EditorGUILayout.HorizontalScope())
        {
            Header("ID", 60);
            Header("Type", 90);
            Header("Name", 160);         // ← TSV의 표시 이름 사용
            Header("Icon", 50);          // ← 아이콘 미리보기
            Header("Weight", 70);
            Header("Effect(Status/Value)", 220);
            GUILayout.FlexibleSpace();
        }
        DrawLine();

        // ── Rows ──────────────────────────────────────────────────
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        foreach (var p in ItemParameterList.itemStats)
        {
            if (!PassFilter(p, _search)) continue;

            using (new EditorGUILayout.HorizontalScope())
            {
                // ID / Type
                Cell(p.id.ToString(), 60);
                Cell(p.type.ToString(), 90);

                // Name(표시용): ItemPresentationDB 우선, 없으면 enum 키
                var pres = ItemPresentationDB.Get(p.id);
                string nameToShow = pres?.displayName;
                if (string.IsNullOrEmpty(nameToShow))
                    nameToShow = p.itemName.ToString(); // fallback (None일 수 있음)
                Cell(nameToShow, 160);

                // Icon preview (Sprite 시트의 슬라이스만 잘라서 표시)
                DrawSpritePreview(pres?.icon, 40f);

                // Weight
                Cell(p.weight.ToString("0.##"), 70);

                // Effect
                string effect = "-";
                if (p is ItemParameterEquipment eq)
                    effect = $"{eq.status} / {eq.value}";
                else if (p is ItemParameterConsumable co)
                    effect = $"{co.status} / {co.value}";
                else if (p is ItemParameterWater wa)
                    effect = $"Thirst / {wa.value} (Q:{wa.quality})";

                if (ItemParameterList.effect2ById.TryGetValue(p.id, out var e2))
                    effect += $"  |  {e2.status} / {e2.value}";

                Cell(effect, 220);

                GUILayout.FlexibleSpace();
            }
        }
        EditorGUILayout.EndScrollView();
    }

    // ── Helpers ──────────────────────────────────────────────────
    static void Header(string text, float w)
    {
        var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft };
        GUILayout.Label(text, style, GUILayout.Width(w));
    }

    static void Cell(string text, float w) => GUILayout.Label(text, GUILayout.Width(w));

    static void DrawLine(int thickness = 1)
    {
        var rect = EditorGUILayout.GetControlRect(false, thickness);
        EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.2f));
    }

    static void DrawSpritePreview(Sprite s, float size)
    {
        var rect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
        if (s == null)
        {
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.08f));
            GUI.Label(rect, "—", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        var tex = s.texture;
        var r = s.rect; // 픽셀 단위
        var uv = new Rect(r.x / tex.width, r.y / tex.height, r.width / tex.width, r.height / tex.height);
        GUI.DrawTextureWithTexCoords(rect, tex, uv, alphaBlend: true);
    }

    static bool PassFilter(ItemParameter p, string key)
    {
        if (string.IsNullOrEmpty(key)) return true;
        key = key.ToLowerInvariant();

        if (p.id.ToString().Contains(key)) return true;
        if (p.type.ToString().ToLowerInvariant().Contains(key)) return true;
        if (p.itemName.ToString().ToLowerInvariant().Contains(key)) return true;

        var pres = ItemPresentationDB.Get(p.id);
        if (!string.IsNullOrEmpty(pres?.displayName) && pres.displayName.ToLowerInvariant().Contains(key)) return true;

        if (p is ItemParameterEquipment eq)
            return eq.status.ToString().ToLowerInvariant().Contains(key);
        if (p is ItemParameterConsumable co)
            return co.status.ToString().ToLowerInvariant().Contains(key);
        if (p is ItemParameterWater wa)
            return ("thirst".Contains(key) || wa.quality.ToString().Contains(key));

        return false;
    }

    static void SafeReload()
    {
        try
        {
            ItemParameterList.LoadFromTSV(); // StreamingAssets/Item_data.tsv
            RepaintAll();
            Debug.Log($"[Item DB Viewer] Reloaded: {ItemParameterList.itemStats.Count} items");
        }
        catch (System.SystemException e)
        {
            Debug.LogError($"[Item DB Viewer] Reload failed: {e.Message}");
        }
    }

    static void RepaintAll()
    {
        var windows = Resources.FindObjectsOfTypeAll<ItemDatabaseViewer>();
        foreach (var w in windows) w.Repaint();
    }
}
#endif
