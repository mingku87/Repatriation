// InventorySlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI countText;
    [SerializeField] GameObject durabilityRoot;
    [SerializeField] Image durabilityFill;

    public void Set(Inventory.SlotView v)
    {
        // 아이콘
        if (icon != null)
        {
            icon.sprite = v.icon;
            icon.enabled = v.icon != null;
        }

        // 개수
        if (countText != null)
        {
            countText.gameObject.SetActive(v.showCount);
            if (v.showCount) countText.text = v.count.ToString();
        }

        // 내구도
        if (durabilityRoot != null) durabilityRoot.SetActive(v.showDurability);
        if (v.showDurability && durabilityFill != null)
            durabilityFill.fillAmount = Mathf.Clamp01(v.durability01);
    }

    public void Clear()
    {
        if (icon != null) { icon.sprite = null; icon.enabled = false; }
        if (countText != null) countText.gameObject.SetActive(false);
        if (durabilityRoot != null) durabilityRoot.SetActive(false);
    }
}
