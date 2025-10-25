using UnityEngine;

public interface IInteractable
{
    bool CanInteract(Transform interactor);  // 거리/상태 체크
    void Interact(Transform interactor);     // 실제 동작
    string GetPrompt();                      // "F - 대화" 같은 문구(선택)
}

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [Header("Interact")]
    public float interactRadius = 1.0f;
    public virtual bool CanInteract(Transform interactor)
        => (interactor.position - transform.position).sqrMagnitude <= interactRadius * interactRadius;
    public abstract void Interact(Transform interactor);
    public virtual string GetPrompt() => "F - 상호작용";
}
