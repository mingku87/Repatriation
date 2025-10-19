using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxInteractable : InteractableBase
{
    public override void Interact(Transform interactor)
    {
        Debug.Log($"[Box] 안녕! 상자 열기: {name}", this);
        // TODO: 상자 열기
    }
    public override string GetPrompt() => "F - 열기";
}