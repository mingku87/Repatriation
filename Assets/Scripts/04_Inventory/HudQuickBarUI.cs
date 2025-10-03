// HudQuickBarUI.cs
using UnityEngine;

public class HudQuickBarUI : MonoBehaviour
{
    [SerializeField] InventorySlotUI[] quickSlots; // HUD�� 5ĭ(�������)

    Inventory inv;

    public void Bind(Inventory inventory)
    {
        inv = inventory;
        inv.OnChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (inv != null) inv.OnChanged -= Refresh;
    }

    public void Refresh()
    {
        if (inv == null || quickSlots == null) return;

        // �κ��丮 0..4ĭ�� �״�� �����ͼ� HUD�� ǥ��
        for (int i = 0; i < quickSlots.Length; i++)
        {
            if (i < inv.ActiveSlotCount)
                quickSlots[i].Set(inv.GetView(i)); // 0~4
            else
                quickSlots[i].Clear();
        }
    }
}
