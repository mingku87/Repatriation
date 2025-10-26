using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShopNPC : NPCInteractable
{
    [Header("Shop Identity")]
    public string shopId = "shop001";

    public override void Interact(Transform interactor)
    {
        if (ShopUIController.Instance == null)
        {
            Debug.LogWarning("[ShopNPC] ShopUIController가 씬에 없습니다.");
            return;
        }

        ShopUIController.Instance.OpenForShop(shopId);
        // 필요하면: 플레이어 이동/입력 잠그기 등 여기서 처리
    }
}
