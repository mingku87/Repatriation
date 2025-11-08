using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopItemSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Refs")]
    [SerializeField] private Image itemImage;      // 아이템 아이콘
    [SerializeField] private TMP_Text nameText;    // 아이템 이름
    [SerializeField] private TMP_Text priceText;   // 가격 숫자
    [SerializeField] private Image sellItemImage;  // 지불(교환) 아이콘
    [SerializeField] private CanvasGroup cg;       // 회색/인터랙션 제어용(없으면 자동 추가)

    private ShopTradeOffer _offer;
    private ShopUIController _shop;

    // 외부에서 보유량 공급자를 주입하려면 사용(옵션)
    public System.Func<int, int> ownedCountProvider;

    void Awake()
    {
        if (!cg) cg = GetComponent<CanvasGroup>();
        _shop = ShopUIController.Get();
    }

    /// <summary>상점 오퍼 한 건으로 슬롯 채우기 + 가용 여부 갱신</summary>
    public void Bind(ShopTradeOffer offer)
    {
        _offer = offer;
        if (offer == null) { Debug.LogWarning("[ShopItemSlot] Bind - offer null", this); return; }

        // 1) 메인 아이콘/이름
        if (itemImage) itemImage.sprite = ItemPresentationDB.GetIcon(offer.itemId);
        else Debug.LogWarning("[ShopItemSlot] itemImage 참조 누락", this);

        if (nameText)
        {
            var nameOrKey = ItemPresentationDB.GetName(offer.itemId);
            var lm = LocalizationManager.Instance;
            nameText.text = lm ? lm.GetOrKey(nameOrKey) : nameOrKey;
        }
        else Debug.LogWarning("[ShopItemSlot] nameText 참조 누락", this);

        // 2) 가격(숫자)
        if (priceText) priceText.text = offer.price.ToString();
        else Debug.LogWarning("[ShopItemSlot] priceText 참조 누락", this);

        // 3) 지불 아이콘
        if (sellItemImage) sellItemImage.sprite = ItemPresentationDB.GetIcon(offer.sellItemId);
        else Debug.LogWarning("[ShopItemSlot] sellItemImage 참조 누락", this);

        // 4) 가용/비가용 상태 반영
        RefreshAvailability();
    }

    public void RefreshAvailability()
    {

        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        // 보유량 조회: 우선 슬롯 제공자 → 없으면 Shop의 델리게이트 사용
        int owned = 0;
        if (ownedCountProvider != null) owned = Mathf.Max(0, ownedCountProvider.Invoke(_offer.sellItemId));
        else if (ShopUIController.Get() && ShopUIController.Get().GetCount != null)
            owned = Mathf.Max(0, ShopUIController.Get().GetCount.Invoke(_offer.sellItemId));

        int maxAmount = (_offer.price <= 0) ? int.MaxValue : (owned / Mathf.Max(1, _offer.price));
        bool canBuy = maxAmount > 0;

        if (!canBuy)
        {
            // 회색(#828282) + 클릭 불가
            Color32 gray = new Color32(0x82, 0x82, 0x82, 0xFF);
            if (itemImage) itemImage.color = gray;
            if (sellItemImage) sellItemImage.color = gray;
            if (nameText) nameText.color = gray;
            if (priceText) priceText.color = gray;

            cg.alpha = 1f; cg.interactable = false; cg.blocksRaycasts = false;
        }
        else
        {
            // 정상 색상/클릭 가능
            if (itemImage) itemImage.color = Color.white;
            if (sellItemImage) sellItemImage.color = Color.white;
            if (nameText) nameText.color = Color.white;
            if (priceText) priceText.color = Color.white;

            cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true;
        }
    }

    // 클릭 → Shop에 구매 프롬프트 요청
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_offer == null) return;
        if (_shop == null) _shop = ShopUIController.Get();
        if (_shop == null) { Debug.LogWarning("[ShopItemSlot] ShopUIController 없음"); return; }

        // 가용 상태가 아니면 무시
        if (cg && (!cg.interactable || !cg.blocksRaycasts)) return;

        _shop.PromptExchange(_offer, titleLocKey: "exchangetitle"); // 타이틀은 로컬라이즈 키 사용
    }
}
