using System;
using UnityEngine;

public partial class Inventory
{
    // ───────── 슬롯/스택 구조 ─────────
    public const int HARD_MAX_SLOTS = 25;        // 시각적 최대
    public int ActiveSlotCount { get; private set; } = HARD_MAX_SLOTS;

    [Serializable]
    public struct ItemStack
    {
        public Item item;
        public int count;

        public bool IsEmpty => item == null || count <= 0;
        public int MaxStack => item?.param?.maxstack ?? 1;

        public float WeightPerItem => item?.param?.weight ?? 0f;
        public float TotalWeight => WeightPerItem * Mathf.Max(0, count);

        public Sprite Icon => item?.info?.image;
        public string DisplayName => item?.info?.name ?? item?.param?.itemName.ToString();

        public bool IsEquipment => item is ItemEquipment;
        public int CurDurability => (item as ItemEquipment)?.durability ?? 0;
        public int MaxDurability => (item as ItemEquipment)?.param?.maxDurability ?? 0;

        public bool NeedDurabilityGauge
            => IsEquipment && MaxDurability > 0 && CurDurability < MaxDurability;
    }

    // 슬롯 배열
    public readonly ItemStack[] slots = new ItemStack[HARD_MAX_SLOTS];

    // UI가 구독할 이벤트
    public event Action OnChanged;

    void Notify() => OnChanged?.Invoke();

    // 초기화 (기존 Initialize에서 호출해도 됨)
    public void InitSlots(int activeSlotCount = HARD_MAX_SLOTS)
    {
        ActiveSlotCount = Mathf.Clamp(activeSlotCount, 1, HARD_MAX_SLOTS);
        for (int i = 0; i < slots.Length; i++) { slots[i].item = null; slots[i].count = 0; }
        Notify();
    }

    public bool IsValidIndex(int i) => i >= 0 && i < ActiveSlotCount;

    int FindEmpty()
    {
        for (int i = 0; i < ActiveSlotCount; i++)
            if (slots[i].IsEmpty) return i;
        return -1;
    }

    int FindStackableIndex(int id)
    {
        for (int i = 0; i < ActiveSlotCount; i++)
        {
            var s = slots[i];
            if (!s.IsEmpty && s.item.id == id && s.MaxStack > s.count) return i;
        }
        return -1;
    }

    // id로 아이템 추가 (스택 규칙 적용). 남은 수량 반환
    public int AddItemById(int id, int count = 1)
    {
        if (count <= 0) return 0;

        // TSV에서 아이템 데이터 조회
        var param = ItemParameterList.GetItemStat(id);
        if (param == null) return count; // 정의가 없으면 실패

        // 1) 스택 채우기
        if (param.maxstack > 1)
        {
            while (count > 0)
            {
                int si = FindStackableIndex(id);
                if (si < 0) break;

                int space = slots[si].MaxStack - slots[si].count;
                int put = Mathf.Min(space, count);
                slots[si].count += put;
                count -= put;
            }
        }

        // 2) 빈칸에 배치
        while (count > 0)
        {
            int empty = FindEmpty();
            if (empty < 0) break;

            var newItem = ItemFactory.Create(id); // ← 실제 아이템 생성
            int put = (param.maxstack > 1) ? Mathf.Min(param.maxstack, count) : 1;

            slots[empty].item = newItem;
            slots[empty].count = put;
            count -= put;
        }

        Notify(); // UI 업데이트 이벤트
        return count; // 남은 수량 (0이 아니면 공간 부족)
    }

    public int RemoveAt(int index, int amount)
    {
        if (!IsValidIndex(index) || amount <= 0) return 0;
        ref var s = ref slots[index];
        if (s.IsEmpty) return 0;

        int removed = Mathf.Min(amount, s.count);
        s.count -= removed;
        if (s.count <= 0) { s.item = null; s.count = 0; }
        Notify();
        return removed;
    }

    public void MoveOrSwap(int from, int to)
    {
        if (!IsValidIndex(from) || !IsValidIndex(to) || from == to) return;
        ref var A = ref slots[from];
        ref var B = ref slots[to];
        if (A.IsEmpty) return;

        // 스택 합치기
        if (!B.IsEmpty && A.item.id == B.item.id && A.MaxStack > 1)
        {
            int space = B.MaxStack - B.count;
            int mv = Mathf.Min(space, A.count);
            B.count += mv;
            A.count -= mv;
            if (A.count <= 0) { A.item = null; A.count = 0; }
        }
        else
        {
            (B, A) = (A, B); // 스왑
        }
        Notify();
    }

    // 가방 착용/해제로 활성 슬롯 수 변경 (줄어드는 경우 넘치는 칸에 아이템 있으면 거부)
    public bool TrySetActiveSlotCount(int newCount)
    {
        newCount = Mathf.Clamp(newCount, 1, HARD_MAX_SLOTS);
        if (newCount >= ActiveSlotCount) { ActiveSlotCount = newCount; Notify(); return true; }

        for (int i = newCount; i < ActiveSlotCount; i++)
            if (!slots[i].IsEmpty) return false;

        ActiveSlotCount = newCount;
        Notify();
        return true;
    }

    // 인벤토리 총 무게 (오직 인벤토리 슬롯만) — 장비창 무게는 포함하지 않음
    public float GetInventoryWeightOnly()
    {
        float sum = 0f;
        for (int i = 0; i < ActiveSlotCount; i++)
            if (!slots[i].IsEmpty) sum += slots[i].TotalWeight;
        return sum;
    }

    // UI에서 쓰기 좋은 뷰 데이터
    public SlotView GetView(int index)
    {
        if (!IsValidIndex(index)) return default;
        var s = slots[index];
        var v = new SlotView
        {
            icon = s.Icon,
            displayName = s.DisplayName,
            count = s.count,
            showCount = s.MaxStack > 1 && s.count >= 2,
            showDurability = s.NeedDurabilityGauge,
            durability01 = 0f
        };
        if (s.NeedDurabilityGauge)
            v.durability01 = Mathf.Clamp01((float)s.CurDurability / Mathf.Max(1, s.MaxDurability));
        return v;
    }

    [Serializable]
    public struct SlotView
    {
        public Sprite icon;
        public string displayName;
        public int count;
        public bool showCount;
        public bool showDurability;
        public float durability01; // 0..1
    }
}
