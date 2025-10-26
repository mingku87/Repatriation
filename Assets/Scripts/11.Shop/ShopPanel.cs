using System.Collections.Generic;
using UnityEngine;

public class ShopPanel : MonoBehaviour
{
    [Header("Data")]
    public ShopTradeDB tradeDB;          // ShopTradeDB 에셋
    [HideInInspector] public string shopId;

    [Header("Slots (scene children)")]
    public Transform gridParent;         // ShopItemUI (슬롯들이 자식으로 있는 부모)
    [Range(1, 12)] public int maxSlots = 12;

    // 캐시
    private readonly List<ShopItemSlot> _slots = new();

    void Awake()
    {
        CacheSlotsFromScene();
        HideAll();
    }

    void CacheSlotsFromScene()
    {
        _slots.Clear();
        if (gridParent == null) gridParent = transform; // 자신을 부모로 사용해도 OK

        // 부모 아래 ShopItemSlot을 순서대로 수집 (최대 12개)
        for (int i = 0; i < gridParent.childCount && _slots.Count < maxSlots; i++)
        {
            var s = gridParent.GetChild(i).GetComponent<ShopItemSlot>();
            if (s != null) _slots.Add(s);
        }
    }

    void HideAll()
    {
        for (int i = 0; i < _slots.Count; i++)
            if (_slots[i]) _slots[i].gameObject.SetActive(false);
    }

    public void SetShopAndRefresh(string newShopId)
    {
        shopId = newShopId;
        Refresh();
    }

    public void Refresh()
    {
        if (tradeDB == null)
        {
            Debug.LogWarning("[ShopPanel] tradeDB 할당 필요", this);
            HideAll(); return;
        }

        var offers = tradeDB.GetOffers(shopId);
        int count = Mathf.Min(offers?.Count ?? 0, _slots.Count);

        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (slot == null) continue;

            if (i < count)
            {
                slot.gameObject.SetActive(true);
                slot.Bind(offers[i]); // 아이콘/이름/가격/교환아이템 아이콘 채움
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }
    }
}
