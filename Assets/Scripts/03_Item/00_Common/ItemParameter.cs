// ItemParameter.cs
// ─────────────────────────────────────────────────────────────
// TSV 기반 아이템 "파라미터(정의 데이터)" 모음.
// * 여기서는 이미 프로젝트에 존재하는 Status / ItemType / EquipmentPart / ItemName 을 그대로 사용합니다.
// * 실제 게임 오브젝트(Item 파생들)는 ItemFactory에서 이 파라미터를 참고해 생성합니다.
// ─────────────────────────────────────────────────────────────

using System;

/// <summary>
/// 공통 아이템 파라미터(모든 타입이 공유)
/// </summary>
public class ItemParameter
{
    public int id;
    public ItemName itemName;      // 기존 정의 사용
    public ItemType type;          // 기존 정의 사용
    public float weight;
    public int maxstack;
    public int buyPrice;
    public int sellPrice;
    public string description;
    public string displayNameKr;   // TSV에 적힌 한글 표시 이름(에디터 뷰어용)
    public ItemParameterEquipment equipment;
    public ItemParameter(int id, ItemName name, float weight)
    {
        this.id = id;
        this.itemName = name;
        this.weight = weight;
    }
}

/// <summary>
/// 장비 파라미터
/// - 착용/휴대 중 지속효과 적용
/// - 가방은 slotBonus로 인벤토리 슬롯 증가
/// - 보조효과(extraEffect) 1개까지
/// </summary>
public class ItemParameterEquipment : ItemParameter
{
    public EquipmentPart equipPart;    // 기존 정의 사용
    public int maxDurability;
    public int durabilityDecayRate;

    // 주 효과
    public Status status;              // 기존 정의 사용
    public float value;

    // 가방 슬롯 보너스 (effect_type 이 slot일 때 누적)
    public int slotBonus;

    // 보조 효과(있을 경우)
    public (Status status, float value)? extraEffect;

    public ItemParameterEquipment(
        int id,
        ItemName name,
        float weight,
        EquipmentPart part,
        Status status,
        float value,
        int maxDurability,
        int decay
    ) : base(id, name, weight)
    {
        this.type = ItemType.Equipment;
        this.equipPart = part;
        this.status = status;
        this.value = value;
        this.maxDurability = maxDurability;
        this.durabilityDecayRate = decay;
    }
}

/// <summary>
/// 소모품 세부유형 (없으면 삭제해도 됨)
/// </summary>
public enum ConsumableDetail
{
    Food,
    Water,
    Drug
}

/// <summary>
/// 소모품 파라미터
/// - 사용 시 effects 배열을 전부 적용(최대 2개 가정)
/// </summary>
public class ItemParameterConsumable : ItemParameter
{
    public ConsumableDetail detail;
    public (Status status, float value)[] effects;

    public ItemParameterConsumable(
        int id,
        ItemName name,
        float weight,
        ConsumableDetail detail,
        (Status, float)[] effects
    ) : base(id, name, weight)
    {
        this.type = ItemType.Consumable;
        this.detail = detail;
        this.effects = effects;
    }
}

/// <summary>
/// 물(워터) 소모품 파라미터
/// - quality(수질) 추가
/// </summary>
public class ItemParameterWater : ItemParameterConsumable
{
    public int quality;

    public ItemParameterWater(
        int id,
        ItemName name,
        float weight,
        (Status, float)[] effects,
        int quality
    ) : base(id, name, weight, ConsumableDetail.Water, effects)
    {
        this.quality = quality;
    }
}
