using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSlot : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private Image itemImage;       // ItemImage
    [SerializeField] private TMP_Text nameText;     // PriceBG/Nametag/Text (TMP)
    [SerializeField] private TMP_Text priceText;    // PriceBG/Pricetag/Text (TMP)
    [SerializeField] private Image sellItemImage;   // PriceBG/Pricetag/Image

    /// <summary>상점 오퍼 한 건으로 슬롯을 채움</summary>
    public void Bind(ShopTradeOffer offer)
    {
        if (offer == null) { Debug.LogWarning("[ShopItemSlot] offer null", this); return; }

        // 1) 메인 아이템 아이콘 / 이름
        if (itemImage)
            itemImage.sprite = ItemPresentationDB.GetIcon(offer.itemId);   // 아이콘 로드 (Resources 스프라이트 시트 지원)

        if (nameText)
        {
            // TSV에서 등록된 이름(혹은 로컬라이즈 키)을 꺼낸다
            var nameOrKey = ItemPresentationDB.GetName(offer.itemId);

            // LocalizationManager가 있으면 키로 조회하고, 없으면 그대로 사용
            var lm = LocalizationManager.Instance;
            nameText.text = lm ? lm.GetOrKey(nameOrKey) : nameOrKey;
        }

        // 2) 가격(숫자)
        if (priceText)
            priceText.text = offer.price.ToString();

        // 3) 교환(지불) 아이템 아이콘
        if (sellItemImage)
            sellItemImage.sprite = ItemPresentationDB.GetIcon(offer.sellItemId);
    }
}
