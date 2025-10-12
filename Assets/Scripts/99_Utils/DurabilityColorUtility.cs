using UnityEngine;

public static class DurabilityColorUtility
{
    private static readonly Color HighColor = HexToColor("#85aa5d");
    private static readonly Color MidColor = HexToColor("#eab75e");
    private static readonly Color LowColor = HexToColor("#b0454b");

    private static Color HexToColor(string hex)
    {
        return ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.white;
    }

    public static Color GetColor(float ratio)
    {
        if (ratio >= 2f / 3f)
            return HighColor;
        if (ratio >= 1f / 3f)
            return MidColor;
        if (ratio > 0f)
            return LowColor;
        return LowColor;
    }
}
