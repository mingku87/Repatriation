using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopUIController : MonoBehaviour
{
    public static ShopUIController Instance { get; private set; }

    [Header("Auto Create Options")]
    public GameObject rootPrefab;          // (선택) 프리팹
    public string resourcesPath = "UI/ShopItemUI"; // (선택) Resources 경로

    [Header("Runtime Wiring")]
    public GameObject root;    // 상점 UI 루트(씬의 ShopItemUI)
    public ShopPanel panel;    // 씬의 ShopPanel

    [Header("Options")]
    public bool dontDestroyOnLoad = true;
    public KeyCode closeKey = KeyCode.Escape;

    // 씬에 없어도 자동 생성
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (Instance != null) return;
        new GameObject("ShopUIController_Auto").AddComponent<ShopUIController>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        EnsureEventSystem();

        // 1) 씬에 이미 있는 ShopPanel/ShopItemUI 먼저 채택
        if (!TryAdoptScenePanel())
        {
            // 2) 없으면 프리팹/리소스에서 생성
            EnsureUIRootFromPrefabOrResources();
            EnsurePanelFromRoot();
        }

        if (root != null) root.SetActive(false);
    }

    void Update()
    {
        if (root && root.activeSelf && Input.GetKeyDown(closeKey)) Close();
    }

    public void OpenForShop(string shopId)
    {
        if (panel == null)
        {
            Debug.LogWarning("[ShopUI] ShopPanel이 없습니다. ShopItemUI에 ShopPanel을 붙였는지 확인하세요.");
            return;
        }
        panel.SetShopAndRefresh(shopId);
        root.SetActive(true);
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
    }

    public void Close()
    {
        if (root) root.SetActive(false);
    }

    // ───────────────── helpers ─────────────────
    bool TryAdoptScenePanel()
    {
        panel = FindObjectOfType<ShopPanel>(true);
        if (panel == null) return false;

        root = panel.gameObject; // ShopItemUI 자체를 루트로 사용
        return true;
    }

    void EnsureUIRootFromPrefabOrResources()
    {
        if (root != null) return;

        GameObject prefab = rootPrefab;
        if (prefab == null && !string.IsNullOrEmpty(resourcesPath))
            prefab = Resources.Load<GameObject>(resourcesPath);

        if (prefab != null)
        {
            var canvas = GetOrCreateCanvas();
            root = Instantiate(prefab, canvas.transform);
            root.name = prefab.name;
        }
        else
        {
            // 최후: 빈 루트 만들기(권장 X)
            var canvas = GetOrCreateCanvas();
            root = new GameObject("ShopItemUI_Auto");
            root.transform.SetParent(canvas.transform, false);
            root.AddComponent<RectTransform>();
            Debug.LogWarning("[ShopUI] 씬에서 ShopPanel을 찾지 못했고, 프리팹/리소스도 없어 빈 UI를 생성했습니다.");
        }
    }

    void EnsurePanelFromRoot()
    {
        if (panel == null && root != null)
            panel = root.GetComponentInChildren<ShopPanel>(true);
    }

    Canvas GetOrCreateCanvas()
    {
        var c = FindObjectOfType<Canvas>();
        if (c) return c;
        var go = new GameObject("Canvas");
        c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }
}
