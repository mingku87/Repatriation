using System;
using UnityEngine;

/// <summary>
/// 인벤토리 본체(Partial)
/// - 슬롯/스택 관리
/// - 활성 슬롯 수(기본 해금 + 장비 보너스) 관리
/// - 아이템 추가/제거/이동/스왑/지정칸 배치
/// - UI 갱신을 위한 OnChanged 이벤트 발행
/// </summary>
public partial class Inventory
{
    // ======================================================================
    //  Constants & Fields
    // ======================================================================

    /// <summary>시각적 최대 슬롯 수(인덱스 상한)</summary>
    public const int HARD_MAX_SLOTS = 25;

    /// <summary>현재 사용 가능한 슬롯 수(= 기본 해금 + 장비 보너스)</summary>
    public int ActiveSlotCount { get; private set; }

    /// <summary>기본 해금 슬롯 수(컨트롤러에서 초기 세팅)</summary>
    private int _baseUnlocked = 5;

    /// <summary>장비(가방)로 증가하는 보너스 슬롯 수</summary>
    private int _equipBonusSlots = 0;

    /// <summary>UI 갱신용 이벤트</summary>
    public event Action OnChanged;

    /// <summary>인벤토리 슬롯 배열(고정 크기)</summary>
    public readonly ItemStack[] slots = new ItemStack[HARD_MAX_SLOTS];

    public void RaiseChanged() => OnChanged?.Invoke();

    // ======================================================================
    //  ItemStack & SlotView
    // ======================================================================

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

        /// <summary>장비이며 내구도가 소모된 경우에만 게이지 표시</summary>
        public bool NeedDurabilityGauge
            => IsEquipment && MaxDurability > 0 && CurDurability < MaxDurability;
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

    // ======================================================================
    //  Lifecycle & Utilities
    // ======================================================================

    /// <summary>외부(UI 등)에 변경을 알림</summary>
    void Notify() => OnChanged?.Invoke();

