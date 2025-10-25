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
        Debug.Log($"[Button] 안녕! 버튼 눌림: {name}", this);
        // TODO: 버튼 로직 (성공 판정 뒤)
        if (markOnSuccess) _svc?.MarkObjectInteracted(objectId);
    }

    public override string GetPrompt() => "F - 버튼";
}
