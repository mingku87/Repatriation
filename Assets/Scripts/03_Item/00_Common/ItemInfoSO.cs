using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/ItemInfo")]
public class ItemInfoSO : SingletonScriptableObject<ItemInfoSO>
{
    [SerializeField] List<ItemInfo> itemInfo;
    public static List<ItemInfo> itemInfos;

    void Awake()
    {
        itemInfos = new();
        foreach (var info in itemInfo) itemInfos.Add(info);
    }

    public static ItemInfo GetItemInfo(ItemName itemName)
    {
        foreach (var itemInfo in itemInfos)
            if (itemInfo.itemName == itemName) return itemInfo;
        return null;
    }
}