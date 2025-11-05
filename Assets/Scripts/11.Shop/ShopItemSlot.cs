using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSlot : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private Image itemImage;     // 아이템 아이콘
    [SerializeField] private TMP_Text nameText;   // 아이템 이름
    [SerializeField] private TMP_Text priceText;  // 가격 숫자
    [SerializeField] private Image sellItemImage; // 지불(교환) 아이템 아이콘

    /// <summary>상점 오퍼 한 건으로 슬롯을 채움</summary>
    public void Bind(ShopTradeOffer offer)
    {
        if (offer == null) { Debug.LogWarning("[ShopItemSlot] Bind - offer null", this); return; }

        Debug.Log($"[ShopItemSlot] Bind - itemId={offer.itemId}, sellItemId={offer.sellItemId}, price={offer.price}", this);

        // 1) 메인 아이템 아이콘 / 이름
        if (itemImage)
            itemImage.sprite = ItemPresentationDB.GetIcon(offer.itemId);
        else
            Debug.LogWarning("[ShopItemSlot] itemImage 참조 누락", this);

        if (nameText)
        {
            var nameOrKey = ItemPresentationDB.GetName(offer.itemId);
            var lm = LocalizationManager.Instance;
            nameText.text = lm ? lm.GetOrKey(nameOrKey) : nameOrKey;
        }
        else
        {
            Debug.LogWarning("[ShopItemSlot] nameText 참조 누락", this);
        }

        // 2) 가격(숫자)
        if (priceText)
            priceText.text = offer.price.ToString();
        else
            Debug.LogWarning("[ShopItemSlot] priceText 참조 누락", this);

        // 3) 교환(지불) 아이템 아이콘
        if (sellItemImage)
            sellItemImage.sprite = ItemPresentationDB.GetIcon(offer.sellItemId);
        else
            Debug.LogWarning("[ShopItemSlot] sellItemImage 참조 누락", this);
    }
}
