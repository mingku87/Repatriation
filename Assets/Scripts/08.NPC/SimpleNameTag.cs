using System.Collections;
using TMPro;
using UnityEngine;

public class SimpleNameTag : MonoBehaviour
{
    [Header("TMP in child Canvas (World Space)")]
    [SerializeField] private TMP_Text label;

    [Header("Layout")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 1.6f, 0);
    [SerializeField] private bool faceCameraBillboard = true;

    [Header("Localization")]
    [Tooltip("����θ� ������(�ν��Ͻ�) �̸��� Ű�� ����մϴ�.")]
    [SerializeField] private string keyOverride = "";
    [Tooltip("Ű�� �ҹ��� ó������ ����(���̺� Ű�� �ҹ��ڶ�� �ѵμ���).")]
    [SerializeField] private bool lowerCaseKey = false;

    Transform _labelRoot;
    Camera _cam;

    void OnValidate()
    {
        if (label == null)
            label = GetComponentInChildren<TMP_Text>(true);
    }

    void Awake()
    {
        if (label == null)
        {
            Debug.LogWarning("[SimpleNameTag] TMP_Text�� �����ϴ�. �ڽĿ� World Space Canvas + TMP_Text �߰��ϼ���.", this);
            enabled = false;
            return;
        }

        _labelRoot = label.transform;
        _cam = Camera.main;
    }

    void OnEnable()
    {
        // LocalizationManager�� �ʰ� �ߴ� ��츦 ���� �� ������ ��õ�
        StartCoroutine(RefreshWhenReady());
    }

    IEnumerator RefreshWhenReady()
    {
        // �ִ� 2��(=120������)���� ����ϸ� ���� �õ�
        const int maxFrames = 120;
        for (int i = 0; i < maxFrames; i++)
        {
            if (TryRefreshLabel()) yield break; // �����ϸ� ����
            yield return null;
        }

        // �׷��� ���и� ���������� Ű�� ����
        label.text = BuildKey();
    }

    bool TryRefreshLabel()
    {
        if (label == null) return true; // �� �� �� ����

        var mgr = LocalizationManager.Instance;
        // �Ŵ����� ���� ������ ���� �����ӿ� �ٽ�
        if (mgr == null) return false;

        string key = BuildKey();
        string value = mgr.GetOrKey(key);   // �� ã���� key �״�� �����شٴ� ����

        if (value == key)
        {
            // ���̺�/Ű Ȯ���� ���� �����
            Debug.LogWarning($"[SimpleNameTag] Ű�� ã�� ���߽��ϴ�. key='{key}' " +
                             $"(���̺� ��Ȯ�� ������ Ű�� �ִ��� Ȯ��)", this);
        }

        label.text = value;
        return true;
    }

    string BuildKey()
    {
        // ������(�ν��Ͻ�) �̸��� Ű�� ���
        string key = string.IsNullOrEmpty(keyOverride)
            ? gameObject.name
            : keyOverride;

        // ���� ������ ����
        key = key.Replace("(Clone)", "").Trim();

        // �ʿ� �� �ҹ��� ����
        if (lowerCaseKey) key = key.ToLowerInvariant();
        return key;
    }

    void LateUpdate()
    {
        if (_labelRoot == null) return;

        // ��ġ ������
        _labelRoot.position = transform.position + worldOffset;

        // ī�޶� �ٶ󺸰�
        if (faceCameraBillboard)
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam != null)
                _labelRoot.rotation = Quaternion.LookRotation(_labelRoot.position - _cam.transform.position);
        }
    }
}
