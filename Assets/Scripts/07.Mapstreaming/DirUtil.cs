using UnityEngine;

public enum ExitDir { LeftUp, LeftDown, RightUp, RightDown }

public static class DirUtil
{
    // 각도(도) → 단위 벡터 (XY 평면)
    public static Vector2 Deg2Dir(float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
    }

    // 포탈 종류별 고정 각도 (우:0°, 위:90° 기준)
    // 아이소 느낌: 우상 60°, 좌상 120°, 좌하 240°, 우하 300°
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
