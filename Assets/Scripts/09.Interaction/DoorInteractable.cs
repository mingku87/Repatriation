using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteractable : InteractableBase
{
    public override void Interact(Transform interactor)
    {
        Debug.Log($"[Door] 안녕! 문 열기: {name}", this);
        // TODO: 문 열기/장면 전환 등
    }
    public override string GetPrompt() => "F - 문 열기";
}
