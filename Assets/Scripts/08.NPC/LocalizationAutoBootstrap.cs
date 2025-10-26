using System.Linq;
using UnityEngine;

public static class LocalizationAutoBootstrap
{
    // �� �ε� "����" �ڵ� ����
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureLocalizationManager()
    {
        // �̹� �����ϸ� �н�
        if (LocalizationManager.Instance != null) return;

        // 1) �Ŵ��� ����
        var go = new GameObject("LocalizationManager");
        var mgr = go.AddComponent<LocalizationManager>();

        // 2) Resources/Localization �������� LocalizationTable ���� �ε�
        var all = Resources.LoadAll<LocalizationTable>("");
        if (all != null && all.Length > 0)
        {
            // language �ʵ�� �ڵ� ��Ī
            mgr.tableKo = all.FirstOrDefault(t => t.language == SystemLanguage.Korean);
            mgr.tableEn = all.FirstOrDefault(t => t.language == SystemLanguage.English);
        }
        else
        {
            Debug.LogWarning("[Localization] No LocalizationTable assets found under Resources/Localization");
        }

        // 3) �ý��� ���� �ʱ� �ε� (�ѱ��̸� Korean, �ƴϸ� ko ���� �� en/ko ������)
        var sys = Application.systemLanguage;
        if (sys == SystemLanguage.Korean && mgr.tableKo != null)
            mgr.LoadLanguage(SystemLanguage.Korean);
        else if (sys == SystemLanguage.English && mgr.tableEn != null)
            mgr.LoadLanguage(SystemLanguage.English);
        else
        {
            // ����
            if (mgr.tableKo != null) mgr.LoadLanguage(SystemLanguage.Korean);
            else if (mgr.tableEn != null) mgr.LoadLanguage(SystemLanguage.English);
            else mgr.LoadLanguage(mgr.language); // �� �������� �̺�Ʈ�� ����
        }
    }
}
