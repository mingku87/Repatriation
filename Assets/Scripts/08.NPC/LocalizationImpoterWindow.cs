#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LocalizationImporterWindow : EditorWindow
{
    [MenuItem("Tools/Localization/Importer")]
    public static void Open()
    {
        GetWindow<LocalizationImporterWindow>("Localization Importer");
    }

    [Header("Targets")]
    SerializedObject _so;
    LocalizationTable _targetTable;
    SystemLanguage _lang = SystemLanguage.Korean;

    enum SaveMode { Merge, ReplaceAll }
    SaveMode _mode = SaveMode.Merge;

    string _pasteBuffer = "";
    Vector2 _scroll;

    void OnEnable()
    {
        minSize = new Vector2(640, 420);
    }

    void OnGUI()
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Paste Excel (TSV) and save to a LocalizationTable asset.", EditorStyles.boldLabel);

        // 대상 테이블 선택
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            _targetTable = (LocalizationTable)EditorGUILayout.ObjectField("Target Table Asset", _targetTable, typeof(LocalizationTable), false);
            _lang = (SystemLanguage)EditorGUILayout.EnumPopup("Language Tag", _lang);
            _mode = (SaveMode)EditorGUILayout.EnumPopup("Save Mode", _mode);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create New Table Asset...", GUILayout.Height(24)))
                    CreateNewTableAssetDialog();

                if (GUILayout.Button("Paste From Clipboard", GUILayout.Height(24)))
                    _pasteBuffer = GUIUtility.systemCopyBuffer;
            }
        }

        // 붙여넣기 텍스트 영역
        EditorGUILayout.LabelField("Paste Area (Columns separated by TAB, rows by newline)");
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        _pasteBuffer = EditorGUILayout.TextArea(_pasteBuffer, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Parse & Preview", GUILayout.Height(28)))
                PreviewParsed();

            EditorGUI.BeginDisabledGroup(_targetTable == null);
            if (GUILayout.Button("Save to Asset", GUILayout.Height(28)))
                SaveToAsset();
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.Space(6);
    }

    struct Row { public string key; public string val; }

    List<Row> _previewRows;

    void PreviewParsed()
    {
        _previewRows = Parse(_pasteBuffer);
        if (_previewRows == null || _previewRows.Count == 0)
            EditorUtility.DisplayDialog("Parse", "No rows parsed. Check your paste.", "OK");
        else
            ShowPreviewDialog(_previewRows);
    }

    static List<Row> Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return new List<Row>();
        var rows = new List<Row>();

        var lines = raw
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n')
            .Select(s => s.TrimEnd())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        if (lines.Count == 0) return rows;

        // 헤더 감지: 첫 줄에 key가 포함돼 있으면 헤더로 간주
        int start = 0;
        {
            var cols0 = lines[0].Split('\t');
            var headerHit = cols0.Any(c =>
                c.Equals("key", StringComparison.OrdinalIgnoreCase));
            if (headerHit) start = 1;
        }

        for (int i = start; i < lines.Count; i++)
        {
            var cols = lines[i].Split('\t');
            if (cols.Length < 2) continue;

            string key = cols[0].Trim();
            if (string.IsNullOrEmpty(key)) continue;

            // 두 번째 열 이름 추론: value/name/ko 등 어떤 머리글이건 두 번째 칸을 값으로
            string val = cols[1];

            rows.Add(new Row { key = key, val = val });
        }
        return rows;
    }

    void SaveToAsset()
    {
        var rows = Parse(_pasteBuffer);
        if (rows == null || rows.Count == 0)
        {
            EditorUtility.DisplayDialog("Save", "Nothing to save. Paste TSV first.", "OK");
            return;
        }
        if (_targetTable == null)
        {
            EditorUtility.DisplayDialog("Save", "Assign a Target Table Asset.", "OK");
            return;
        }

        Undo.RecordObject(_targetTable, "Localization Import");
        _targetTable.language = _lang;

        if (_mode == SaveMode.ReplaceAll)
        {
            _targetTable.entries.Clear();
            foreach (var r in rows)
                _targetTable.entries.Add(new LocalizationTable.Entry { key = r.key, value = r.val });
        }
        else // Merge
        {
            // 키 인덱스 캐시
            var index = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < _targetTable.entries.Count; i++)
                if (!index.ContainsKey(_targetTable.entries[i].key))
                    index[_targetTable.entries[i].key] = i;

            foreach (var r in rows)
            {
                if (index.TryGetValue(r.key, out var idx))
                {
                    var e = _targetTable.entries[idx];
                    e.value = r.val; // 덮어쓰기
                    _targetTable.entries[idx] = e;
                }
                else
                {
                    _targetTable.entries.Add(new LocalizationTable.Entry { key = r.key, value = r.val });
                    index[r.key] = _targetTable.entries.Count - 1;
                }
            }
        }

        EditorUtility.SetDirty(_targetTable);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Saved", $"Saved {_targetTable.entries.Count} entries to {_targetTable.name}.", "OK");
    }

    void CreateNewTableAssetDialog()
    {
        var path = EditorUtility.SaveFilePanelInProject(
            "Create Localization Table",
            "LocalizationTable_ko",
            "asset",
            "Choose location for the new LocalizationTable asset.");

        if (string.IsNullOrEmpty(path)) return;

        var asset = CreateInstance<LocalizationTable>();
        asset.language = _lang;
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        _targetTable = asset;
        EditorGUIUtility.PingObject(asset);
    }

    static void ShowPreviewDialog(List<Row> rows)
    {
        int show = Mathf.Min(20, rows.Count);
        string msg = $"Parsed {rows.Count} rows.\n\nPreview (first {show}):\n";
        for (int i = 0; i < show; i++)
            msg += $"{rows[i].key}  ->  {rows[i].val}\n";

        EditorUtility.DisplayDialog("Preview", msg, "OK");
    }
}
#endif
