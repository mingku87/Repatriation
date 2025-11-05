using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShopNPC : NPCInteractable
{
    [Header("Shop Info")]
    public string shopId = "shop001"; // 이 키가 DB에 존재해야 목록이 뜸

    public override void Interact(Transform interactor)
    {
        Debug.Log($"[ShopNPC] Interact - npcId={NpcId}, shopId={shopId}, interactor={(interactor ? interactor.name : "NULL")}");

        var ui = ShopUIController.Get();
        if (!ui)
        {
            Debug.LogWarning("[ShopNPC] ShopUIController 못 찾음 - 씬에 ShopUIController가 필요합니다.");
            return;
        }

        ui.OpenForShop(shopId, NpcId);
    }
}
