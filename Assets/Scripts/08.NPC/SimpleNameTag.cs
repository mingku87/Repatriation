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
    [Tooltip("비워두면 프리팹(인스턴스) 이름을 키로 사용합니다.")]
    [SerializeField] private string keyOverride = "";
    [Tooltip("키를 소문자 처리할지 여부(테이블 키가 소문자라면 켜두세요).")]
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
            Debug.LogWarning("[SimpleNameTag] TMP_Text가 없습니다. 자식에 World Space Canvas + TMP_Text 추가하세요.", this);
            enabled = false;
            return;
        }

        _labelRoot = label.transform;
        _cam = Camera.main;
    }

    void OnEnable()
    {
        // LocalizationManager가 늦게 뜨는 경우를 위해 몇 프레임 재시도
        StartCoroutine(RefreshWhenReady());
    }

    IEnumerator RefreshWhenReady()
    {
        // 최대 2초(=120프레임)까지 대기하며 갱신 시도
        const int maxFrames = 120;
        for (int i = 0; i < maxFrames; i++)
        {
            if (TryRefreshLabel()) yield break; // 성공하면 종료
            yield return null;
        }

        // 그래도 실패면 마지막으로 키만 찍어둠
        label.text = BuildKey();
    }

    bool TryRefreshLabel()
    {
        if (label == null) return true; // 더 할 일 없음

        var mgr = LocalizationManager.Instance;
        // 매니저가 아직 없으면 다음 프레임에 다시
        if (mgr == null) return false;

        string key = BuildKey();
        string value = mgr.GetOrKey(key);   // 못 찾으면 key 그대로 돌려준다는 가정

        if (value == key)
        {
            // 테이블/키 확인을 위한 디버그
            Debug.LogWarning($"[SimpleNameTag] 키를 찾지 못했습니다. key='{key}' " +
                             $"(테이블에 정확히 동일한 키가 있는지 확인)", this);
        }

        label.text = value;
        return true;
    }

    string BuildKey()
    {
        // 프리팹(인스턴스) 이름을 키로 사용
        string key = string.IsNullOrEmpty(keyOverride)
            ? gameObject.name
            : keyOverride;

        // 흔한 노이즈 제거
        key = key.Replace("(Clone)", "").Trim();

        // 필요 시 소문자 통일
        if (lowerCaseKey) key = key.ToLowerInvariant();
        return key;
    }

    void LateUpdate()
    {
        if (_labelRoot == null) return;

        // 위치 오프셋
        _labelRoot.position = transform.position + worldOffset;

        // 카메라를 바라보게
        if (faceCameraBillboard)
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam != null)
                _labelRoot.rotation = Quaternion.LookRotation(_labelRoot.position - _cam.transform.position);
        }
    }
}
