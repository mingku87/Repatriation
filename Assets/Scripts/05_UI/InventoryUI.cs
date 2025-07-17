using UnityEngine;

public class InventoryUI : SingletonObject<InventoryUI>
{
    [SerializeField] private GameObject inventoryPanel;

    void Update()
    {
        ShowInventory();
    }

    public void ShowInventory()
    {
        if (Input.GetKeyDown(KeySetting.GetKey(PlayerAction.Inventory)) == false) return;
        Util.SetActive(inventoryPanel, !inventoryPanel.activeSelf);
    }
}