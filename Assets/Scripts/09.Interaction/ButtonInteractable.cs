using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInteractable : InteractableBase
{
    public override void Interact(Transform interactor)
    {
        Debug.Log($"[Button] �ȳ�! ��ư ����: {name}", this);
        // TODO: ��ư ����
    }
    public override string GetPrompt() => "F - ��ư";
}
