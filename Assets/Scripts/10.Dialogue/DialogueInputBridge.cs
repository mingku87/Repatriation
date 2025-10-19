using UnityEngine;
using Game.Dialogue;

[DefaultExecutionOrder(500)] // DialogueManager ���Ŀ� ���� ������ ���� ����
public class DialogueInputBridge : MonoBehaviour
{
    private static DialogueInputBridge _instance;
    private DialogueManager manager;

    void Awake()
    {
        // �̱���
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
        // ������ DialogueManager ã�Ƽ� ����
        manager = FindObjectOfType<DialogueManager>();
        if (manager == null)
            Debug.LogWarning("DialogueInputBridge: DialogueManager�� ���� �� ã�ҽ��ϴ�. ���߿� ����� �ڵ� ����˴ϴ�.");
    }

    void Update()
    {
        // �� ������ �ʰ� ���� �Ŵ����� �ڵ� ����
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

// �� ���� ��� �ڵ� ���� (������/���� ����)
public static class DialogueInputBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureInputBridge()
    {
        if (Object.FindObjectOfType<DialogueInputBridge>() == null)
        {
            var go = new GameObject("DialogueInputBridge (Auto)");
            go.AddComponent<DialogueInputBridge>();
            // �ʿ��ϸ� ���⼭ ��ġ/���� ���� ����
        }
    }
}
