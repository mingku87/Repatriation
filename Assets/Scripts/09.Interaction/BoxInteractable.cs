using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxInteractable : InteractableBase
{
    public override void Interact(Transform interactor)
    {
        Debug.Log($"[Box] �ȳ�! ���� ����: {name}", this);
        // TODO: ���� ����
    }
    public override string GetPrompt() => "F - ����";
}