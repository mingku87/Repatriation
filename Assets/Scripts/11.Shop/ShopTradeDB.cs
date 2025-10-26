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
/// 단일 에셋(DB) + 런타임 조회 API + (에디터에서) 구글시트 새로고침까지 모두 포함
/// </summary>
[CreateAssetMenu(menuName = "Shop/Shop Trade DB", fileName = "ShopTradeDB")]
public class ShopTradeDB : ScriptableObject
{
    [Header("Google Sheet URL (한 번 넣으면 저장됨)")]
    [Tooltip("보기 링크(edit#gid=...), 또는 export?format=tsv 링크 아무거나.")]
    public string googleSheetUrl;

    [Header("Offers (저장 데이터)")]
    public List<ShopTradeOffer> offers = new List<ShopTradeOffer>();

    // --- 런타임 조회용 인덱스 ---
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
            // 중복 방지(같은 itemId+sellItemId)
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
    // ===================== 에디터 전용: 구글시트 새로고침 =====================
    const string H_ShopId = "shopid";
    const string H_ItemId = "itemid";
    const string H_SellItem = "sellitem";
    const string H_Price = "price";

    /// <summary>Tools 메뉴에서 호출: 선택한 DB를 새로고침</summary>
    [UnityEditor.MenuItem("Tools/Shop Trade/Refresh DB From Google Sheet...", priority = 0)]
    static void Menu_RefreshDBFromGoogleSheet()
    {
        var db = GetSelectedDB();
        if (db == null)
        {
            UnityEditor.EditorUtility.DisplayDialog("Shop Trade",
                "프로젝트에서 ShopTradeDB 에셋을 선택해주세요.\n(Project → Create → Shop → Shop Trade DB 로 생성 가능)",
                "확인");
            return;
        }

        // 입력창: 저장된 값 기본
        string url = db.googleSheetUrl ?? "";
        url = UnityEditor.EditorUtility.DisplayDialogComplex("Shop Trade",
            "Google Sheet URL을 입력하세요.\n(입력한 URL은 DB에 저장됩니다.)",
            "확인", "취소", "붙여넣기") switch
        {
            2 => (db.googleSheetUrl = GUIUtility.systemCopyBuffer), // '붙여넣기' 선택
            _ => db.googleSheetUrl                                // 확인/취소→ 기존 값 유지
        };

        // 취소였고 기존 값도 없다면 중단
        if (string.IsNullOrWhiteSpace(url))
        {
            UnityEditor.EditorUtility.DisplayDialog("Shop Trade", "URL이 비어있습니다.", "확인");
            return;
        }

        // 입력창이 없으면 TextField 쓰자
        url = UnityEditor.EditorUtility.DisplayDialog("Shop Trade", $"이 URL로 새로고침할까요?\n\n{url}", "진행", "취소")
            ? url : null;

        if (string.IsNullOrEmpty(url)) return;

        db.googleSheetUrl = url; // 저장(유지)
        UnityEditor.EditorUtility.SetDirty(db);

        if (!TryBuildExportTsvUrl(url, out var exportUrl, out var msg))
        {
            UnityEditor.EditorUtility.DisplayDialog("Shop Trade", "URL 분석 실패: " + msg, "확인");
            return;
        }

        DownloadAndApplyInEditor(db, exportUrl);
    }

    static ShopTradeDB GetSelectedDB()
    {
        var obj = UnityEditor.Selection.activeObject as ShopTradeDB;
        if (obj != null) return obj;
        // 프로젝트 전체에서 첫 번째 DB 검색
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
            if (!mId.Success) { message = "문서 ID를 찾지 못했습니다."; return false; }
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

        Debug.Log("[ShopTradeDB] 다운로드 요청: " + url);
        UnityEditor.EditorUtility.DisplayProgressBar("Shop Trade", "Google Sheet에서 불러오는 중…", 0f);

        void Poll()
        {
            UnityEditor.EditorUtility.DisplayProgressBar("Shop Trade", "Google Sheet에서 불러오는 중…",
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
                UnityEditor.EditorUtility.DisplayDialog("Shop Trade", "다운로드 실패: " + req.error, "확인");
                req.Dispose();
                return;
            }

            var text = req.downloadHandler.text;
            if (!TryParseDelimited(text, out var parsed, out var err))
            {
                UnityEditor.EditorUtility.DisplayDialog("Shop Trade", "파싱 실패: " + err, "확인");
                req.Dispose();
                return;
            }

            UnityEditor.Undo.RecordObject(db, "ShopTrade Import (Google Sheets)");
            db.offers = parsed;
            db.BuildIndex();
            UnityEditor.EditorUtility.SetDirty(db);
            Debug.Log($"[ShopTradeDB] 새로고침 완료. 상점 수: {db._byShop.Count}");
            req.Dispose();
        }

        UnityEditor.EditorApplication.update += Poll;
    }

    // CSV/TSV 자동감지 + 헤더 정규화 + 동의어 허용 파서
    static bool TryParseDelimited(string text, out List<ShopTradeOffer> result, out string message)
    {
        result = new List<ShopTradeOffer>();
        message = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            message = "빈 문서";
            return false;
        }

        // 첫 줄로 구분자 감지 (쉼표/탭 중 많은 쪽)
        string firstLine;
        using (var reader = new StringReader(text))
            firstLine = reader.ReadLine() ?? "";

        int commaCount = firstLine.Split(',').Length;
        int tabCount = firstLine.Split('\t').Length;
        char delim = (tabCount > commaCount) ? '\t' : ',';

        // 공통 유틸
        string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("\uFEFF", "") // BOM 제거
                 .Replace("\u00A0", " ") // NBSP 제거
                 .Trim()
                 .ToLowerInvariant();
            return new string(s.Where(char.IsLetterOrDigit).ToArray());
        }

        // CSV 안전 Split (따옴표 감안)
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

        // 동의어 테이블
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

            // 주석/공백 제거
            var trimmed = line.Trim().TrimStart('\uFEFF', '\u200B');
            if (trimmed.StartsWith("#") || trimmed.StartsWith("//")) continue;

            var cols = SplitRow(trimmed);

            if (!headerParsed)
            {
                // 헤더
                for (int i = 0; i < cols.Count; i++)
                {
                    string key = Normalize(cols[i]);
                    if (!headerIndex.ContainsKey(key))
                        headerIndex[key] = i;
                }

                // 동의어 탐색
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
                        Debug.LogError($"[ShopTradeDB] 헤더 누락: {kv.Key} / 감지된 헤더: {string.Join(", ", headerIndex.Keys)}");
#endif
                        message = $"헤더 누락: {kv.Key}";
                        return false;
                    }
                }

                headerParsed = true;
                continue;
            }

            // 데이터
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



    // 감지된 구분자
    static char _delim = '\t';

    // CSV에서 따옴표를 고려해서 안전하게 split
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
                // 이중따옴표 "" → 실제 " 로 처리
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