    /// <summary>
    /// 슬롯 초기화. 기존 Initialize에서 호출해도 무방.
    /// </summary>
    public void InitSlots(int activeSlotCount = HARD_MAX_SLOTS)
    {
        ActiveSlotCount = Mathf.Clamp(activeSlotCount, 1, HARD_MAX_SLOTS);
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].item = null;
            slots[i].count = 0;
        }
        Notify();
    }

    /// <summary>현재 활성 슬롯 범위 내 인덱스인지 확인</summary>
    public bool IsValidIndex(int i) => i >= 0 && i < ActiveSlotCount;

    /// <summary>활성 슬롯 범위에서 비어있는 첫 칸의 인덱스. 없으면 -1</summary>
    int FindEmpty()
    {
        for (int i = 0; i < ActiveSlotCount; i++)
            if (slots[i].IsEmpty) return i;
        return -1;
    }

    /// <summary>같은 ID이고 스택 가능하며 여유가 있는 칸의 인덱스. 없으면 -1</summary>
    int FindStackableIndex(int id)
    {
        for (int i = 0; i < ActiveSlotCount; i++)
        {
            var s = slots[i];
            if (!s.IsEmpty && s.item.id == id && s.MaxStack > s.count)
                return i;
        }
        return -1;
    }

    // ======================================================================
    //  Active Slot Count (Base + Equipment-Bonus)
    // ======================================================================

    /// <summary>초기 해금 슬롯 수 설정(컨트롤러 Initialize에서 1회 호출 권장)</summary>
    public void SetBaseUnlocked(int count)
    {
        _baseUnlocked = Mathf.Clamp(count, 0, HARD_MAX_SLOTS);
        RecomputeActiveSlotCount();
    }

    /// <summary>장비(가방)의 슬롯 보너스 설정(EquipmentModel에서 호출)</summary>
    public void SetEquipmentBonusSlots(int bonus)
    {
        _equipBonusSlots = Mathf.Max(0, bonus);
        RecomputeActiveSlotCount();
    }

    /// <summary>활성 슬롯 수 재계산 후 변경 시 UI 알림</summary>
    private void RecomputeActiveSlotCount()
    {
        int next = Mathf.Clamp(_baseUnlocked + _equipBonusSlots, 0, HARD_MAX_SLOTS);
        if (ActiveSlotCount != next)
        {
            ActiveSlotCount = next;
            OnChanged?.Invoke();
        }
    }

    /// <summary>
    /// 활성 슬롯 수를 직접 설정(가방 해제 등 ↓로 줄이는 경우 넘치는 칸이 비어 있어야 함)
    /// </summary>
    public bool TrySetActiveSlotCount(int newCount)
    {
        newCount = Mathf.Clamp(newCount, 1, HARD_MAX_SLOTS);

        // 늘리는 건 언제나 허용
        if (newCount >= ActiveSlotCount)
        {
            ActiveSlotCount = newCount;
            Notify();
            return true;
        }

        // 줄이는 경우 잘리는 구간에 아이템이 있으면 거부
        for (int i = newCount; i < ActiveSlotCount; i++)
            if (!slots[i].IsEmpty) return false;

        ActiveSlotCount = newCount;
        Notify();
        return true;
    }

    // ======================================================================
    //  Item Put / Remove / Move / Swap
    // ======================================================================

    /// <summary>
    /// 지정 칸이 비어 있으면 외부에서 전달된 아이템 1개를 그 칸에 꽂는다(스왑 시 사용).
    /// </summary>
    public bool TryPlaceItemAtEmpty(int index, Item item)
    {
        if (!IsValidIndex(index)) return false;
        var s = slots[index];
        if (!s.IsEmpty) return false;

        slots[index] = new ItemStack { item = item, count = 1 };
        OnChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// TSV(DB)에서 id로 파라미터를 찾고, 스택 규칙을 적용하여 아이템을 추가한다.
    /// 반환값은 남은 개수(0이면 전부 들어간 것).
    /// </summary>
    public int AddItemById(int id, int count = 1)
    {
        if (count <= 0) return 0;

        // TSV에서 아이템 데이터 조회
        var param = ItemParameterList.GetItemStat(id);
        if (param == null) return count; // 정의가 없으면 실패

        // (1) 기존 스택 채우기
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

        // (2) 빈칸에 새 스택 생성
        while (count > 0)
        {
            int empty = FindEmpty();
            if (empty < 0) break;

            var newItem = ItemFactory.Create(id);
            int put = (param.maxstack > 1) ? Mathf.Min(param.maxstack, count) : 1;

            slots[empty].item = newItem;
            slots[empty].count = put;
            count -= put;
        }

        Notify();
        return count;
    }

    /// <summary>지정 칸에서 amount만큼 제거하고 실제 제거한 개수를 반환</summary>
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

    /// <summary>
    /// from → to 이동 또는 스왑/스택 병합.
    /// - 같은 아이템이며 스택 가능: to에 병합
    /// - 그 외: 두 칸 스왑
    /// </summary>
    public void MoveOrSwap(int from, int to)
    {
        if (!IsValidIndex(from) || !IsValidIndex(to) || from == to) return;

        ref var A = ref slots[from];
        ref var B = ref slots[to];
        if (A.IsEmpty) return;

        // 스택 병합
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

    // ======================================================================
    //  Queries for UI
    // ======================================================================

    /// <summary>오직 인벤토리 슬롯(장비창 제외)의 총 무게</summary>
    public float GetInventoryWeightOnly()
    {
        float sum = 0f;
        for (int i = 0; i < ActiveSlotCount; i++)
            if (!slots[i].IsEmpty) sum += slots[i].TotalWeight;
        return sum;
    }

    /// <summary>UI에서 사용하기 좋은 뷰 데이터</summary>
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

    // ======================================================================
    //  External Placement API
    // ======================================================================

    /// <summary>
    /// 외부(장비창 등)에서 넘어온 아이템을 인벤토리 'index' 칸에 배치.
    /// - 인덱스 유효/미잠금이어야 함
    /// - 빈칸이면 그대로 배치
    /// - 같은 ID이며 스택 가능하면 합침
    /// - 다른 아이템이 있으면 실패(스왑은 별도 정책에서 처리)
    /// </summary>
    public bool PlaceExistingItemAt(int index, Item item, int count, out string reason)
    {
        reason = null;

        if (!IsValidIndex(index))
        {
            reason = "유효하지 않은 인덱스입니다.";
            return false;
        }

        ref var dst = ref slots[index];

        // 빈칸 → 그대로 배치
        if (dst.IsEmpty)
        {
            dst.item = item;
            dst.count = Mathf.Max(1, count);
            Notify();
            return true;
        }

        // 같은 아이템 & 스택 가능 → 합치기
        if (dst.item != null && dst.item.id == item.id)
        {
            int max = dst.MaxStack;
            if (max > 1)
            {
                int space = max - dst.count;
                if (space <= 0) { reason = "스택이 가득 찼습니다."; return false; }

                int put = Mathf.Min(space, count);
                dst.count += put;
                Notify();
                return true;
            }
            else
            {
                reason = "해당 칸은 이미 같은 장비가 있습니다.";
                return false;
            }
        }

        // 다른 아이템이 있으면 실패
        reason = "해당 칸이 비어있지 않습니다.";
        return false;
    }
    public Item PeekAt(int i) => IsValidIndex(i) && !slots[i].IsEmpty ? slots[i].item : null;

    public bool TryFindByGuid(Guid guid, out int index, out Item item)
    {
        index = -1; item = null;
        for (int i = 0; i < ActiveSlotCount; i++)
        {
            var s = slots[i];
            if (!s.IsEmpty && s.item != null && s.item.guid == guid)
            {
                index = i; item = s.item;
                return true;
            }
        }
        return false;
    }
    public Item TakeAt(int i)
    {
        if (!IsValidIndex(i)) return null;
        var s = slots[i];
        if (s.IsEmpty) return null;
        slots[i] = new ItemStack(); // reset
        OnChanged?.Invoke();
        return s.item;
    }

    public void SetAt(int i, Item item)
    {
        if (!IsValidIndex(i)) return;
        slots[i] = new ItemStack { item = item, count = 1 };
        OnChanged?.Invoke();
    }

    public bool Add(Item item)
    {
        if (item == null) return false;
        int empty = -1;
        for (int i = 0; i < ActiveSlotCount; i++)
            if (slots[i].IsEmpty) { empty = i; break; }
        if (empty < 0) return false;
        slots[empty] = new ItemStack { item = item, count = 1 };
        OnChanged?.Invoke();
        return true;
    }

    public void SubtractAt(int index, int amount)
    {
        if (!IsValidIndex(index)) return;
        slots[index].count -= amount;
        if (slots[index].count <= 0)
        {
            slots[index].item = null;
            slots[index].count = 0;
        }
    }
}