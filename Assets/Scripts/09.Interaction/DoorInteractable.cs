using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteractable : InteractableBase
{
    public override void Interact(Transform interactor)
    {
        Debug.Log($"[Door] �ȳ�! �� ����: {name}", this);
        // TODO: �� ����/��� ��ȯ ��
    }
    public override string GetPrompt() => "F - �� ����";
}
