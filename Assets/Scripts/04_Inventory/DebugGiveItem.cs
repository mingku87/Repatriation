using UnityEngine;

public class DebugGiveItem : MonoBehaviour
{
    [Header("획득할 아이템")]
    [Tooltip("TSV의 item_id")]
    [SerializeField] private int itemId = 10001;

    [Tooltip("획득 수량 (스택 적용)")]
    [SerializeField] private int count = 1;

    [Tooltip("장비 내구도. 음수면 TSV quality(최대 내구도)를 사용")] 
    [SerializeField] private int durability = -1;

    [Header("선택 사항")]
    [Tooltip("InventoryController가 아직 초기화되지 않았으면 자동 초기화 시도")]
    [SerializeField] private bool autoInitController = true;

    void Awake()
    {
        if (autoInitController && InventoryController.Instance == null)
        {
            // 씬 어디에도 없을 수 있으니 안전하게 생성해서 Initialize 호출
            var go = new GameObject("InventorySystem(Auto)");
            var ctrl = go.AddComponent<InventoryController>();
            ctrl.Initialize();
        }
    }

    /// <summary>버튼에서 호출: 현재 설정된 itemId / count로 아이템 지급</summary>
    public void Give()
    {
        var ctrl = InventoryController.Instance;
        if (ctrl == null) { Debug.LogError("[DebugGiveItem] InventoryController not found."); return; }

        if (autoInitController && ctrl.inventory == null)
            ctrl.Initialize();

        var inv = ctrl.inventory;
        if (inv == null) { Debug.LogError("[DebugGiveItem] inventory is null."); return; }

        int resolvedDurability = ResolveDurability(itemId, durability);

        int remain = inv.AddItemById(itemId, Mathf.Max(1, count), resolvedDurability);  // 인벤토리에 호출
        if (remain > 0)
            Debug.LogWarning($"{remain}개는 공간 부족/정의 없음으로 미지급");
        else
        {
            if (resolvedDurability >= 0)
                Debug.Log($"지급 완료: id={itemId}, count={count}, durability={resolvedDurability}");
            else
                Debug.Log($"지급 완료: id={itemId}, count={count}");
        }
    }

    /// <summary>버튼에서 호출: 원하는 ID/개수를 파라미터로 지급(유니티 이벤트에서 쓸 수 있음)</summary>
    public void GiveWithArgs(int id, int amount, int durabilityOverride = -1)
    {
        var ctrl = InventoryController.Instance;
        if (ctrl == null)
        {
            Debug.LogError("[DebugGiveItem] InventoryController.Instance == null");
            return;
        }

        int resolvedDurability = ResolveDurability(id, durabilityOverride);
        ctrl.inventory.AddItemById(id, Mathf.Max(1, amount), resolvedDurability);
    }

    private static int ResolveDurability(int id, int requested)
    {
        if (requested >= 0)
            return requested;

        var param = ItemParameterList.GetItemStat(id) as ItemParameterEquipment;
        if (param != null)
            return param.maxDurability;

        return -1;
    }
}
