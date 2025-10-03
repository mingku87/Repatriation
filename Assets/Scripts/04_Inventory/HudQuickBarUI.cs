// HudQuickBarUI.cs
using UnityEngine;

public class HudQuickBarUI : MonoBehaviour
{
    [SerializeField] InventorySlotUI[] quickSlots; // HUD의 5칸(순서대로)

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

        // 인벤토리 0..4칸을 그대로 가져와서 HUD에 표시
        for (int i = 0; i < quickSlots.Length; i++)
        {
            if (i < inv.ActiveSlotCount)
                quickSlots[i].Set(inv.GetView(i)); // 0~4
            else
                quickSlots[i].Clear();
        }
    }
}
