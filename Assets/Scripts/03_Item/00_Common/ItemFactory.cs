using UnityEngine;

/// <summary>
/// 아이템 생성 팩토리 (TSV 기반)
/// </summary>
public static class ItemFactory
{
    /// <summary>
    /// 아이템 생성
    /// value = durability override (장비일 때). -1이면 랜덤 초기화
    /// </summary>
    public static Item Create(int id, int value = -1)
    {
        var param = ItemParameterList.GetItemStat(id);
        if (param == null)
        {
            Debug.LogError($"[ItemFactory] Unknown id {id}");
            return null;
        }

        Item item;
        switch (param.type)
        {
            case ItemType.Equipment:
                {
                    var pe = param as ItemParameterEquipment;
                    var eq = new ItemEquipment(id);
                    if (value < 0 && pe.maxDurability > 0)
                        eq.durability = Random.Range(1, pe.maxDurability + 1);
                    else
                        eq.durability = Mathf.Clamp(value, 0, pe.maxDurability);
                    item = eq;
                    break;
                }

            case ItemType.Consumable:
                {
                    if (param is ItemParameterWater)
                        item = new ItemConsumableWater(id);
                    else
                        item = new ItemConsumable(id);
                    break;
                }

            default: // Etc
                {
                    item = new Item(id);
                    break;
                }
        }

        // 표시 정보 주입
        var pres = ItemPresentationDB.Get(id);
        if (pres != null)
            item.info = new ItemInfo { name = pres.name, description = pres.description, image = pres.icon };

        return item;
    }
}
