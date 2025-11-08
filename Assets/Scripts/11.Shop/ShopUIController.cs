using System;
using System.Text.RegularExpressions;
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

    // ✅ 인벤토리 연결용 델리게이트(브릿지에서 주입)
    public System.Func<int, int> GetCount;            // (itemId) -> 총 보유 수량
    public System.Func<int, int, bool> TryRemove;     // (itemId, amount) -> 성공/실패
    public System.Action<int, int> Add;               // (itemId, amount)

    void Awake()
    {
        _cached = this;
        if (!root) root = gameObject;
        if (!panel && root) panel = root.GetComponentInChildren<ShopPanel>(true);
        if (root) root.SetActive(false);
    }

    /// <summary>상점 UI 열기 (NPC/버블/슬롯 갱신)</summary>
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
        if (!panel.gameObject.activeSelf) panel.gameObject.SetActive(true);
        var exitButton = root.transform.Find("Exit");
        if (exitButton) exitButton.gameObject.SetActive(true);

        // NPC 스탠드 이미지 자동 교체
        TrySetNpcStandImage(npcId);

        // 버블 표시
        ShowBubble(bubbleKeyOrText, isLocKey, npcId);

        // 상점 슬롯 갱신
        panel.SetShopAndRefresh(shopId);

        // ✅ 상점 열 때마다 슬롯 활성/비활성 재평가
        RefreshAllSlots();
        StartCoroutine(RefreshAllSlotsNextFrame());
    }

    private void TrySetNpcStandImage(string npcId)
    {
        if (!shopNpcImage) { Debug.LogWarning("[ShopUI] shopNpcImage 미지정"); return; }
        if (string.IsNullOrEmpty(npcId)) { Debug.LogWarning("[ShopUI] npcId 비어 있음"); return; }

        var digits = Regex.Replace(npcId, @"\D", "");
        if (string.IsNullOrEmpty(digits)) digits = npcId;

        var spriteName = $"ShopNPCStand{digits.PadLeft(3, '0')}";
        var path = $"Images/Stand/{spriteName}";

        var sprite = Resources.Load<Sprite>(path);
        if (!sprite) { Debug.LogWarning($"[ShopUI] NPC 스탠드 스프라이트를 찾지 못함: Resources/{path}"); return; }

        shopNpcImage.sprite = sprite;
        shopNpcImage.preserveAspect = true;
        if (!shopNpcImage.gameObject.activeSelf) shopNpcImage.gameObject.SetActive(true);

        Debug.Log($"[ShopUI] NPC 스탠드 적용 완료: npcId={npcId} → {path}");
    }

    private void ShowBubble(string bubbleKeyOrText, bool isLocKey, string npcId)
    {
        if (!bubble) { Debug.LogWarning("[ShopUI] bubble 미지정"); return; }

        if (!bubble.gameObject.activeSelf) bubble.gameObject.SetActive(true);

        string textToShow = string.IsNullOrEmpty(bubbleKeyOrText)
            ? $"안녕! ({npcId}) 상점에 온 걸 환영해."
            : bubbleKeyOrText;

        var cg = bubble.GetComponent<CanvasGroup>() ?? bubble.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true;

        bool invoked = false;
        var t = bubble.GetType();

        var m = t.GetMethod("Show", new[] { typeof(string), typeof(bool) });
        if (m != null) { m.Invoke(bubble, new object[] { textToShow, isLocKey }); invoked = true; }

        if (!invoked)
        {
            m = t.GetMethod("Show", new[] { typeof(string) });
            if (m != null) { m.Invoke(bubble, new object[] { textToShow }); invoked = true; }
        }

        if (!invoked)
        {
            m = t.GetMethod("SetText", new[] { typeof(string) }) ??
                t.GetMethod("ShowText", new[] { typeof(string) });
            if (m != null) { m.Invoke(bubble, new object[] { textToShow }); invoked = true; }
        }

        if (!invoked)
        {
            var uiText = bubble.GetComponentInChildren<Text>(true);
            if (uiText) uiText.text = textToShow;
            else
            {
                var tmp = bubble.GetComponentInChildren<TMPro.TMP_Text>(true);
                if (tmp) tmp.text = textToShow;
                else Debug.LogWarning("[ShopUI] 버블에 Text / TMP_Text가 없어 텍스트를 설정할 수 없습니다.");
            }
        }

        Debug.Log($"[ShopUI] Bubble set. invokedMethod={(invoked ? "yes" : "no-direct-text")}, text=\"{textToShow}\"");
    }

    // =============== 구매 흐름 ===============

    /// <summary>슬롯 클릭 시 호출: 수량 팝업 띄우기 → 확인 시 HandleExchangeConfirm</summary>
    public void PromptExchange(ShopTradeOffer offer, string titleLocKey = "exchangetitle")
    {
        if (offer == null) return;
        if (GetCount == null || TryRemove == null || Add == null)
        {
            Debug.LogError("[ShopUI] 인벤 연결(델리게이트) 없음 — Bridge에서 Inject 필요");
            return;
        }

        int ownedCostItem = Mathf.Max(0, GetCount.Invoke(offer.sellItemId));
        int maxByCost = (offer.price <= 0) ? int.MaxValue : (ownedCostItem / Mathf.Max(1, offer.price));
        int maxAmount = Mathf.Clamp(maxByCost, 0, 9999);

        // 타이틀 로컬라이즈
        string title = titleLocKey;
        var lm = LocalizationManager.Instance;
        if (lm) title = lm.GetOrKey(titleLocKey);

        // ✔ 수량 팝업 호출 (DiscardQuantityPopup API 시그니처와 일치)
        var popup = DiscardQuantityPopup.EnsureInstance();
        popup.Show(
            max: maxAmount,
            onConfirm: amount => HandleExchangeConfirm(offer, amount),
            title: title,
            initialValue: (maxAmount > 0 ? 1 : 0)
        );
    }

    /// <summary>실제 거래 처리: (sellItemId)에서 price*amount 차감 → (itemId) amount 지급</summary>
    private void HandleExchangeConfirm(ShopTradeOffer offer, int amount)
    {
        if (GetCount == null || TryRemove == null || Add == null) { Debug.LogError("[ShopUI] 델리게이트 없음"); return; }
        if (offer == null || amount <= 0) return;

        int totalCost = offer.price * amount;
        int owned = GetCount.Invoke(offer.sellItemId);

        if (owned < totalCost)
        {
            Debug.LogWarning($"[ShopUI] 보유 부족: 필요 {totalCost}, 보유 {owned}");
            return;
        }

        if (!TryRemove.Invoke(offer.sellItemId, totalCost))
        {
            Debug.LogWarning("[ShopUI] 차감 실패(동시성/레이스 가능)");
            return;
        }

        Add.Invoke(offer.itemId, amount);
        Debug.Log($"[ShopUI] 거래 완료 sell:{offer.sellItemId} -{totalCost}, item:{offer.itemId} +{amount}");
    }

    // 유틸: afford 계산 (슬롯에서 사용)
    public int GetMaxAffordableAmount(ShopTradeOffer offer)
    {
        if (offer == null || GetCount == null) return 0;
        int owned = Mathf.Max(0, GetCount.Invoke(offer.sellItemId));
        return (offer.price <= 0) ? int.MaxValue : (owned / Mathf.Max(1, offer.price));
    }

    public void Close()
    {
        if (!root) return;

        // 루트 비활성화
        root.SetActive(false);

        // 배경, 버블, NPC 이미지 등도 함께 꺼주면 깔끔
        if (bg) bg.SetActive(false);
        if (shopNpcImage) shopNpcImage.gameObject.SetActive(false);
        if (bubble) bubble.gameObject.SetActive(false);

        Debug.Log("[ShopUI] 상점 창 닫힘");
    }
    public void RefreshAllSlots()
    {
        Transform scope = panel ? panel.transform : (root ? root.transform : transform);
        if (!scope)
        {
            Debug.LogWarning("[ShopUI] RefreshAllSlots: scope가 없습니다.");
            return;
        }

        var slots = scope.GetComponentsInChildren<ShopItemSlot>(true);
        if (slots == null || slots.Length == 0)
        {
            Debug.Log("[ShopUI] RefreshAllSlots: ShopItemSlot 없음");
            return;
        }

        foreach (var s in slots)
        {
            if (s == null) continue;
            s.RefreshAvailability(); // ⚙️ 아이템 보유량 기준으로 색상/클릭 가능 상태 갱신
        }

        Debug.Log($"[ShopUI] RefreshAllSlots: {slots.Length}개 슬롯 상태 재평가 완료");
    }

    // 슬롯 생성/바인딩이 완료된 다음 프레임에도 한 번 더 갱신 (안정성 확보용)
    private System.Collections.IEnumerator RefreshAllSlotsNextFrame()
    {
        yield return null;
        RefreshAllSlots();
    }

    // ==========================
    // Exit 버튼, ESC키 둘 다 처리
    // ==========================
    private void Update()
    {
        // 1️⃣ ESC 키 눌렀을 때 닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (root && root.activeSelf)
                Close();
        }
    }

    // 2️⃣ Exit 버튼이 눌렸을 때 연결할 함수
    public void OnClickExitButton()
    {
        Close();
    }

}
