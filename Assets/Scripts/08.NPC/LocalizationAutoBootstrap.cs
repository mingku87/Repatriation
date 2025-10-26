using System.Linq;
using UnityEngine;

public static class LocalizationAutoBootstrap
{
    // 씬 로드 "전에" 자동 실행
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureLocalizationManager()
    {
        // 이미 존재하면 패스
        if (LocalizationManager.Instance != null) return;

        // 1) 매니저 생성
        var go = new GameObject("LocalizationManager");
        var mgr = go.AddComponent<LocalizationManager>();

        // 2) Resources/Localization 폴더에서 LocalizationTable 전부 로드
        var all = Resources.LoadAll<LocalizationTable>("");
        if (all != null && all.Length > 0)
        {
            // language 필드로 자동 매칭
            mgr.tableKo = all.FirstOrDefault(t => t.language == SystemLanguage.Korean);
            mgr.tableEn = all.FirstOrDefault(t => t.language == SystemLanguage.English);
        }
        else
        {
            Debug.LogWarning("[Localization] No LocalizationTable assets found under Resources/Localization");
        }

        // 3) 시스템 언어로 초기 로드 (한국이면 Korean, 아니면 ko 없을 땐 en/ko 순으로)
        var sys = Application.systemLanguage;
        if (sys == SystemLanguage.Korean && mgr.tableKo != null)
            mgr.LoadLanguage(SystemLanguage.Korean);
        else if (sys == SystemLanguage.English && mgr.tableEn != null)
            mgr.LoadLanguage(SystemLanguage.English);
        else
        {
            // 폴백
            if (mgr.tableKo != null) mgr.LoadLanguage(SystemLanguage.Korean);
            else if (mgr.tableEn != null) mgr.LoadLanguage(SystemLanguage.English);
            else mgr.LoadLanguage(mgr.language); // 빈 맵이지만 이벤트는 쏴줌
        }
    }
}
