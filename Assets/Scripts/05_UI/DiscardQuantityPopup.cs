using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiscardQuantityPopup : MonoBehaviour
{
    public static DiscardQuantityPopup Instance { get; private set; }

    [Header("Root / Title")]
    [SerializeField] private GameObject root;      // Panel 루트
    [SerializeField] private TMP_Text titleLabel;  // "몇 개 버리시겠습니까?"

    [Header("Controls")]
    [SerializeField] private TMP_InputField input; // 숫자 입력(기본 0)
    [SerializeField] private Button plusButton;    // +
    [SerializeField] private Button minusButton;   // -
    [SerializeField] private Button okButton;      // 확인
    [SerializeField] private Button cancelButton;  // 취소

    private int _max = 0;
    private Action<int> _onConfirm;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (root) root.SetActive(false);

        if (plusButton) plusButton.onClick.AddListener(() => Step(+1));
        if (minusButton) minusButton.onClick.AddListener(() => Step(-1));
        if (okButton) okButton.onClick.AddListener(OnClickOK);   // ✅ 오타 수정
        if (cancelButton) cancelButton.onClick.AddListener(Close);

        if (input)
        {
            input.contentType = TMP_InputField.ContentType.IntegerNumber;
            input.onEndEdit.AddListener(OnInputEndEdit);
            input.onValueChanged.AddListener(OnInputChanged);
            input.SetTextWithoutNotify("0");
        }
    }

    public void Show(int max, Action<int> onConfirm, string title = "몇 개 버리시겠습니까?")
    {
        _max = Mathf.Max(0, max);
        _onConfirm = onConfirm;
        if (titleLabel) titleLabel.text = title;

        SetValue(0);           // 기본 0
        SetOKInteractable(false);

        if (root) root.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Close()
    {
        if (root) root.SetActive(false);
        _onConfirm = null;
    }

    private void OnClickOK()
    {
        int val = GetValue();
        if (val <= 0) return;
        _onConfirm?.Invoke(Mathf.Clamp(val, 0, _max));
        Close();
    }

    private void Step(int delta)
    {
        int v = Mathf.Clamp(GetValue() + delta, 0, _max);
        SetValue(v);
        SetOKInteractable(v > 0);
    }

    private void OnInputEndEdit(string s)
    {
        if (!int.TryParse(s, out int v)) v = 0;
        v = Mathf.Clamp(v, 0, _max);
        SetValue(v);
        SetOKInteractable(v > 0);
    }

    private void OnInputChanged(string s)
    {
        if (!int.TryParse(s, out int v)) v = 0;
        v = Mathf.Clamp(v, 0, _max);
        SetOKInteractable(v > 0);
    }

    private void SetValue(int v)
    {
        v = Mathf.Clamp(v, 0, _max);
        if (input) input.SetTextWithoutNotify(v.ToString());
    }

    private int GetValue()
    {
        if (input && int.TryParse(input.text, out int v))
            return Mathf.Clamp(v, 0, _max);
        return 0;
    }

    private void SetOKInteractable(bool on)
    {
        if (okButton) okButton.interactable = on;
    }

    void Update()
    {
        if (root && root.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }
}
