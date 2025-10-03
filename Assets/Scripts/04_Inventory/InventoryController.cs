using UnityEngine;
public class InventoryController : SingletonObject<InventoryController>
{
    [Header("�κ��丮 ���� ����")]
    private static bool s_itemsLoaded = false;
    [SerializeField] private int unlockedSlotCount = 5; //�⺻ �ر� ���� �� (�⺻ 5)
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
        //TSV �ݵ�� ���� �ε�
        if (!s_itemsLoaded)
        {
            ItemParameterList.LoadFromTSV(); // "Item_data.tsv"
            s_itemsLoaded = true;
        }

        ItemParameterList.LoadFromTSV();
        inventory = new Inventory();
        inventory.InitSlots(unlockedSlotCount);

        // UI ����
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
        // �κ��丮 ���� UI���� inventory.GetView(i) �����ؼ� �׸� ����
    }

    void RefreshEquipmentUI()
    {
        // ���â ���� UI ����
        // ���� ����/������ ���� ���� ���ϸ�:
        // inventory.TrySetActiveSlotCount(PlayerStatus.Instance.SlotCapacity);
    }

    // ����: ������ ȹ��
    public void AddItemById(int id, int count = 1)
    {
        int remain = inventory.AddItemById(id, count);
        if (remain > 0) Debug.LogWarning($"{remain}���� ���� �������� �� �ݽ��ϴ�.");
    }
}
