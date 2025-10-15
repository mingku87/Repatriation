using UnityEngine;

public enum ExitDir { LeftUp, LeftDown, RightUp, RightDown }

public static class DirUtil
{
    // ����(��) �� ���� ���� (XY ���)
    public static Vector2 Deg2Dir(float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
    }

    // ��Ż ������ ���� ���� (��:0��, ��:90�� ����)
    // ���̼� ����: ��� 60��, �»� 120��, ���� 240��, ���� 300��
    public static float ToRayAngleDeg(ExitDir d) => d switch
    {
        ExitDir.RightUp => 30f,
        ExitDir.LeftUp => 120f,
        ExitDir.LeftDown => 210f,
        ExitDir.RightDown => 300f,
        _ => 0f
    };

    public static Vector2 ToRayDir(ExitDir d) => Deg2Dir(ToRayAngleDeg(d));
}
