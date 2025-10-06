// File: Assets/Editor/ItemDatabaseViewer.cs
// Item DB grid viewer (Sprite-slice icon support, multi-sheet, Resources & global index)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ItemDatabaseViewer : EditorWindow
{
    [MenuItem("Tools/Item Database Viewer")]
    public static void Open()
    {
        var w = GetWindow<ItemDatabaseViewer>("Item DB (Table)");
        w.minSize = new Vector2(1060, 540);
        w.Show();
    }

    // ===== UI state =====
    Vector2 _scrollV, _scrollH;
    string _search = "";
    int _sortCol = 0;
    bool _sortAsc = true;

    // ===== Columns =====
    class Col
    {
        public string title;
        public float width;
        public Func<Row, string> getter;
        public Action<Rect, Row> drawCell;
    }
    List<Col> _cols;

    // ===== Row projection =====
    class Row
    {
        public object raw;
        public string ID, Name, Type, PartOrDetail, Weight, MaxStack, Buy, Sell, Effects, Durability, SlotBonus, Quality, Desc;
        public string IconPath;          // Resources-relative (no "Assets/Resources/", no extension)
        public Sprite IconSprite;        // ★ use Sprite (not Texture2D)
        public bool IconMissing;
        public string IconDebug;
    }

    // cache: id|path -> sprite
    readonly Dictionary<string, Sprite> _iconCache = new();

#if UNITY_EDITOR
    // ===== Global sprite index (under Assets/Resources) =====
    static Dictionary<string, Sprite> s_NameToSprite; // cleaned slice/file name -> Sprite
    static Dictionary<string, Texture2D> s_NameToTex; // fallback
    static bool s_IndexBuilt;
#endif

    // ===== Styles (lazy to avoid EditorStyles NRE) =====
    GUIStyle _headerStyle, _cellStyle, _cellRight;
    bool _stylesBuilt;
    void EnsureStyles()
    {
        if (_stylesBuilt) return;
        var baseLabel = new GUIStyle(GUI.skin.label);
        var baseButton = new GUIStyle(GUI.skin.button);

        _cellStyle = new GUIStyle(baseLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,
            padding = new RectOffset(6, 6, 1, 1),
            wordWrap = false
        };
        _cellRight = new GUIStyle(_cellStyle) { alignment = TextAnchor.MiddleRight };

        _headerStyle = new GUIStyle(baseButton)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            clipping = TextClipping.Clip,
            padding = new RectOffset(4, 4, 2, 2)
        };
        _stylesBuilt = true;
    }

    void OnEnable()
    {
        _cols = new List<Col>
        {
            new Col{ title="ID",          width=110, getter=r=>r.ID },
            new Col{ title="Icon",        width=48,  getter=r=>"", drawCell=DrawIconCell },
            new Col{ title="Name",        width=220, getter=r=>r.Name },
            new Col{ title="Type",        width=92,  getter=r=>r.Type },
            new Col{ title="Part/Detail", width=120, getter=r=>r.PartOrDetail },
            new Col{ title="Weight",      width=78,  getter=r=>r.Weight },
            new Col{ title="Stack",       width=70,  getter=r=>r.MaxStack },
            new Col{ title="Buy",         width=90,  getter=r=>r.Buy },
            new Col{ title="Sell",        width=90,  getter=r=>r.Sell },
            new Col{ title="Effects",     width=260, getter=r=>r.Effects },
            new Col{ title="Durability",  width=150, getter=r=>r.Durability },
            new Col{ title="Slot+",       width=70,  getter=r=>r.SlotBonus },
            new Col{ title="Quality",     width=90,  getter=r=>r.Quality },
            new Col{ title="Description", width=420, getter=r=>r.Desc },
        };
    }

    void OnGUI()
    {
        EnsureStyles();

        var src = EnsureSourceSingleton();

        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Reload TSV", EditorStyles.toolbarButton, GUILayout.Width(100)))
                SafeCall(src, "LoadFromTSV");
#if UNITY_EDITOR
            if (GUILayout.Button("Rebuild Icon Index", EditorStyles.toolbarButton, GUILayout.Width(140)))
                RebuildIconIndex();
            if (GUILayout.Button("Validate Icons", EditorStyles.toolbarButton, GUILayout.Width(110)))
                ValidateAllIcons(src);
            if (!s_IndexBuilt) GUILayout.Label("Index not built", EditorStyles.miniLabel, GUILayout.Width(110));
#endif
            GUILayout.Space(8);
            GUILayout.Label("Search", GUILayout.Width(48));
            _search = GUILayout.TextField(_search ?? "", EditorStyles.toolbarSearchField, GUILayout.MinWidth(260));
            GUILayout.FlexibleSpace();
            if (_cols.Count > 0)
                GUILayout.Label($"Sort: {_cols[_sortCol].title} {(_sortAsc ? "▲" : "▼")}", EditorStyles.miniLabel);
        }

        if (src == null)
        {
            EditorGUILayout.HelpBox("씬에 ItemParameterList(또는 동등 역할 컴포넌트)를 배치하세요. [ExecuteAlways] 권장.", MessageType.Info);
            return;
        }

        // item list 가져오기
        var itemsObj = GetMember(src, "ItemStats") ??
                       GetMember(src, "Items") ??
                       GetMember(src, "items") ??
                       GetMember(src, "itemList");
        var list = (itemsObj as IEnumerable)?.Cast<object>().ToList();
        if (list == null)
        {
            EditorGUILayout.HelpBox("아이템 리스트를 찾지 못했습니다. (ItemStats/Items/items)", MessageType.Warning);
            return;
        }

