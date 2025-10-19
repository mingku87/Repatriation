using UnityEngine;
using Game.Dialogue;

[DefaultExecutionOrder(500)] // DialogueManager 이후에 돌고 싶으면 숫자 조절
public class DialogueInputBridge : MonoBehaviour
{
    private static DialogueInputBridge _instance;
    private DialogueManager manager;

    void Awake()
    {
        // 싱글턴
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 씬에서 DialogueManager 찾아서 연결
        manager = FindObjectOfType<DialogueManager>();
        if (manager == null)
            Debug.LogWarning("DialogueInputBridge: DialogueManager를 아직 못 찾았습니다. 나중에 생기면 자동 연결됩니다.");
    }

    void Update()
    {
        // 매 프레임 늦게 생긴 매니저도 자동 연결
        if (manager == null)
        {
            manager = FindObjectOfType<DialogueManager>();
        }

        if (manager == null || !manager.IsActive) return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.F) ||
            Input.GetMouseButtonDown(0))
        {
            manager.Next();
        }
    }
}

// ▶ 씬에 없어도 자동 생성 (에디터/빌드 공통)
public static class DialogueInputBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureInputBridge()
    {
        if (Object.FindObjectOfType<DialogueInputBridge>() == null)
        {
            var go = new GameObject("DialogueInputBridge (Auto)");
            go.AddComponent<DialogueInputBridge>();
            // 필요하면 여기서 위치/계층 조정 가능
        }
    }
}
