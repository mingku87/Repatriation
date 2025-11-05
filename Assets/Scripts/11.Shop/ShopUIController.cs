using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIController : MonoBehaviour
{
    static ShopUIController _cached;
    public static ShopUIController Get()
        => _cached ? _cached : (_cached = FindObjectOfType<ShopUIController>(true));

    [Header("References")]
    [SerializeField] private GameObject root;
    [SerializeField] private ShopPanel panel;
    [SerializeField] private GameObject bg;
    [SerializeField] private Image shopNpcImage;
    [SerializeField] private TextBubbleAuto bubble;

    void Awake()
    {
        _cached = this;
        if (!root) root = gameObject;
        if (!panel && root) panel = root.GetComponentInChildren<ShopPanel>(true);
        if (root) root.SetActive(false);
    }

    /// <summary>
    /// 상점 UI 열기 (NPC ID와 상점 ID에 맞춰 패널/이미지/버블 설정)
    /// </summary>
    public void OpenForShop(string shopId, string npcId, string bubbleKeyOrText = null, bool isLocKey = true)
    {
        if (!root || !panel)
        {
            Debug.LogWarning("[ShopUI] root 또는 panel 미지정");
            return;
        }

        // 루트/BG/패널 활성화
        if (!root.activeSelf) root.SetActive(true);
        if (bg) bg.SetActive(true);
        if (!panel.gameObject.activeSelf)
            panel.gameObject.SetActive(true);

        // NPC 스탠드 이미지 자동 교체
        TrySetNpcStandImage(npcId);

        // 버블 표시
        ShowBubble(bubbleKeyOrText, isLocKey, npcId);

        // 상점 슬롯 갱신
        panel.SetShopAndRefresh(shopId);
    }

    /// <summary>
    /// npcId에 따라 Resources/Images/Stand/ShopNPCStand### 스프라이트 자동 교체
    /// </summary>
    private void TrySetNpcStandImage(string npcId)
    {
        if (!shopNpcImage)
        {
            Debug.LogWarning("[ShopUI] shopNpcImage 미지정");
            return;
        }

        if (string.IsNullOrEmpty(npcId))
        {
            Debug.LogWarning("[ShopUI] npcId 비어 있음");
            return;
        }

        // 숫자 추출 (shop001 → 001)
        var digits = Regex.Replace(npcId, @"\D", "");
        if (string.IsNullOrEmpty(digits))
            digits = npcId;

        var spriteName = $"ShopNPCStand{digits.PadLeft(3, '0')}";
        var path = $"Images/Stand/{spriteName}"; // Resources 경로

        var sprite = Resources.Load<Sprite>(path);
        if (!sprite)
        {
            Debug.LogWarning($"[ShopUI] NPC 스탠드 스프라이트를 찾지 못함: Resources/{path}");
            return;
        }

        shopNpcImage.sprite = sprite;
        shopNpcImage.preserveAspect = true;
        if (!shopNpcImage.gameObject.activeSelf)
            shopNpcImage.gameObject.SetActive(true);

        Debug.Log($"[ShopUI] NPC 스탠드 적용 완료: npcId={npcId} → {path}");
    }

    /// <summary>
    /// TextBubbleAuto를 활성화하고 텍스트를 표시
    /// </summary>
    private void ShowBubble(string bubbleKeyOrText, bool isLocKey, string npcId)
    {
        if (!bubble)
        {
            Debug.LogWarning("[ShopUI] bubble 미지정");
            return;
        }

        // 1) 활성 보장
        if (!bubble.gameObject.activeSelf)
            bubble.gameObject.SetActive(true);

        // 2) 내용 구성 (네가 나중에 조건 분기할 수 있게 기본값만 넣음)
        string textToShow = string.IsNullOrEmpty(bubbleKeyOrText)
            ? $"안녕! ({npcId}) 상점에 온 걸 환영해."
            : bubbleKeyOrText;

        // 3) CanvasGroup 알파 보장
        var cg = bubble.GetComponent<CanvasGroup>() ?? bubble.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true;

        // 4) 메서드가 있으면(시그니처 불문) 호출 → 없으면 텍스트 직접 세팅
        bool invoked = false;
        var t = bubble.GetType();

        // (a) Try: Show(string,bool)
        var m = t.GetMethod("Show", new[] { typeof(string), typeof(bool) });
        if (m != null) { m.Invoke(bubble, new object[] { textToShow, isLocKey }); invoked = true; }

        // (b) Try: Show(string)
        if (!invoked)
        {
            m = t.GetMethod("Show", new[] { typeof(string) });
            if (m != null) { m.Invoke(bubble, new object[] { textToShow }); invoked = true; }
        }

        // (c) Try: SetText(string) / ShowText(string) 등 일반 패턴
        if (!invoked)
        {
            m = t.GetMethod("SetText", new[] { typeof(string) }) ??
                t.GetMethod("ShowText", new[] { typeof(string) });
            if (m != null) { m.Invoke(bubble, new object[] { textToShow }); invoked = true; }
        }

        // (d) 메서드가 전혀 없으면: 자식의 Text / TMP_Text를 직접 세팅
        if (!invoked)
        {
            var uiText = bubble.GetComponentInChildren<Text>(true);
            if (uiText) uiText.text = textToShow;
            else
            {
                var tmp = bubble.GetComponentInChildren<TMPro.TMP_Text>(true); // using 없이 풀네임 사용
                if (tmp) tmp.text = textToShow;
                else Debug.LogWarning("[ShopUI] 버블에 Text / TMP_Text가 없어 텍스트를 설정할 수 없습니다.");
            }
        }

        Debug.Log($"[ShopUI] Bubble set. invokedMethod={(invoked ? "yes" : "no-direct-text")}, text=\"{textToShow}\"");
    }

    public void Close()
    {
        if (root) root.SetActive(false);
    }
}
