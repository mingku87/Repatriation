using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteractable : InteractableBase
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
        Debug.Log($"[Door] 안녕! 문 열기: {name}", this);
        // TODO: 문 열기/장면 전환 로직 (성공 시)
        if (markOnSuccess) _svc?.MarkObjectInteracted(objectId);
    }

    public override string GetPrompt() => "F - 문 열기";
}
