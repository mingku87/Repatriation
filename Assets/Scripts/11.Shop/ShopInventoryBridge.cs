using System.Collections;
using UnityEngine;

/// <summary>
/// 상점 UI와 인벤토리를 연결하는 브릿지.
/// - Inventory는 MonoBehaviour가 아니므로 FindObjectOfType로 못 찾음 → 전역핸들이나 InventoryController에서 참조
/// - GetCount / TryRemove / Add 델리게이트를 ShopUIController에 주입
/// - 인벤 변경 시(선택) 상점 슬롯 재평가(RefreshAllSlots)
/// </summary>
[DefaultExecutionOrder(200)]
public class ShopInventoryBridge : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ShopUIController shop;         // 씬의 ShopUIController (비워도 자동 탐색)
    [SerializeField] private InventoryController invCtrl;   // 선택: 있으면 여기서 Inventory를 받음

    // Inventory는 MonoBehaviour가 아님(씬 오브젝트 아님)
    private Inventory playerInventory;

    // ✅ 기존 코드 호환을 위해 "중첩 전역핸들" 유지 (InventoryController에서 ShopInventoryBridge.InventoryHandle.Active 로 접근 가능)
    public static class InventoryHandle
    {
        public static Inventory Active;
    }

    void Awake()
    {
        if (!shop) shop = FindObjectOfType<ShopUIController>(true);
        if (!invCtrl) invCtrl = FindObjectOfType<InventoryController>(true);
    }

    void Start()
    {
        // 전역 핸들이나 컨트롤러가 준비될 때까지 대기 후 주입
        StartCoroutine(TryBindRoutine());
    }

    IEnumerator TryBindRoutine()
    {
        int tries = 0;
        while (tries < 120)
        {
            if (!shop) shop = FindObjectOfType<ShopUIController>(true);

            // 1) 전역 핸들 우선
            if (playerInventory == null && InventoryHandle.Active != null)
                playerInventory = InventoryHandle.Active;

            // 2) 컨트롤러에서 꺼내기
            if (playerInventory == null && invCtrl != null)
                playerInventory = invCtrl.inventory;

            if (shop != null && playerInventory != null) break;

            tries++;
            yield return null;
        }

        if (shop == null)
        {
            Debug.LogError("[ShopInventoryBridge] ShopUIController 인스턴스를 찾지 못했습니다.");
            yield break;
        }
        if (playerInventory == null)
        {
            Debug.LogError("[ShopInventoryBridge] Inventory 인스턴스를 찾지 못했습니다. " +
                           "InventoryController.Initialize() 직후에 ShopInventoryBridge.InventoryHandle.Active = inventory; 를 한 번만 설정하거나, " +
                           "ShopInventoryBridge.Init(inv) 를 호출해 주세요.");
            yield break;
        }

        Init(playerInventory);
    }

    /// <summary>외부에서 명시적으로 인벤을 주입하고 싶을 때 호출</summary>
    public void Init(Inventory inv)
    {
        playerInventory = inv;
        if (playerInventory == null)
        {
            Debug.LogError("[ShopInventoryBridge] Init(inv=null)");
            return;
        }
        if (shop == null)
        {
            shop = FindObjectOfType<ShopUIController>(true);
            if (shop == null)
            {
                Debug.LogError("[ShopInventoryBridge] ShopUIController 인스턴스를 찾지 못했습니다.");
                return;
            }
        }

        // ✅ 델리게이트 주입
        shop.GetCount = GetItemCount;
        shop.TryRemove = TryRemoveById;
        shop.Add = AddById;

        // 인벤 변경 시(선택) 상점 슬롯 재평가
        playerInventory.OnChanged += () =>
        {
            var s = ShopUIController.Get();
            if (s != null) s.RefreshAllSlots();
        };

        Debug.Log("[ShopInventoryBridge] ✅ 인벤 델리게이트 주입 완료");
    }

    // ----------------- 델리게이트 구현 -----------------

    private int GetItemCount(int itemId)
    {
        if (playerInventory == null) return 0;

        int total = 0;
        int max = playerInventory.ActiveSlotCount;
        for (int i = 0; i < max; i++)
        {
            var st = playerInventory.slots[i];
            if (!st.IsEmpty && st.item != null && st.item.id == itemId)
                total += st.count;
        }
        return total;
    }

    private bool TryRemoveById(int itemId, int amount)
    {
        if (playerInventory == null) return false;
        if (amount <= 0) return true;

        int remain = amount;
        int max = playerInventory.ActiveSlotCount;

        for (int i = 0; i < max; i++)
        {
            ref var s = ref playerInventory.slots[i];
            if (s.IsEmpty || s.item == null || s.item.id != itemId) continue;

            int take = Mathf.Min(remain, s.count);
            s.count -= take;
            remain -= take;
            if (s.count <= 0) { s.item = null; s.count = 0; }

            if (remain <= 0)
            {
                playerInventory.RaiseChanged();
                return true;
            }
        }

        playerInventory.RaiseChanged(); // UI 갱신은 한 번
        return false;
    }

    private void AddById(int itemId, int count)
    {
        if (playerInventory == null || count <= 0) return;
        playerInventory.AddItemById(itemId, count); // 내부에서 Notify 호출됨
    }
}
