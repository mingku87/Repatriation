using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxInteractable : InteractableBase
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
        Debug.Log($"[Box] 안녕! 상자 열기: {name}", this);
        // TODO: 상자 열기 로직 (열기에 성공했다고 가정)
        if (markOnSuccess) _svc?.MarkObjectInteracted(objectId);
    }

    public override string GetPrompt() => "F - 열기";
}
