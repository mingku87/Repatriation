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
        // �����Ϳ��� �ڵ� �Ҵ�
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

        // ������ ��ġ ������(�ڽ��̶� localPosition�̸� ���)
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

        // ���� ������ (�ʿ� ������ �� ��� ��°�� ���� ��)
        var dir = _labelRoot.position - _cam.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 1e-6f)
            _labelRoot.rotation = Quaternion.LookRotation(dir);
    }

    void ApplyLocalizedText()
    {
        if (label == null) return;

        // ������Ʈ �̸��� �״�� Ű�� ����ϵ�, (Clone) �ڵ� ����
        string key = gameObject.name.Replace("(Clone)", "").Trim();

        // LocalizationManager ������ Ű �״�� ǥ��
        string text = (LocalizationManager.Instance != null)
                        ? LocalizationManager.Instance.GetOrKey(key)
                        : key;

        label.text = text;
    }
}
