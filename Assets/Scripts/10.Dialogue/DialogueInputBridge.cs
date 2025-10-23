using UnityEngine;
using Game.Dialogue;

[DefaultExecutionOrder(500)] // DialogueManager Ŀ    
public class DialogueInputBridge : MonoBehaviour
{
    private static DialogueInputBridge _instance;
    private DialogueManager manager;

    int _suppressInputFrame = -1;

    void Awake()
    {
        // ̱
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

        AttachManager(FindObjectOfType<DialogueManager>());
        if (manager == null)
            Debug.LogWarning("DialogueInputBridge: DialogueManager   ãҽϴ. ߿  ڵ ˴ϴ.");
    }

    void Update()
    {
        if (manager == null)
        {
            AttachManager(FindObjectOfType<DialogueManager>());
        }

        if (manager == null || !manager.IsActive) return;

        if (Time.frameCount == _suppressInputFrame)
            return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.F) ||
            Input.GetMouseButtonDown(0))
        {
            manager.Next();
        }
    }

    void AttachManager(DialogueManager dm)
    {
        if (manager == dm) return;

        if (manager != null)
        {
            manager.onDialogueStart.RemoveListener(OnDialogueStarted);
        }

        manager = dm;

        if (manager != null)
        {
            manager.onDialogueStart.AddListener(OnDialogueStarted);
        }
    }

    void OnDialogueStarted(string npcId, string dialogueId)
    {
        _suppressInputFrame = Time.frameCount;
    }

    void OnDestroy()
    {
        AttachManager(null);
    }
}

//    ڵ  (/ )
public static class DialogueInputBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureInputBridge()
    {
        if (Object.FindObjectOfType<DialogueInputBridge>() == null)
        {
            var go = new GameObject("DialogueInputBridge (Auto)");
            go.AddComponent<DialogueInputBridge>();
            // ʿϸ ⼭ ġ/  
        }
    }
}