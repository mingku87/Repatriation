using UnityEngine;
public class InventoryController : SingletonObject<InventoryController>
{
    [Header("인벤토리 슬롯 설정")]
    private static bool s_itemsLoaded = false;
    [SerializeField] private int unlockedSlotCount = 5; //기본 해금 슬롯 수 (기본 5)
    [SerializeField] InventoryUIManager ui;
    [SerializeField] HudQuickBarUI hudUI;
    public Inventory inventory { get; private set; }
    public EquipmentModel equipment { get; private set; }
    protected override void Awake()
    {
        base.Awake();
        if (inventory == null) Initialize();
    }

    public void Initialize()
    {
        //TSV 반드시 먼저 로드
        if (!s_itemsLoaded)
        {
            ItemParameterList.LoadFromTSV(); // "Item_data.tsv"
            s_itemsLoaded = true;
        }

        ItemParameterList.LoadFromTSV();
        inventory = new Inventory();
        inventory.InitSlots(unlockedSlotCount);

        // UI 구독
        if (ui == null) ui = FindObjectOfType<InventoryUIManager>();
        if (!hudUI) hudUI = FindObjectOfType<HudQuickBarUI>();

        if (ui != null)
        {
            inventory.OnChanged += ui.RefreshInventory;
            ui.Bind(inventory);
        }

        if (hudUI)
        {
            hudUI.Bind(inventory);
        }

        equipment = new EquipmentModel();

        inventory.OnChanged += RefreshInventoryUI;
        equipment.OnChanged += RefreshEquipmentUI;
    }

    void RefreshInventoryUI()
    {
        // 인벤토리 슬롯 UI에게 inventory.GetView(i) 전달해서 그림 갱신
    }

    void RefreshEquipmentUI()
    {
        // 장비창 슬롯 UI 갱신
        // 가방 착용/해제로 슬롯 수가 변하면:
        // inventory.TrySetActiveSlotCount(PlayerStatus.Instance.SlotCapacity);
    }

    // 편의: 아이템 획득
    public void AddItemById(int id, int count = 1)
    {
        int remain = inventory.AddItemById(id, count);
        if (remain > 0) Debug.LogWarning($"{remain}개는 공간 부족으로 못 줍습니다.");
    }
}
