using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFacing : MonoBehaviour
{
    // ������ �̵� ���� (�⺻�� ������)
    public Vector2 lastMoveDir = Vector2.right;

    // �� ������ �Է��� �޾� ���� ����
    public void SetInput(Vector2 move)
    {
        if (move.sqrMagnitude > 0.0001f)
            lastMoveDir = move.normalized;
    }

    // ���� �ٶ󺸴� ���� ����
    public Vector2 GetFacing()
    {
        return lastMoveDir;
    }
}
