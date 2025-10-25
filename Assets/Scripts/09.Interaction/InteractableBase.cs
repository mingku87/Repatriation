using UnityEngine;

public interface IInteractable
{
    bool CanInteract(Transform interactor);  // �Ÿ�/���� üũ
    void Interact(Transform interactor);     // ���� ����
    string GetPrompt();                      // "F - ��ȭ" ���� ����(����)
}

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [Header("Interact")]
    public float interactRadius = 1.0f;
    public virtual bool CanInteract(Transform interactor)
        => (interactor.position - transform.position).sqrMagnitude <= interactRadius * interactRadius;
    public abstract void Interact(Transform interactor);
    public virtual string GetPrompt() => "F - ��ȣ�ۿ�";
}
