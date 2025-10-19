using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInteractable : InteractableBase
{
    public override void Interact(Transform interactor)
    {
        Debug.Log($"[Button] 안녕! 버튼 눌림: {name}", this);
        // TODO: 버튼 로직
    }
    public override string GetPrompt() => "F - 버튼";
}
