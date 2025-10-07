using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }

    [Header("Slots")]
    [SerializeField] int unlockedSlotCount = 5;

    [Header("Scene References")]
    [SerializeField] ItemParameterList parameterList;        // TSV 로더(씬에 1개)
    [SerializeField] InventoryUIManager inventoryUI;
    [SerializeField] HudQuickBarUI hudQuickBarUI;
    [SerializeField] public EquipmentUIManager equipmentUI;

    // 외부 스크립트에서 참조하는 공개 속성들
    public Inventory inventory { get; private set; }

    /// <summary>
    /// 다른 스크립트들이 기대하는 이름 그대로 노출 (PlayerStatsPanelUI 등)
    /// </summary>
    public EquipmentModel equipment { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        unlockedSlotCount = 10; // ✅ 강제 세팅

        // ✅ 자동 연결 시도 (필요할 경우 true 인자로 비활성 객체도 포함)
        if (parameterList == null)
            parameterList = FindObjectOfType<ItemParameterList>(true);

        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<InventoryUIManager>(true);

        if (hudQuickBarUI == null)
            hudQuickBarUI = FindObjectOfType<HudQuickBarUI>(true);

        if (equipmentUI == null)
            equipmentUI = FindObjectOfType<EquipmentUIManager>(true);
    }

    void Start()
    {
        Initialize();
    }

    /// <summary>
    /// 1) TSV 로드 → 2) 인벤토리/장비 모델 생성 → 3) UI 바인딩
    /// </summary>
    public void Initialize()
    {
        // 1) TSV 먼저 로드(인스턴스 호출)
        if (parameterList == null)
        {
            Debug.LogError("[InventoryController] ItemParameterList not found in scene.");
            return;
        }
        parameterList.LoadFromTSV();

        // 2) 인벤토리 준비
        if (inventory == null) inventory = new Inventory();
        inventory.SetBaseUnlocked(unlockedSlotCount);              // 먼저 해금 수 설정
        inventory.InitSlots(inventory.ActiveSlotCount);            // 해금 수 기준으로 초기화
        inventory.SetBaseMaxWeight(InventoryConstant.DefaultMaxCarryWeight);

        // 3) 장비 모델 준비 (프로젝트에 맞는 생성자를 사용하세요)
        if (equipment == null) equipment = new EquipmentModel();
        // 인벤토리-장비 모델 간 연결이 필요하면 여기에서 해주세요.
        // 예: equipment.BindInventory(inventory);

        // 4) UI 바인딩
        if (inventoryUI != null) inventoryUI.Bind(inventory);
        if (hudQuickBarUI != null) hudQuickBarUI.Bind(inventory);
        if (equipmentUI != null) equipmentUI.Initialize(equipment); // ★ 모델 전달

        // 5) 최초 갱신
        // 주의: event는 외부에서 Invoke 불가 → 각 UI가 바인딩 시 구독하므로
        // 여기서는 강제 갱신이 필요하면 UI 쪽 공개 메서드로 Refresh를 호출하세요.
        TryInitialRefresh();
    }

    /// <summary>
    /// 초기 1회 화면 갱신(이벤트 Invoke 금지; UI 제공 메서드로 호출)
    /// </summary>
    void TryInitialRefresh()
    {
        // 프로젝트 구현에 맞춰 있는 메서드만 호출하세요.
        // 아래 메서드명이 없다면 주석 처리해도 무방합니다.
        try
        {
            if (inventoryUI != null)
                inventoryUI.RefreshInventory();

            if (hudQuickBarUI != null)
                hudQuickBarUI.Refresh();

            if (equipmentUI != null)
                equipmentUI.Refresh();
        }
        catch { /* UI 컴포넌트가 해당 메서드를 안 가지면 무시 */ }
    }
}
