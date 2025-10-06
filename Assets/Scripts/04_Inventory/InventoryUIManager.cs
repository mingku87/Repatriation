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
            inventorySlots[i].Clear(); // ��Ŭ����
        }

        if (_inv != null)
        {
            _inv.OnChanged += RefreshInventory;
            RefreshInventory();
        }
    }
        void OnEnable()
    {
        // ���� �ε��� ����
        if (inventorySlots != null)
            for (int i = 0; i < inventorySlots.Length; i++)
                if (inventorySlots[i]) inventorySlots[i].index = i;

        // �̹� ���ε��� �ִٸ� ��� ��������
        if (_inv != null) RefreshInventory();
        else
        {
            // Ȥ�� ��Ʈ�ѷ��� ���� �ʱ�ȭ�� ��� �ڵ� ���ε� �õ� (������ ����)
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
