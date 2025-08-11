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
        if (Input.GetKeyDown(KeyManager.GetKey(PlayerAction.Inventory)))
        {
            Util.SetActive(inventoryPanel, !inventoryPanel.activeSelf);
            return;
        }

        if (inventoryPanel.activeSelf && Input.GetKeyDown(KeyManager.GetKey(PlayerAction.Escape)))
        {
            Util.SetActive(inventoryPanel, false);
            return;
        }
    }
}