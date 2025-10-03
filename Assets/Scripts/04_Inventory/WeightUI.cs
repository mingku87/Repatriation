using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeightUI : MonoBehaviour
{
    [SerializeField] TMP_Text label;     // "0/000"
    [SerializeField] Image fill;         // 게이지 fill

    [Header("최대 하중(임시/외부에서 갱신 가능)")]
    [SerializeField] float maxCapacity = 100f;   // 인스펙터에서 설정하거나 외부에서 SetMaxCapacity로 갱신
    void OnEnable()
    {
        var ctrl = InventoryController.Instance;
        if (ctrl == null)
        {
            Debug.LogError("[WeightUI] InventoryController not found.");
            return;
        }
        if (ctrl.inventory == null) ctrl.Initialize();   // 안전하게 초기화

        ctrl.inventory.OnChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        var ctrl = InventoryController.Instance;
        if (ctrl != null && ctrl.inventory != null)
            ctrl.inventory.OnChanged -= Refresh;
    }

    public void SetMaxCapacity(float value)
    {
        maxCapacity = Mathf.Max(1f, value);
        Refresh();
    }

    void Refresh()
    {
        var inv = InventoryController.Instance.inventory;

        float current = inv.GetInventoryWeightOnly();  // 인벤토리만(장비 제외)
        float max = Mathf.Max(1f, maxCapacity);

        if (label != null)
            label.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";

        if (fill != null)
            fill.fillAmount = Mathf.Clamp01(current / max);
    }
}
