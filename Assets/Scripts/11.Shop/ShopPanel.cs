using System.Collections.Generic;
using UnityEngine;

public class ShopPanel : MonoBehaviour
{
    [Header("Data")]
    public ShopTradeDB tradeDB;          // ShopTradeDB ����
    [HideInInspector] public string shopId;

    [Header("Slots (scene children)")]
    public Transform gridParent;         // ShopItemUI (���Ե��� �ڽ����� �ִ� �θ�)
    [Range(1, 12)] public int maxSlots = 12;

    // ĳ��
    private readonly List<ShopItemSlot> _slots = new();

    void Awake()
    {
        CacheSlotsFromScene();
        HideAll();
    }

    void CacheSlotsFromScene()
    {
        _slots.Clear();
        if (gridParent == null) gridParent = transform; // �ڽ��� �θ�� ����ص� OK

        // �θ� �Ʒ� ShopItemSlot�� ������� ���� (�ִ� 12��)
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
            Debug.LogWarning("[ShopPanel] tradeDB �Ҵ� �ʿ�", this);
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
                slot.Bind(offers[i]); // ������/�̸�/����/��ȯ������ ������ ä��
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }
    }
}
