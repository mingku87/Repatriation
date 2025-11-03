using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TMP 텍스트 길이에 따라 말풍선(RectTransform)을 자동으로 리사이즈.
/// LocalizationManager가 있으면 키로 조회 가능.
/// </summary>
public class TextBubbleAuto : MonoBehaviour
{
    [Header("Wiring")]
    public TMP_Text text;             // 버블 안의 TMP_Text
    public RectTransform bubbleRect;  // 말풍선 BG(RectTransform) - 9-sliced 추천
    public Image bubbleImage;         // 선택(알파/색 조정용)

    [Header("Layout")]
    public Vector2 padding = new Vector2(40, 24); // 좌우/상하 여백
    public float minWidth = 120f;
    public float maxWidth = 520f;
    public float minHeight = 60f;

    void OnValidate()
    {
        if (!text) text = GetComponentInChildren<TMP_Text>(true);
        if (!bubbleRect) bubbleRect = GetComponent<RectTransform>();
    }

    public void SetText(string keyOrText, bool isLocalizationKey)
    {
        if (!text || !bubbleRect) return;

        string final = keyOrText ?? "";
        if (isLocalizationKey && LocalizationManager.Instance)
            final = LocalizationManager.Instance.GetOrKey(final);

        text.enableWordWrapping = true;
        text.text = final;
        text.ForceMeshUpdate();

        // 1) 폭 결정
        var preferredW = Mathf.Min(maxWidth, Mathf.Max(minWidth, text.preferredWidth));
        var txtRect = text.rectTransform;
        txtRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredW);

        // 2) 텍스트 높이 재산출
        text.ForceMeshUpdate();
        var preferredH = Mathf.Max(minHeight, text.preferredHeight);

        // 3) 말풍선 사이즈 = 텍스트 + 패딩
        var w = preferredW + padding.x;
        var h = preferredH + padding.y;
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
    }
}
