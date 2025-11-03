using UnityEngine;

public class ShopUIController : MonoBehaviour
{
    static ShopUIController _cached;
    public static ShopUIController Get()
        => _cached ? _cached : (_cached = FindObjectOfType<ShopUIController>(true));

    [SerializeField] GameObject root;      // InGameUI/Shop
    [SerializeField] ShopPanel panel;      // ShopItemUI에 붙은 것
    [SerializeField] GameObject bg;        // BG
    [SerializeField] UnityEngine.UI.Image shopNpcImage;
    [SerializeField] TextBubbleAuto bubble;

    void Awake()
    {
        _cached = this;
        if (!root) root = gameObject;
        if (!panel && root) panel = root.GetComponentInChildren<ShopPanel>(true);
        if (root) root.SetActive(false);
    }

    public void OpenForShop(string shopId, string npcId, string bubbleKeyOrText = null, bool isLocKey = true)
    {
        if (!root || !panel) { Debug.LogWarning("[ShopUI] root/panel 미지정"); return; }

        panel.SetShopAndRefresh(shopId);
        if (bg) bg.SetActive(true);

        // NPC 이미지/버블(이전 답변 코드 그대로 사용)
        // ... (이미 연결했다면 그대로 동작)

        root.SetActive(true);
    }

    public void Close() { if (root) root.SetActive(false); }
}
