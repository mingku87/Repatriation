using System.Collections.Generic;
using UnityEngine;

public class ShopPanel : MonoBehaviour
{
    [Header("Data")]
    public ShopTradeDB tradeDB;          // 상점 거래 DB(에셋 참조) ※ 인스펙터에서 연결!
    [HideInInspector] public string shopId;

    [Header("Slots (scene children)")]
    public Transform gridParent;         // 슬롯들이 자식으로 달린 부모(없으면 자기 자신)
    [Range(1, 12)] public int maxSlots = 12;

    // 내부 슬롯 캐시
    private readonly List<ShopItemSlot> _slots = new();

    void Awake()
    {
        CacheSlotsFromScene();
        HideAll();
        Debug.Log($"[ShopPanel] Awake - cachedSlots={_slots.Count}, gridParent={(gridParent ? gridParent.name : "NULL")}");
    }

    void CacheSlotsFromScene()
    {
        _slots.Clear();
        if (gridParent == null) gridParent = transform;

        // ✅ 자식 전체(비활성 포함)를 재귀로 스캔
        var found = gridParent.GetComponentsInChildren<ShopItemSlot>(true);

        // 최대 maxSlots까지만 수집
        for (int i = 0; i < found.Length && _slots.Count < maxSlots; i++)
            if (found[i] != null) _slots.Add(found[i]);

        // 디버그: 어떤 슬롯을 잡았는지 출력
        var names = string.Join(", ", _slots.ConvertAll(s => s ? s.name : "null"));
        Debug.Log($"[ShopPanel] CacheSlotsFromScene - found={_slots.Count} (max={maxSlots}) | {names}");
    }

    void HideAll()
    {
        for (int i = 0; i < _slots.Count; i++)
            if (_slots[i]) _slots[i].gameObject.SetActive(false);

        Debug.Log("[ShopPanel] HideAll - 모든 슬롯 비활성화");
    }

    /// <summary>외부에서 shopId를 지정하고 즉시 갱신</summary>
    public void SetShopAndRefresh(string newShopId)
    {
        shopId = newShopId;
        Debug.Log($"[ShopPanel] SetShopAndRefresh - shopId={shopId}");
        Refresh();
    }

    /// <summary>DB에서 오퍼를 가져와 슬롯을 표시/숨김</summary>
    public void Refresh()
    {
        if (tradeDB == null)
        {
            Debug.LogWarning("[ShopPanel] tradeDB가 연결되지 않았습니다.", this);
            HideAll(); return;
        }

        // 🔧 캐시 비었거나 슬롯 오브젝트들이 전부 비활성인 경우 재캐시
        if (_slots.Count == 0) CacheSlotsFromScene();

        var offers = tradeDB.GetOffers(shopId);
        int offerCount = offers?.Count ?? 0;
        Debug.Log($"[ShopPanel] Refresh - shopId={shopId}, offers={offerCount}, slotsCached={_slots.Count}");

        int count = Mathf.Min(offerCount, _slots.Count);

        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (!slot) continue;

            bool on = i < count;
            slot.gameObject.SetActive(on);
            if (on)
            {
                slot.Bind(offers[i]);  // 아이콘/이름/가격/교환아이콘 채움
                                       // Debug.Log($"[ShopPanel] slot#{i} ON -> {slot.name}");
            }
            // else Debug.Log($"[ShopPanel] slot#{i} OFF -> {slot.name}");
        }

        if (count == 0)
            Debug.LogWarning($"[ShopPanel] 표시할 오퍼가 없습니다. shopId='{shopId}'");
    }
}
