using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopUIController : MonoBehaviour
{
    public static ShopUIController Instance { get; private set; }

    [Header("Auto Create Options")]
    public GameObject rootPrefab;          // (����) ������
    public string resourcesPath = "UI/ShopItemUI"; // (����) Resources ���

    [Header("Runtime Wiring")]
    public GameObject root;    // ���� UI ��Ʈ(���� ShopItemUI)
    public ShopPanel panel;    // ���� ShopPanel

    [Header("Options")]
    public bool dontDestroyOnLoad = true;
    public KeyCode closeKey = KeyCode.Escape;

    // ���� ��� �ڵ� ����
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

        // 1) ���� �̹� �ִ� ShopPanel/ShopItemUI ���� ä��
        if (!TryAdoptScenePanel())
        {
            // 2) ������ ������/���ҽ����� ����
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
            Debug.LogWarning("[ShopUI] ShopPanel�� �����ϴ�. ShopItemUI�� ShopPanel�� �ٿ����� Ȯ���ϼ���.");
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

    // ���������������������������������� helpers ����������������������������������
    bool TryAdoptScenePanel()
    {
        panel = FindObjectOfType<ShopPanel>(true);
        if (panel == null) return false;

        root = panel.gameObject; // ShopItemUI ��ü�� ��Ʈ�� ���
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
            // ����: �� ��Ʈ �����(���� X)
            var canvas = GetOrCreateCanvas();
            root = new GameObject("ShopItemUI_Auto");
            root.transform.SetParent(canvas.transform, false);
            root.AddComponent<RectTransform>();
            Debug.LogWarning("[ShopUI] ������ ShopPanel�� ã�� ���߰�, ������/���ҽ��� ���� �� UI�� �����߽��ϴ�.");
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
