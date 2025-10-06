using UnityEngine;

public class HudQuickBarUI : MonoBehaviour
{
    [SerializeField] InventorySlotUI[] quickSlots; // 0..4
    private Inventory _inv;

    public void Bind(Inventory inv)
    {
        if (_inv != null) _inv.OnChanged -= Refresh;
        _inv = inv;

        for (int i = 0; i < quickSlots.Length; i++)
        {
            quickSlots[i].index = i;   // 인벤토리 0~4 미러링
            quickSlots[i].Clear();
        }

        if (_inv != null) { _inv.OnChanged += Refresh; Refresh(); }
    }

    public void Refresh()
    {
        if (_inv == null) return;
        for (int i = 0; i < quickSlots.Length; i++)
            quickSlots[i]?.Set(_inv.GetView(i));
    }
}
