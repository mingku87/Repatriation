using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiscardQuantityPopup : MonoBehaviour
{
    public static DiscardQuantityPopup Instance { get; private set; }

    public static DiscardQuantityPopup EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        Instance = FindObjectOfType<DiscardQuantityPopup>(includeInactive: true);

        if (Instance != null && Instance.root == null)
            Instance.root = Instance.gameObject;

        return Instance;
    }

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
    private int _min = 0;
    private Action<int> _onConfirm;
    private bool _activatedSelf = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (root == null)
            root = gameObject;

        SetPopupActive(false);

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

    public void Show(int max, Action<int> onConfirm, string title = "몇 개 버리시겠습니까?", int initialValue = 1)
    {
        _max = Mathf.Max(0, max);
        _min = _max > 0 ? 1 : 0;
        _onConfirm = onConfirm;
        if (titleLabel) titleLabel.text = title;

        int startValue = Mathf.Clamp(initialValue, _min, _max);
        SetValue(startValue);
        RefreshState(startValue);

        SetPopupActive(true);
        transform.SetAsLastSibling();
    }

    public void Close()
    {
        SetPopupActive(false);
        _onConfirm = null;
    }

    private void OnClickOK()
    {
        int val = GetValue();
        if (val < _min) return;
        _onConfirm?.Invoke(Mathf.Clamp(val, _min, _max));
        Close();
    }

    private void SetPopupActive(bool active)
    {
        if (active && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            _activatedSelf = true;
        }

        var target = root != null ? root : gameObject;
        if (target != null && target.activeSelf != active)
            target.SetActive(active);

        if (!active && _activatedSelf)
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
            _activatedSelf = false;
        }

        if (!active && target == gameObject)
            _activatedSelf = false;
    }

    private void Step(int delta)
    {
        int v = Mathf.Clamp(GetValue() + delta, _min, _max);
        SetValue(v);
        RefreshState(v);
    }

    private void OnInputEndEdit(string s)
    {
        if (!int.TryParse(s, out int v)) v = 0;
        v = Mathf.Clamp(v, _min, _max);
        SetValue(v);
        RefreshState(v);
    }

    private void OnInputChanged(string s)
    {
        if (!int.TryParse(s, out int v)) v = 0;
        v = Mathf.Clamp(v, _min, _max);
        RefreshState(v);
    }

    private void SetValue(int v)
    {
        v = Mathf.Clamp(v, _min, _max);
        if (input) input.SetTextWithoutNotify(v.ToString());
    }

    private int GetValue()
    {
        if (input && int.TryParse(input.text, out int v))
            return Mathf.Clamp(v, _min, _max);
        return 0;
    }

    private void SetOKInteractable(bool on)
    {
        if (okButton) okButton.interactable = on;
    }

    private void RefreshState(int value)
    {
        SetOKInteractable(value >= _min && value > 0);

        if (plusButton)
            plusButton.interactable = value < _max;

        if (minusButton)
            minusButton.interactable = value > _min;
    }

    void Update()
    {
        if (root && root.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }
}
