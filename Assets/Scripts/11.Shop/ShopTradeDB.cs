using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class ShopTradeOffer
{
    public string shopId;
    public int itemId;
    public int sellItemId;
    public int price;
}

/// <summary>
/// ���� ����(DB) + ��Ÿ�� ��ȸ API + (�����Ϳ���) ���۽�Ʈ ���ΰ�ħ���� ��� ����
/// </summary>
[CreateAssetMenu(menuName = "Shop/Shop Trade DB", fileName = "ShopTradeDB")]
public class ShopTradeDB : ScriptableObject
{
    [Header("Google Sheet URL (�� �� ������ �����)")]
    [Tooltip("���� ��ũ(edit#gid=...), �Ǵ� export?format=tsv ��ũ �ƹ��ų�.")]
    public string googleSheetUrl;

    [Header("Offers (���� ������)")]
    public List<ShopTradeOffer> offers = new List<ShopTradeOffer>();

    // --- ��Ÿ�� ��ȸ�� �ε��� ---
    [NonSerialized] private Dictionary<string, List<ShopTradeOffer>> _byShop;

    void OnEnable() => BuildIndex();

    public void BuildIndex()
    {
        _byShop = new Dictionary<string, List<ShopTradeOffer>>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < offers.Count; i++)
        {
            var o = offers[i];
            if (o == null || string.IsNullOrEmpty(o.shopId)) continue;
            if (!_byShop.TryGetValue(o.shopId, out var list))
            {
                list = new List<ShopTradeOffer>();
                _byShop.Add(o.shopId, list);
            }
            // �ߺ� ����(���� itemId+sellItemId)
            bool dup = false;
            for (int j = 0; j < list.Count; j++)
            {
                var e = list[j];
                if (e.itemId == o.itemId && e.sellItemId == o.sellItemId) { dup = true; break; }
            }
            if (!dup) list.Add(o);
        }
    }

    public List<ShopTradeOffer> GetOffers(string shopId)
    {
        if (_byShop == null) BuildIndex();
        return _byShop != null && _byShop.TryGetValue(shopId, out var list) ? list : _empty;
    }
    static readonly List<ShopTradeOffer> _empty = new List<ShopTradeOffer>();

