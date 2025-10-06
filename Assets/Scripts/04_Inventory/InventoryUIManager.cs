using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [SerializeField] InventorySlotUI[] inventorySlots;

    private Inventory _inv;

    public void Bind(Inventory inv)
    {
        if (_inv != null) _inv.OnChanged -= RefreshInventory;
        _inv = inv;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (!inventorySlots[i]) continue;
            inventorySlots[i].index = i;
            inventorySlots[i].Clear(); // 선클리어
        }

        if (_inv != null)
        {
            _inv.OnChanged += RefreshInventory;
            RefreshInventory();
        }
    }
        void OnEnable()
    {
        // 슬롯 인덱스 보장
        if (inventorySlots != null)
            for (int i = 0; i < inventorySlots.Length; i++)
                if (inventorySlots[i]) inventorySlots[i].index = i;

        // 이미 바인딩돼 있다면 즉시 리프레시
        if (_inv != null) RefreshInventory();
        else
        {
            // 혹시 컨트롤러가 먼저 초기화된 경우 자동 바인딩 시도 (없으면 무시)
            var ctrl = InventoryController.Instance;
            if (ctrl && ctrl.inventory != null) Bind(ctrl.inventory);
        }
    }

    public void RefreshInventory()
    {
        if (_inv == null) { foreach (var s in inventorySlots) s?.Clear(); return; }

        for (int i = 0; i < inventorySlots.Length; i++)
            inventorySlots[i]?.Set(_inv.GetView(i));
    }
}
