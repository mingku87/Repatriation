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
        if (Input.GetKeyDown(KeySetting.GetKey(PlayerAction.Inventory)))
        {
            Util.SetActive(inventoryPanel, !inventoryPanel.activeSelf);
            return;
        }

        if (inventoryPanel.activeSelf && Input.GetKeyDown(KeySetting.GetKey(PlayerAction.Escape)))
        {
            Util.SetActive(inventoryPanel, false);
            return;
        }
    }
}