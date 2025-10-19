using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Game.Dialogue; // �� ��Ÿ�� Ÿ�� ����

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
        EditorGUILayout.LabelField("Dialogue.tsv (��Ÿ) �ٿ��ֱ�", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("���: index, npcid, dialoguetype, conditiontype, conditionkey, dialogueid", MessageType.Info);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Ŭ������ ���̱�", GUILayout.Width(140))) metaTsv = GUIUtility.systemCopyBuffer ?? "";
            if (GUILayout.Button("�����", GUILayout.Width(90))) metaTsv = "";
            GUILayout.FlexibleSpace();
        }
        s1 = EditorGUILayout.BeginScrollView(s1, GUILayout.Height(160));
        metaTsv = EditorGUILayout.TextArea(metaTsv, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Dialogue_Text.tsv (����) �ٿ��ֱ�", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("���: dialogueid, dialogueindex, nextdialogue, speaker, textkey  (preview ����)\n" +
                                "speaker�� �ݵ�� 'player' �Ǵ� 'npc:ID' �����̾�� �մϴ�.", MessageType.Info);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Ŭ������ ���̱�", GUILayout.Width(140))) textTsv = GUIUtility.systemCopyBuffer ?? "";
            if (GUILayout.Button("�����", GUILayout.Width(90))) textTsv = "";
            GUILayout.FlexibleSpace();
        }
        s2 = EditorGUILayout.BeginScrollView(s2, GUILayout.Height(220));
        textTsv = EditorGUILayout.TextArea(textTsv, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(12);
        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(metaTsv) && string.IsNullOrWhiteSpace(textTsv)))
        {
            if (GUILayout.Button("Import (�ٿ����� ���� ���)", GUILayout.Height(34)))
            {
                EnsureFolder(Folder);
                if (!string.IsNullOrWhiteSpace(metaTsv)) ImportMeta(metaTsv);
                if (!string.IsNullOrWhiteSpace(textTsv)) ImportLines(textTsv);
                AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("�Ϸ�", "����Ʈ�� �Ϸ�Ǿ����ϴ�.", "OK");
            }
        }
    }

    // ���� META
    void ImportMeta(string raw)
    {
        var lines = SplitLines(StripBom(raw));
        if (lines.Count == 0) { Err("META ��� ����"); return; }

        var header = SplitTsv(lines[0]);
        var col = Map(header);
        string[] req = { "index", "npcid", "dialoguetype", "conditiontype", "conditionkey", "dialogueid" };
        foreach (var r in req) if (!col.ContainsKey(r)) { Err($"META ��� ����: {r}"); return; }

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
                Debug.LogError($"META {i + 1}�� �Ľ� ����: {ex.Message}\n{row}");
            }
        }

        var asset = AssetDatabase.LoadAssetAtPath<DialogueMetaTable>(MetaPath);
        if (asset == null) { asset = ScriptableObject.CreateInstance<DialogueMetaTable>(); AssetDatabase.CreateAsset(asset, MetaPath); }
        asset.rows = list;
        EditorUtility.SetDirty(asset);
        Debug.Log($"META ����Ʈ: {list.Count} rows �� {MetaPath}");
    }

    // ���� LINES
    void ImportLines(string raw)
    {
        var physical = SplitLines(StripBom(raw));
        if (physical.Count == 0) { Err("LINES ��� ����"); return; }

        // ��� ã��
        int headerIdx = physical.FindIndex(s => !string.IsNullOrWhiteSpace(s));
        if (headerIdx < 0) { Err("LINES ��� ����"); return; }

        var header = SplitTsv(physical[headerIdx]);
        var col = Map(header);
        string[] req = { "dialogueid", "dialogueindex", "nextdialogue", "speaker", "textkey" };
        foreach (var r in req)
            if (!col.ContainsKey(r)) { Err($"LINES ��� ����: {r}"); return; }

        // �� �� �ٹٲ� ����
        var logicalRows = AssembleLogicalRows(physical, headerIdx + 1, minCols: 5);

        var outLines = new List<Game.Dialogue.DialogueLine>();

        foreach (var row in logicalRows)
        {
            if (string.IsNullOrWhiteSpace(row)) continue;
            var c = SplitTsv(row);
            if (c.All(string.IsNullOrEmpty)) continue;

            try
            {
                // ������ speaker �Ľ� ����� �̰ɷ� ��ü
                var spRaw0 = (Get(c, col, "speaker") ?? "").Trim();
                var spRaw = Unquote(spRaw0); // "NPC001" ���� ����ǥ�� ������ ����
                Game.Dialogue.SpeakerKind who;
                string whoNpc = "";

                // 1) player
                if (string.IsNullOrEmpty(spRaw) || spRaw.Equals("player", StringComparison.OrdinalIgnoreCase))
                {
                    who = Game.Dialogue.SpeakerKind.Player;
                }
                // 2) npc:ID  (��ҹ��� ������ ���)
                else if (spRaw.Length >= 4 && spRaw.Substring(0, 4).Equals("npc:", StringComparison.OrdinalIgnoreCase))
                {
                    var id = spRaw.Substring(spRaw.IndexOf(':') + 1).Trim();
                    if (string.IsNullOrEmpty(id))
                        throw new Exception("speaker�� 'npc:' �����ε� ID�� ��� �ֽ��ϴ�. ��) npc:NPC001");
                    who = Game.Dialogue.SpeakerKind.Npc;
                    whoNpc = id;
                }
                // 3) �ݷ� ���� ID�� �� ��� �� NPC�� �ؼ�
                else
                {
                    // "npc" �� �ܾ ���� ���� �Ǽ��� Ȯ���� ������ ������ ó��
                    if (spRaw.Equals("npc", StringComparison.OrdinalIgnoreCase))
                        throw new Exception("speaker�� 'npc'�� �ԷµǾ����ϴ�. 'npc:ID' �Ǵ� 'ID'�� �Է��ϼ���. ��) npc:NPC001 / NPC001");

                    who = Game.Dialogue.SpeakerKind.Npc;
                    whoNpc = spRaw; // �״�� NPC ID�� ���
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
                Debug.LogError($"LINES �� �Ľ� ����: {ex.Message}\n����: {row}");
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
        Debug.Log($"LINES ����Ʈ: {outLines.Count} lines �� {LinesPath}");
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
                buf += "\n" + physical[i];   // �� �� �ٹٲ����� �̾��� ���� ��ħ

            if (cols(buf) >= minCols)
            {
                result.Add(buf);
                buf = "";
            }
            // cols�� ���ڶ�� ���� �ٰ� ��� ��ģ��
        }

        // ������ ���۰� ���������� �÷��� ��� �״�� �߰�(�α븸)
        if (!string.IsNullOrEmpty(buf))
        {
            Debug.LogWarning($"LINES: ������ ���� �÷� ���� {cols(buf)}�� {minCols}���� �����ϴ�. �״�� �߰��մϴ�.\n{buf}");
            result.Add(buf);
        }
        return result;
    }

    // ���� helpers
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
    static void Err(string m) { EditorUtility.DisplayDialog("����", m, "OK"); Debug.LogError(m); }

    static string Unquote(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        // ���ʿ� ���� ����ǥ�� ������ ���ܳ��� (����/��Ʈ���� ����� �� ���)
        if ((s.StartsWith("\"") && s.EndsWith("\"")) || (s.StartsWith("'") && s.EndsWith("'")))
            return s.Substring(1, s.Length - 2);
        return s;
    }
}
