using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInteractable : InteractableBase
{
    [SerializeField] string objectId;
    [SerializeField] bool markOnSuccess = true;
    GameConditionService _svc;

    void Awake()
    {
        _svc = FindFirstObjectByType<GameConditionService>();
        if (string.IsNullOrEmpty(objectId))
            objectId = $"{gameObject.scene.name}:{gameObject.name}";
    }

    public override void Interact(Transform interactor)
    {
        Debug.Log($"[Button] �ȳ�! ��ư ����: {name}", this);
        // TODO: ��ư ���� (���� ���� ��)
        if (markOnSuccess) _svc?.MarkObjectInteracted(objectId);
    }

    public override string GetPrompt() => "F - ��ư";
}