#if UNITY_EDITOR
        if (!s_IndexBuilt) RebuildIconIndex();
#endif

        var rows = Adapt(list);

        // 검색 필터
        if (!string.IsNullOrEmpty(_search))
        {
            string s = _search.ToLowerInvariant();
            rows = rows.Where(r =>
                   (r.ID?.ToLowerInvariant().Contains(s) ?? false)
                || (r.Name?.ToLowerInvariant().Contains(s) ?? false)
                || (r.Type?.ToLowerInvariant().Contains(s) ?? false)
                || (r.PartOrDetail?.ToLowerInvariant().Contains(s) ?? false)
                || (r.Effects?.ToLowerInvariant().Contains(s) ?? false)
                || (r.Desc?.ToLowerInvariant().Contains(s) ?? false)
            ).ToList();
        }

        rows = Sort(rows);

        DrawTable(rows, list.Count);
    }

    // ===== Table draw =====
    void DrawTable(List<Row> rows, int totalCount)
    {
        float rowH = EditorGUIUtility.singleLineHeight + 8;
        float gap = 1f;
        float totalW = _cols.Sum(c => c.width) + (_cols.Count - 1) * gap + 12;

        Rect headerRect = GUILayoutUtility.GetRect(0, 10000, rowH, rowH);
        EditorGUI.DrawRect(headerRect, new Color(0.18f, 0.18f, 0.18f, 1f));
        float x = 6;
        for (int i = 0; i < _cols.Count; i++)
        {
            var c = _cols[i];
            var r = new Rect(x, headerRect.y, c.width, rowH);
            EditorGUI.DrawRect(new Rect(r.xMax, r.y + 2, 1, r.height - 4), new Color(0, 0, 0, 0.25f));
            string title = c.title + (_sortCol == i ? (_sortAsc ? " ▲" : " ▼") : "");
            if (GUI.Button(r, title, _headerStyle ?? GUI.skin.button))
            {
                if (_sortCol == i) _sortAsc = !_sortAsc; else { _sortCol = i; _sortAsc = true; }
            }
            x += c.width + gap;
        }

        var bodyRect = GUILayoutUtility.GetRect(0, 10000, position.height - headerRect.height - 48, position.height - headerRect.height - 48);
        var bodyView = new Rect(bodyRect.x, bodyRect.y, bodyRect.width, bodyRect.height);
        var bodyContent = new Rect(0, 0, totalW, rows.Count * rowH);

        _scrollV = GUI.BeginScrollView(bodyView, _scrollV, bodyContent, true, true);
        var old = GUI.matrix; GUI.matrix = Matrix4x4.Translate(new Vector3(-_scrollH.x, 0, 0)) * GUI.matrix;

        for (int i = 0; i < rows.Count; i++)
        {
            Rect rr = new Rect(0, i * rowH, totalW, rowH);
            if ((i & 1) == 0) EditorGUI.DrawRect(rr, new Color(1, 1, 1, 0.025f));
            float cx = 6;
            for (int j = 0; j < _cols.Count; j++)
            {
                var c = _cols[j];
                var cr = new Rect(cx, rr.y, c.width, rowH);
                EditorGUI.DrawRect(new Rect(cr.xMax, cr.y + 2, 1, cr.height - 4), new Color(0, 0, 0, 0.12f));

                if (c.drawCell != null)
                {
                    var ir = new Rect(cr.x + 6, cr.y + 2, Mathf.Min(40, cr.width - 12), cr.height - 4);
                    c.drawCell(ir, rows[i]);
                }
                else
                {
                    var val = c.getter(rows[i]) ?? "";
                    var style = (j == 5 || j == 6 || j == 7 || j == 8) ? (_cellRight ?? GUI.skin.label) : (_cellStyle ?? GUI.skin.label);
                    GUI.Label(new Rect(cr.x + 2, cr.y, cr.width - 4, cr.height), new GUIContent(val, val), style);
                }
                cx += c.width + gap;
            }
        }

        GUI.matrix = old; GUI.EndScrollView();

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField($"{rows.Count} / {totalCount} items", EditorStyles.miniLabel);
    }

    // ===== Icon cell: draw Sprite slice =====
    void DrawIconCell(Rect r, Row row)
    {
        var sp = row.IconSprite;
        if (sp != null)
        {
            var tex = sp.texture;
            var tr = sp.textureRect;
            var uv = new Rect(tr.x / tex.width, tr.y / tex.height, tr.width / tex.width, tr.height / tex.height);
            GUI.DrawTextureWithTexCoords(r, tex, uv, true);
            return;
        }

        // placeholder + tooltip when missing
        var prev = GUI.color; GUI.color = new Color(1, 1, 1, 0.08f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = prev;
        var dot = new Rect(r.xMax - 8, r.y + 2, 6, 6);
        EditorGUI.DrawRect(dot, new Color(1f, 0.6f, 0.1f, 0.9f));
        GUI.Label(r, new GUIContent("", row.IconDebug ?? $"No icon for ID {row.ID}")); // tooltip
    }

    // ===== Data adapt =====
    List<Row> Adapt(List<object> list)
    {
        var rows = new List<Row>(list.Count);
        foreach (var p in list)
        {
            var r = new Row { raw = p };

            r.ID = CleanName(SafeStr(GetMember(p, "id")));

            r.Name = FirstNonEmpty(
                SafeStr(GetMember(p, "displayNameKr")),
                SafeStr(GetMember(p, "displayNameKR")),
                SafeStr(GetMember(p, "nameKr")),
                SafeStr(GetMember(p, "itemNameKr")),
                SafeStr(GetMember(p, "localizedName")),
                SafeStr(GetMember(p, "koName")),
                SafeStr(GetMember(p, "itemName"))
            );

            r.Type = SafeStr(GetMember(p, "type"));
            r.Weight = SafeNum(GetMember(p, "weight")) ?? "";
            r.MaxStack = SafeInt(GetMember(p, "maxstack"));
            r.Buy = SafeInt(GetMember(p, "buyPrice"));
            r.Sell = SafeInt(GetMember(p, "sellPrice"));
            r.Desc = SafeStr(GetMember(p, "description"))?.Replace("\n", " ");
            r.PartOrDetail = FirstNonEmpty(SafeStr(GetMember(p, "part")), SafeStr(GetMember(p, "equipPart")), "-");

            var tname = p.GetType().Name;
            if (tname.IndexOf("Equipment", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var maxDur = SafeInt(GetMember(p, "maxDurability"));
                var decay = SafeNum(GetMember(p, "durabilityDecayRate")) ?? SafeNum(GetMember(p, "decay"));
                r.Durability = string.IsNullOrEmpty(maxDur) ? "" : (decay != null ? $"{maxDur} (decay {decay})" : maxDur);

                var slotBonus = GetMember(p, "slotBonus");
                r.SlotBonus = (slotBonus is int iv && iv > 0) ? $"+{iv}" : "";

                var status = GetMember(p, "status");
                var value = GetMember(p, "value");
                string main = (status != null) ? $"{status} {TrimFloat(value)}" : "";

                var extra = GetMember(p, "extraEffect");
                string ex = "";
                if (extra != null)
                {
                    var exStat = GetMember(extra, "Item1") ?? GetMember(extra, "status");
                    var exVal = GetMember(extra, "Item2") ?? GetMember(extra, "value");
                    if (exStat != null) ex = $"{exStat} {TrimFloat(exVal)}";
                }
                r.Effects = string.IsNullOrEmpty(ex) ? main : $"{main} | {ex}";
            }
            else if (tname.IndexOf("Consumable", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var effArr = GetMember(p, "effects") as Array;
                if (effArr != null && effArr.Length > 0)
                {
                    var listStr = new List<string>(effArr.Length);
                    foreach (var e in effArr)
                    {
                        var s = GetMember(e, "Item1") ?? GetMember(e, "status");
                        var v = GetMember(e, "Item2") ?? GetMember(e, "value");
                        listStr.Add($"{s} {TrimFloat(v)}");
                    }
                    r.Effects = string.Join(" | ", listStr);
                }
                var q = GetMember(p, "quality");
                if (q != null) r.Quality = q.ToString();
            }

            r.IconPath = NormalizeResourcesPath(
                FirstNonEmpty(SafeStr(GetMember(p, "iconPath")),
                              SafeStr(GetMember(p, "icon_path")),
                              SafeStr(GetMember(p, "icon")))
            );

            (r.IconSprite, r.IconMissing, r.IconDebug) = ResolveIconSprite(r.ID, r.IconPath);

            rows.Add(r);
        }
        return rows;
    }

    // ===== Sorting =====
    List<Row> Sort(List<Row> rows)
    {
        Func<Row, string> key = _cols[_sortCol].getter;
        bool numeric = new[] { "Weight", "Stack", "Buy", "Sell", "Slot+" }.Contains(_cols[_sortCol].title);
        IOrderedEnumerable<Row> ordered;
        if (numeric)
        {
            float PFloat(string s) { float.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var f); return f; }
            int PInt(string s) { int.TryParse(s, out var n); return n; }
            string title = _cols[_sortCol].title;
            ordered = title == "Weight" ? rows.OrderBy(r => PFloat(key(r) ?? "0")) : rows.OrderBy(r => PInt(key(r) ?? "0"));
        }
        else ordered = rows.OrderBy(r => key(r) ?? "", StringComparer.OrdinalIgnoreCase);
        return _sortAsc ? ordered.ToList() : ordered.Reverse().ToList();
    }

    // ===== Icon resolution (Sprite) =====
    (Sprite sprite, bool missing, string debug) ResolveIconSprite(string idRaw, string iconPathRaw)
    {
        string id = CleanName(idRaw);
        string iconPath = CleanPath(iconPathRaw);

        string key = (id ?? "") + "|" + (iconPath ?? "");
        if (_iconCache.TryGetValue(key, out var cached))
            return (cached, cached == null, cached == null ? $"Icon not found for id={id}, path={iconPath}" : null);

        Sprite sp = null;
        string why = null;

        // 1) iconPath 우선 (Resources)
        if (!string.IsNullOrEmpty(iconPath))
        {
            sp = Resources.Load<Sprite>(iconPath);
            if (sp == null)
            {
                // 시트 이름이 iconPath일 때: 그 시트 안에서 id로 매칭
                var arr = Resources.LoadAll<Sprite>(iconPath);
                if (arr != null && arr.Length > 0) sp = FindBestSlice(arr, id, GetLast(iconPath));

                // 디렉토리 단위 보조 탐색
                if (sp == null)
                {
                    var arr2 = Resources.LoadAll<Sprite>(GetDir(iconPath));
                    if (arr2 != null && arr2.Length > 0) sp = FindBestSlice(arr2, id, GetLast(iconPath));
                }
            }
        }

#if UNITY_EDITOR
        // 2) 글로벌 인덱스 (Assets/Resources 전체)로 최종 매칭
        if (sp == null)
        {
            if (!s_IndexBuilt) RebuildIconIndex();
            if (s_NameToSprite != null && s_NameToSprite.TryGetValue(CleanName(id), out var s)) sp = s;
            else if (!string.IsNullOrEmpty(iconPath) &&
                     s_NameToSprite.TryGetValue(CleanName(GetLast(iconPath)), out var s2)) sp = s2;

            // 마지막 수단: 텍스처만 있으면 전체를 임시 스프라이트로 생성
            if (sp == null && s_NameToTex != null)
            {
                if (s_NameToTex.TryGetValue(CleanName(id), out var t0) && t0 != null)
                    sp = Sprite.Create(t0, new Rect(0, 0, t0.width, t0.height), new Vector2(0.5f, 0.5f), 100);
                else if (!string.IsNullOrEmpty(iconPath) &&
                         s_NameToTex.TryGetValue(CleanName(GetLast(iconPath)), out var t1) && t1 != null)
                    sp = Sprite.Create(t1, new Rect(0, 0, t1.width, t1.height), new Vector2(0.5f, 0.5f), 100);
            }
        }
#endif

        if (sp == null) why = $"No sprite match. id={id}, iconPath={iconPath}";
        _iconCache[key] = sp;
        return (sp, sp == null, why);
    }

#if UNITY_EDITOR
    // Build name -> sprite/texture index from Assets/Resources (includes slices)
    static void RebuildIconIndex()
    {
        s_NameToSprite = new Dictionary<string, Sprite>(2048, StringComparer.OrdinalIgnoreCase);
        s_NameToTex = new Dictionary<string, Texture2D>(2048, StringComparer.OrdinalIgnoreCase);

        string[] guidsSprites = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Resources" });
        foreach (var g in guidsSprites)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var objs = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var o in objs)
            {
                if (o is Sprite sp)
                {
                    string key = CleanName(sp.name);
                    if (!s_NameToSprite.ContainsKey(key)) s_NameToSprite[key] = sp;
                }
            }
        }

        string[] guidsTex = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources" });
        foreach (var g in guidsTex)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                string key = CleanName(System.IO.Path.GetFileNameWithoutExtension(path));
                if (!s_NameToTex.ContainsKey(key)) s_NameToTex[key] = tex;
            }
        }

        s_IndexBuilt = true;
        Debug.Log($"[Icon Index] Built: sprites={s_NameToSprite.Count}, textures={s_NameToTex.Count}");
    }