#if UNITY_EDITOR
    // ===================== ������ ����: ���۽�Ʈ ���ΰ�ħ =====================
    const string H_ShopId = "shopid";
    const string H_ItemId = "itemid";
    const string H_SellItem = "sellitem";
    const string H_Price = "price";

    /// <summary>Tools �޴����� ȣ��: ������ DB�� ���ΰ�ħ</summary>
    [UnityEditor.MenuItem("Tools/Shop Trade/Refresh DB From Google Sheet...", priority = 0)]
    static void Menu_RefreshDBFromGoogleSheet()
    {
        var db = GetSelectedDB();
        if (db == null)
        {
            UnityEditor.EditorUtility.DisplayDialog("Shop Trade",
                "������Ʈ���� ShopTradeDB ������ �������ּ���.\n(Project �� Create �� Shop �� Shop Trade DB �� ���� ����)",
                "Ȯ��");
            return;
        }

        // �Է�â: ����� �� �⺻
        string url = db.googleSheetUrl ?? "";
        url = UnityEditor.EditorUtility.DisplayDialogComplex("Shop Trade",
            "Google Sheet URL�� �Է��ϼ���.\n(�Է��� URL�� DB�� ����˴ϴ�.)",
            "Ȯ��", "���", "�ٿ��ֱ�") switch
        {
            2 => (db.googleSheetUrl = GUIUtility.systemCopyBuffer), // '�ٿ��ֱ�' ����
            _ => db.googleSheetUrl                                // Ȯ��/��ҡ� ���� �� ����
        };

        // ��ҿ��� ���� ���� ���ٸ� �ߴ�
        if (string.IsNullOrWhiteSpace(url))
        {
            UnityEditor.EditorUtility.DisplayDialog("Shop Trade", "URL�� ����ֽ��ϴ�.", "Ȯ��");
            return;
        }

        // �Է�â�� ������ TextField ����
        url = UnityEditor.EditorUtility.DisplayDialog("Shop Trade", $"�� URL�� ���ΰ�ħ�ұ��?\n\n{url}", "����", "���")
            ? url : null;

        if (string.IsNullOrEmpty(url)) return;

        db.googleSheetUrl = url; // ����(����)
        UnityEditor.EditorUtility.SetDirty(db);

        if (!TryBuildExportTsvUrl(url, out var exportUrl, out var msg))
        {
            UnityEditor.EditorUtility.DisplayDialog("Shop Trade", "URL �м� ����: " + msg, "Ȯ��");
            return;
        }

        DownloadAndApplyInEditor(db, exportUrl);
    }

    static ShopTradeDB GetSelectedDB()
    {
        var obj = UnityEditor.Selection.activeObject as ShopTradeDB;
        if (obj != null) return obj;
        // ������Ʈ ��ü���� ù ��° DB �˻�
        var guids = UnityEditor.AssetDatabase.FindAssets("t:ShopTradeDB");
        if (guids != null && guids.Length > 0)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<ShopTradeDB>(path);
        }
        return null;
    }

    static bool TryBuildExportTsvUrl(string anyUrl, out string exportUrl, out string message)
    {
        exportUrl = null; message = null;
        try
        {
            anyUrl = anyUrl.Trim();
            if (anyUrl.Contains("export?"))
            {
                exportUrl = Regex.Replace(anyUrl, "format=csv", "format=tsv");
                return true;
            }
            var mId = Regex.Match(anyUrl, @"spreadsheets\/d\/([a-zA-Z0-9-_]+)");
            var mGid = Regex.Match(anyUrl, @"[?&#]gid=([0-9]+)");
            if (!mId.Success) { message = "���� ID�� ã�� ���߽��ϴ�."; return false; }
            var id = mId.Groups[1].Value;
            var gid = mGid.Success ? mGid.Groups[1].Value : "0";
            exportUrl = $"https://docs.google.com/spreadsheets/d/{id}/export?format=tsv&gid={gid}";
            return true;
        }
        catch (Exception e) { message = e.Message; return false; }
    }

    static void DownloadAndApplyInEditor(ShopTradeDB db, string url)
    {
        var req = UnityEngine.Networking.UnityWebRequest.Get(url);
        var op = req.SendWebRequest();

        Debug.Log("[ShopTradeDB] �ٿ�ε� ��û: " + url);
        UnityEditor.EditorUtility.DisplayProgressBar("Shop Trade", "Google Sheet���� �ҷ����� �ߡ�", 0f);

        void Poll()
        {
            UnityEditor.EditorUtility.DisplayProgressBar("Shop Trade", "Google Sheet���� �ҷ����� �ߡ�",
                Mathf.Clamp01(op.progress));

            if (!op.isDone) return;

            UnityEditor.EditorApplication.update -= Poll;
            UnityEditor.EditorUtility.ClearProgressBar();

#if UNITY_2020_2_OR_NEWER
            if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                UnityEditor.EditorUtility.DisplayDialog("Shop Trade", "�ٿ�ε� ����: " + req.error, "Ȯ��");
                req.Dispose();
                return;
            }

            var text = req.downloadHandler.text;
            if (!TryParseDelimited(text, out var parsed, out var err))
            {
                UnityEditor.EditorUtility.DisplayDialog("Shop Trade", "�Ľ� ����: " + err, "Ȯ��");
                req.Dispose();
                return;
            }

            UnityEditor.Undo.RecordObject(db, "ShopTrade Import (Google Sheets)");
            db.offers = parsed;
            db.BuildIndex();
            UnityEditor.EditorUtility.SetDirty(db);
            Debug.Log($"[ShopTradeDB] ���ΰ�ħ �Ϸ�. ���� ��: {db._byShop.Count}");
            req.Dispose();
        }

        UnityEditor.EditorApplication.update += Poll;
    }

    // CSV/TSV �ڵ����� + ��� ����ȭ + ���Ǿ� ��� �ļ�
    static bool TryParseDelimited(string text, out List<ShopTradeOffer> result, out string message)
    {
        result = new List<ShopTradeOffer>();
        message = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            message = "�� ����";
            return false;
        }

        // ù �ٷ� ������ ���� (��ǥ/�� �� ���� ��)
        string firstLine;
        using (var reader = new StringReader(text))
            firstLine = reader.ReadLine() ?? "";

        int commaCount = firstLine.Split(',').Length;
        int tabCount = firstLine.Split('\t').Length;
        char delim = (tabCount > commaCount) ? '\t' : ',';

        // ���� ��ƿ
        string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("\uFEFF", "") // BOM ����
                 .Replace("\u00A0", " ") // NBSP ����
                 .Trim()
                 .ToLowerInvariant();
            return new string(s.Where(char.IsLetterOrDigit).ToArray());
        }

        // CSV ���� Split (����ǥ ����)
        List<string> SplitRow(string line)
        {
            var list = new List<string>();
            if (line == null) return list;

            bool inQuotes = false;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"'); i++;
                    }
                    else inQuotes = !inQuotes;
                    continue;
                }

                if (c == delim && !inQuotes)
                {
                    list.Add(sb.ToString());
                    sb.Length = 0;
                    continue;
                }

                sb.Append(c);
            }
            list.Add(sb.ToString());
            return list;
        }

        // ���Ǿ� ���̺�
        var synonyms = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        { "shopid",   new[]{ "shopid","shop","npc","npcid" } },
        { "itemid",   new[]{ "itemid","item","buyid","rewardid" } },
        { "sellitem", new[]{ "sellitem","needitem","costitem","requireitem" } },
        { "price",    new[]{ "price","count","num","amount","quantity" } },
    };

        var headerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        bool headerParsed = false;
        int lineNo = 0;
        var reader2 = new StringReader(text);

        while (true)
        {
            string line = reader2.ReadLine();
            if (line == null) break;
            lineNo++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            // �ּ�/���� ����
            var trimmed = line.Trim().TrimStart('\uFEFF', '\u200B');
            if (trimmed.StartsWith("#") || trimmed.StartsWith("//")) continue;

            var cols = SplitRow(trimmed);

            if (!headerParsed)
            {
                // ���
                for (int i = 0; i < cols.Count; i++)
                {
                    string key = Normalize(cols[i]);
                    if (!headerIndex.ContainsKey(key))
                        headerIndex[key] = i;
                }

                // ���Ǿ� Ž��
                foreach (var kv in synonyms)
                {
                    bool found = false;
                    foreach (var syn in kv.Value)
                    {
                        if (headerIndex.ContainsKey(Normalize(syn)))
                        {
                            headerIndex[kv.Key] = headerIndex[Normalize(syn)];
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
#if UNITY_EDITOR
                        Debug.LogError($"[ShopTradeDB] ��� ����: {kv.Key} / ������ ���: {string.Join(", ", headerIndex.Keys)}");
#endif
                        message = $"��� ����: {kv.Key}";
                        return false;
                    }
                }

                headerParsed = true;
                continue;
            }

            // ������
            bool Has(string key) => headerIndex.TryGetValue(key, out var idx) && idx < cols.Count;
            if (!Has("shopid") || !Has("itemid") || !Has("sellitem") || !Has("price")) continue;

            string shopId = cols[headerIndex["shopid"]].Trim();
            if (string.IsNullOrEmpty(shopId)) continue;

            bool TryInt(string s, out int v)
            {
                s = (s ?? "").Trim().Trim('"');
                return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v);
            }

            if (!TryInt(cols[headerIndex["itemid"]], out var itemId)) continue;
            if (!TryInt(cols[headerIndex["sellitem"]], out var sellItemId)) continue;
            if (!TryInt(cols[headerIndex["price"]], out var price)) continue;

            result.Add(new ShopTradeOffer
            {
                shopId = shopId,
                itemId = itemId,
                sellItemId = sellItemId,
                price = price
            });
        }

        return true;
    }



    // ������ ������
    static char _delim = '\t';

    // CSV���� ����ǥ�� ����ؼ� �����ϰ� split
    static List<string> SplitRow(string line, char delim)
    {
        var list = new List<string>(16);
        if (string.IsNullOrEmpty(line)) { list.Add(""); return list; }

        bool inQuotes = false;
        var sb = new System.Text.StringBuilder(64);

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // ���ߵ���ǥ "" �� ���� " �� ó��
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                else inQuotes = !inQuotes;
                continue;
            }

            if (c == delim && !inQuotes)
            {
                list.Add(sb.ToString());
                sb.Length = 0;
                continue;
            }

            sb.Append(c);
        }
        list.Add(sb.ToString());
        return list;
    }
#endif
}
