using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFacing : MonoBehaviour
{
    // 마지막 이동 방향 (기본은 오른쪽)
    public Vector2 lastMoveDir = Vector2.right;

    // 매 프레임 입력을 받아 방향 갱신
    public void SetInput(Vector2 move)
    {
        if (move.sqrMagnitude > 0.0001f)
            lastMoveDir = move.normalized;
    }

    // 현재 바라보는 방향 리턴
    public Vector2 GetFacing()
    {
        return lastMoveDir;
    }
}
