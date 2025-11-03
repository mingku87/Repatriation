using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShopNPC : NPCInteractable
{
    [Header("Shop Info")]
    public string shopId = "shop001";

    // NPCInteractable에 있는 npcId 상속받음
    // NpcId 프로퍼티로 접근 가능 (protected string npcId; public string NpcId => npcId;)

    public override void Interact(Transform interactor)
    {
        var ui = ShopUIController.Get();
        if (!ui)
        {
            Debug.LogWarning("[ShopNPC] ShopUIController 못 찾음");
            return;
        }

        ui.OpenForShop(shopId, NpcId);
    }
}
