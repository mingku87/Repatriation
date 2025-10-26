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
            Debug.LogWarning("[ShopNPC] ShopUIController�� ���� �����ϴ�.");
            return;
        }

        ShopUIController.Instance.OpenForShop(shopId);
        // �ʿ��ϸ�: �÷��̾� �̵�/�Է� ��ױ� �� ���⼭ ó��
    }
}
