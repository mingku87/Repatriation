using TMPro;
using UnityEngine;

public class SimpleNameTag : MonoBehaviour
{
    [Header("TMP in child Canvas (World Space)")]
    [SerializeField] TMP_Text label;

    [Header("Layout")]
    [SerializeField] Vector3 worldOffset = new Vector3(0, 1.6f, 0);
    [SerializeField] bool faceCameraBillboard = true;

    Transform _labelRoot;
    Camera _cam;

    void OnValidate()
    {
        // 에디터에서 자동 할당
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
    }

    void Awake()
    {
        if (label == null)
        {
            Debug.LogWarning($"{name}: SimpleNameTag - TMP_Text reference missing.");
            return;
        }

        _labelRoot = label.transform.parent != null ? label.transform.parent : label.transform;
        _cam = Camera.main;

        // 네임택 위치 오프셋(자식이라 localPosition이면 충분)
        _labelRoot.localPosition = worldOffset;

        ApplyLocalizedText();
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += ApplyLocalizedText;
    }

    void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= ApplyLocalizedText;
    }

    void LateUpdate()
    {
        if (!faceCameraBillboard || _cam == null || _labelRoot == null) return;

        // 수평 빌보딩 (필요 없으면 이 블록 통째로 꺼도 됨)
        var dir = _labelRoot.position - _cam.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 1e-6f)
            _labelRoot.rotation = Quaternion.LookRotation(dir);
    }

    void ApplyLocalizedText()
    {
        if (label == null) return;

        // 오브젝트 이름을 그대로 키로 사용하되, (Clone) 자동 제거
        string key = gameObject.name.Replace("(Clone)", "").Trim();

        // LocalizationManager 없으면 키 그대로 표시
        string text = (LocalizationManager.Instance != null)
                        ? LocalizationManager.Instance.GetOrKey(key)
                        : key;

        label.text = text;
    }
}
