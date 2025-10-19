using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Game.Dialogue; // ← 런타임 타입 참조

public class DialogueDualTsvPasteImporter : EditorWindow
{
    [TextArea(6, 30)] string metaTsv = "";   // Dialogue.tsv
    [TextArea(10, 30)] string textTsv = "";  // Dialogue_Text.tsv
    Vector2 s1, s2;

    const string Folder = "Assets/Resources/Dialogue";
    const string MetaPath = Folder + "/DialogueMetaTable.asset";
    const string LinesPath = Folder + "/DialogueLinesTable.asset";

    [MenuItem("Tools/Import/Paste Dialogue (Meta + Text)...")]
    public static void Open()
    {
        var w = GetWindow<DialogueDualTsvPasteImporter>("Paste Dialogue TSVs");
        w.minSize = new Vector2(760, 520);
        w.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Dialogue.tsv (메타) 붙여넣기", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("헤더: index, npcid, dialoguetype, conditiontype, conditionkey, dialogueid", MessageType.Info);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("클립보드 붙이기", GUILayout.Width(140))) metaTsv = GUIUtility.systemCopyBuffer ?? "";
            if (GUILayout.Button("지우기", GUILayout.Width(90))) metaTsv = "";
            GUILayout.FlexibleSpace();
        }
        s1 = EditorGUILayout.BeginScrollView(s1, GUILayout.Height(160));
        metaTsv = EditorGUILayout.TextArea(metaTsv, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Dialogue_Text.tsv (라인) 붙여넣기", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("헤더: dialogueid, dialogueindex, nextdialogue, speaker, textkey  (preview 무시)\n" +
                                "speaker는 반드시 'player' 또는 'npc:ID' 형식이어야 합니다.", MessageType.Info);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("클립보드 붙이기", GUILayout.Width(140))) textTsv = GUIUtility.systemCopyBuffer ?? "";
            if (GUILayout.Button("지우기", GUILayout.Width(90))) textTsv = "";
            GUILayout.FlexibleSpace();
        }
        s2 = EditorGUILayout.BeginScrollView(s2, GUILayout.Height(220));
        textTsv = EditorGUILayout.TextArea(textTsv, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(12);
        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(metaTsv) && string.IsNullOrWhiteSpace(textTsv)))
        {
            if (GUILayout.Button("Import (붙여넣은 내용 모두)", GUILayout.Height(34)))
            {
                EnsureFolder(Folder);
                if (!string.IsNullOrWhiteSpace(metaTsv)) ImportMeta(metaTsv);
                if (!string.IsNullOrWhiteSpace(textTsv)) ImportLines(textTsv);
                AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("완료", "임포트가 완료되었습니다.", "OK");
            }
        }
    }

    // ── META
    void ImportMeta(string raw)
    {
        var lines = SplitLines(StripBom(raw));
        if (lines.Count == 0) { Err("META 비어 있음"); return; }

        var header = SplitTsv(lines[0]);
        var col = Map(header);
        string[] req = { "index", "npcid", "dialoguetype", "conditiontype", "conditionkey", "dialogueid" };
        foreach (var r in req) if (!col.ContainsKey(r)) { Err($"META 헤더 누락: {r}"); return; }

        var list = new List<DialogueMeta>();
        for (int i = 1; i < lines.Count; i++)
        {
            var row = lines[i]; if (string.IsNullOrWhiteSpace(row) || row.StartsWith("#")) continue;
            var c = SplitTsv(row); if (c.All(string.IsNullOrEmpty)) continue;

            try
            {
                var dm = new DialogueMeta
                {
                    index = ParseInt(Get(c, col, "index")),
                    npcId = Get(c, col, "npcid"),
                    dialogueType = (DialogueType)ParseEnumishInt(Get(c, col, "dialoguetype"), 1),
                    conditionType = (ConditionType)ParseEnumishInt(Get(c, col, "conditiontype"), 0),
                    conditionKey = NormalizeNone(Get(c, col, "conditionkey")),
                    dialogueId = Get(c, col, "dialogueid")
                };
                list.Add(dm);
            }
            catch (Exception ex)
            {
                Debug.LogError($"META {i + 1}행 파싱 실패: {ex.Message}\n{row}");
            }
        }

        var asset = AssetDatabase.LoadAssetAtPath<DialogueMetaTable>(MetaPath);
        if (asset == null) { asset = ScriptableObject.CreateInstance<DialogueMetaTable>(); AssetDatabase.CreateAsset(asset, MetaPath); }
        asset.rows = list;
        EditorUtility.SetDirty(asset);
        Debug.Log($"META 임포트: {list.Count} rows → {MetaPath}");
    }

    // ── LINES
    void ImportLines(string raw)
    {
        var physical = SplitLines(StripBom(raw));
        if (physical.Count == 0) { Err("LINES 비어 있음"); return; }

        // 헤더 찾기
        int headerIdx = physical.FindIndex(s => !string.IsNullOrWhiteSpace(s));
        if (headerIdx < 0) { Err("LINES 헤더 없음"); return; }

        var header = SplitTsv(physical[headerIdx]);
        var col = Map(header);
        string[] req = { "dialogueid", "dialogueindex", "nextdialogue", "speaker", "textkey" };
        foreach (var r in req)
            if (!col.ContainsKey(r)) { Err($"LINES 헤더 누락: {r}"); return; }

        // 셀 내 줄바꿈 보정
        var logicalRows = AssembleLogicalRows(physical, headerIdx + 1, minCols: 5);

        var outLines = new List<Game.Dialogue.DialogueLine>();

        foreach (var row in logicalRows)
        {
            if (string.IsNullOrWhiteSpace(row)) continue;
            var c = SplitTsv(row);
            if (c.All(string.IsNullOrEmpty)) continue;

            try
            {
                // 기존의 speaker 파싱 블록을 이걸로 교체
                var spRaw0 = (Get(c, col, "speaker") ?? "").Trim();
                var spRaw = Unquote(spRaw0); // "NPC001" 같이 따옴표가 있으면 제거
                Game.Dialogue.SpeakerKind who;
                string whoNpc = "";

                // 1) player
                if (string.IsNullOrEmpty(spRaw) || spRaw.Equals("player", StringComparison.OrdinalIgnoreCase))
                {
                    who = Game.Dialogue.SpeakerKind.Player;
                }
                // 2) npc:ID  (대소문자 섞여도 허용)
                else if (spRaw.Length >= 4 && spRaw.Substring(0, 4).Equals("npc:", StringComparison.OrdinalIgnoreCase))
                {
                    var id = spRaw.Substring(spRaw.IndexOf(':') + 1).Trim();
                    if (string.IsNullOrEmpty(id))
                        throw new Exception("speaker가 'npc:' 형식인데 ID가 비어 있습니다. 예) npc:NPC001");
                    who = Game.Dialogue.SpeakerKind.Npc;
                    whoNpc = id;
                }
                // 3) 콜론 없이 ID만 쓴 경우 → NPC로 해석
                else
                {
                    // "npc" 한 단어만 들어온 경우는 실수일 확률이 높으니 에러로 처리
                    if (spRaw.Equals("npc", StringComparison.OrdinalIgnoreCase))
                        throw new Exception("speaker가 'npc'만 입력되었습니다. 'npc:ID' 또는 'ID'로 입력하세요. 예) npc:NPC001 / NPC001");

                    who = Game.Dialogue.SpeakerKind.Npc;
                    whoNpc = spRaw; // 그대로 NPC ID로 사용
                }

                var dl = new Game.Dialogue.DialogueLine
                {
                    dialogueId = Get(c, col, "dialogueid"),
                    dialogueIndex = ParseInt(Get(c, col, "dialogueindex")),
                    nextDialogue = ParseInt(Get(c, col, "nextdialogue"), 0),
                    speakerKind = who,
                    speakerNpcId = whoNpc,
                    textKey = Get(c, col, "textkey")
                };
                outLines.Add(dl);
            }
            catch (Exception ex)
            {
                Debug.LogError($"LINES 행 파싱 실패: {ex.Message}\n원본: {row}");
            }
        }

        var asset = AssetDatabase.LoadAssetAtPath<Game.Dialogue.DialogueLinesTable>(LinesPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<Game.Dialogue.DialogueLinesTable>();
            AssetDatabase.CreateAsset(asset, LinesPath);
        }
        asset.lines = outLines;
        EditorUtility.SetDirty(asset);
        Debug.Log($"LINES 임포트: {outLines.Count} lines → {LinesPath}");
    }

    static List<string> AssembleLogicalRows(List<string> physical, int startIndex, int minCols)
    {
        var result = new List<string>();
        var buf = "";
        int cols(string s) => (s ?? "").Split('\t').Length;

        for (int i = startIndex; i < physical.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(physical[i])) continue;

            if (string.IsNullOrEmpty(buf))
                buf = physical[i];
            else
                buf += "\n" + physical[i];   // 셀 내 줄바꿈으로 이어진 줄을 합침

            if (cols(buf) >= minCols)
            {
                result.Add(buf);
                buf = "";
            }
            // cols가 모자라면 다음 줄과 계속 합친다
        }

        // 마지막 버퍼가 남아있으면 컬럼이 적어도 그대로 추가(로깅만)
        if (!string.IsNullOrEmpty(buf))
        {
            Debug.LogWarning($"LINES: 마지막 버퍼 컬럼 수가 {cols(buf)}로 {minCols}보다 적습니다. 그대로 추가합니다.\n{buf}");
            result.Add(buf);
        }
        return result;
    }

    // ── helpers
    static string StripBom(string s) => (!string.IsNullOrEmpty(s) && s[0] == '\uFEFF') ? s.Substring(1) : s;
    static List<string> SplitLines(string s) => (s ?? "").Replace("\r\n", "\n").Replace("\r", "\n").Split('\n').ToList();
    static string[] SplitTsv(string line) => (line ?? "").Split('\t');
    static Dictionary<string, int> Map(string[] header)
    {
        var m = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < header.Length; i++) { var h = (header[i] ?? "").Trim(); if (!string.IsNullOrEmpty(h)) m[h] = i; }
        return m;
    }
    static string Get(string[] cells, Dictionary<string, int> col, string name)
    { if (!col.TryGetValue(name, out var idx)) return ""; return (idx < 0 || idx >= cells.Length) ? "" : (cells[idx] ?? "").Trim(); }
    static int ParseInt(string s, int fb = 0) => int.TryParse(s, out var v) ? v : fb;
    static int ParseEnumishInt(string s, int fb = 0) { if (string.IsNullOrWhiteSpace(s)) return fb; var t = s.Trim(); if (t.Equals("none", StringComparison.OrdinalIgnoreCase)) return 0; return int.TryParse(t, out var v) ? v : fb; }
    static string NormalizeNone(string s) { if (string.IsNullOrWhiteSpace(s)) return ""; return s.Trim().Equals("none", StringComparison.OrdinalIgnoreCase) ? "" : s.Trim(); }
    static void EnsureFolder(string folder)
    {
        var parts = folder.Split('/');
        string cur = ""; for (int i = 0; i < parts.Length; i++) { cur = (i == 0) ? parts[0] : $"{cur}/{parts[i]}"; if (!AssetDatabase.IsValidFolder(cur)) { var parent = string.Join("/", parts.Take(i)); if (string.IsNullOrEmpty(parent)) parent = "Assets"; AssetDatabase.CreateFolder(parent, parts[i]); } }
    }
    static void Err(string m) { EditorUtility.DisplayDialog("오류", m, "OK"); Debug.LogError(m); }

    static string Unquote(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        // 양쪽에 같은 따옴표가 있으면 벗겨내기 (엑셀/시트에서 복사된 값 대비)
        if ((s.StartsWith("\"") && s.EndsWith("\"")) || (s.StartsWith("'") && s.EndsWith("'")))
            return s.Substring(1, s.Length - 2);
        return s;
    }
}