#endif

    // ===== Helpers =====
    static Sprite FindBestSlice(Sprite[] slices, string id, string hint)
    {
        if (slices == null || slices.Length == 0) return null;
        string idC = CleanName(id);
        string hintC = CleanName(hint);

        var hit = slices.FirstOrDefault(a => CleanName(a.name) == idC);
        if (hit != null) return hit;
        hit = slices.FirstOrDefault(a => CleanName(a.name) == hintC);
        if (hit != null) return hit;
        hit = slices.FirstOrDefault(a => CleanName(a.name).EndsWith(idC, StringComparison.OrdinalIgnoreCase));
        if (hit != null) return hit;
        hit = slices.FirstOrDefault(a => idC.EndsWith(CleanName(a.name), StringComparison.OrdinalIgnoreCase));
        return hit;
    }

    static string NormalizeResourcesPath(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return null;
        string p = raw.Replace("\\", "/");
        if (p.StartsWith("Assets/Resources/", StringComparison.OrdinalIgnoreCase))
            p = p.Substring("Assets/Resources/".Length);
        if (p.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase))
            p = p.Substring("Resources/".Length);
        if (p.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            p.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
            p = p.Substring(0, p.LastIndexOf('.'));
        return p;
    }
    static string CleanPath(string raw) => NormalizeResourcesPath(raw);
    static string GetDir(string path)
    {
        if (string.IsNullOrEmpty(path)) return "";
        int i = path.LastIndexOf('/');
        return i >= 0 ? path.Substring(0, i) : "";
    }
    static string GetLast(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        int i = path.LastIndexOf('/');
        return i >= 0 ? path.Substring(i + 1) : path;
    }
    static string CleanName(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        s = s.Trim();
        s = s.Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", "").Replace("\uFEFF", "");
        return s;
    }

    object EnsureSourceSingleton()
    {
        var t = FindTypeByName("ItemParameterList");
        if (t == null) return null;
        var pi = t.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var inst = pi?.GetValue(null);
        if (inst == null)
        {
            var all = Resources.FindObjectsOfTypeAll(t);
            if (all.Length > 0) { inst = all[0]; pi?.SetValue(null, inst); }
        }
        return inst;
    }

    static Type FindTypeByName(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try { var t = asm.GetTypes().FirstOrDefault(x => x.Name == typeName); if (t != null) return t; }
            catch { }
        }
        return null;
    }
    static object GetMember(object obj, string name)
    {
        if (obj == null) return null;
        var t = obj.GetType();
        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null) return p.GetValue(obj);
        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null) return f.GetValue(obj);
        return null;
    }
    static void SafeCall(object obj, string method)
    {
        if (obj == null) return;
        var mi = obj.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        mi?.Invoke(obj, null);
    }

    static string FirstNonEmpty(params string[] arr) => arr.FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? "";

    static string TrimFloat(object v)
    {
        if (v == null) return "0";
        if (v is float f) return Mathf.Approximately(f, Mathf.Round(f)) ? Mathf.RoundToInt(f).ToString() : f.ToString("0.###");
        if (v is double d) return Math.Abs(d - Math.Round(d)) < 1e-6 ? ((int)Math.Round(d)).ToString() : d.ToString("0.###");
        if (float.TryParse(v.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var pf))
            return Mathf.Approximately(pf, Mathf.Round(pf)) ? Mathf.RoundToInt(pf).ToString() : pf.ToString("0.###");
        return v.ToString();
    }
    static string SafeStr(object v) => v?.ToString() ?? "";
    static string SafeInt(object v)
    {
        if (v == null) return "";
        if (v is int i) return i.ToString();
        if (int.TryParse(v.ToString(), out var n)) return n.ToString();
        return v.ToString();
    }
    static string SafeNum(object v)
    {
        if (v == null) return null;
        if (v is float f) return f.ToString("0.###");
        if (v is double d) return d.ToString("0.###");
        if (float.TryParse(v.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var pf))
            return pf.ToString("0.###");
        return v.ToString();
    }

    // ===== Validators =====
    void ValidateAllIcons(object src)
    {
        var itemsObj = GetMember(src, "ItemStats") ?? GetMember(src, "Items") ?? GetMember(src, "items") ?? GetMember(src, "itemList");
        var list = (itemsObj as IEnumerable)?.Cast<object>().ToList();
        if (list == null) { Debug.LogWarning("Validate Icons: no item list"); return; }

        int miss = 0, total = 0;
        foreach (var p in list)
        {
            total++;
            string id = CleanName(SafeStr(GetMember(p, "id")));
            string iconPath = NormalizeResourcesPath(
                FirstNonEmpty(SafeStr(GetMember(p, "iconPath")),
                              SafeStr(GetMember(p, "icon_path")),
                              SafeStr(GetMember(p, "icon")))
            );
            var (sp, missing, why) = ResolveIconSprite(id, iconPath);
            if (missing) { miss++; Debug.LogWarning($"[Icon Missing] id={id}, iconPath={iconPath} :: {why}"); }
        }
        Debug.Log($"Validate Icons: {total - miss}/{total} matched.");
    }
}
