using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeightUI : MonoBehaviour
{
    [SerializeField] TMP_Text label;   // "0/100"
    [SerializeField] Image fill;       // 게이지 fill (Image.Type=Filled)

    [Header("최대 하중(임시/외부에서 갱신 가능)")]
    [SerializeField] float maxCapacity = 100f;

    // 현재 구독 중인 인벤토리 캐시(인스턴스 변경 감지용)
    private Inventory boundInventory;

    void OnEnable()
    {
        // 게이지 이미지 타입을 강제 세팅(인스펙터 실수 방지)
        if (fill != null)
        {
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0; // Left
            fill.fillAmount = 0f;
        }

        TryBind();   // 현재 인벤토리에 구독
        Refresh();   // 첫 그리기
    }

    void OnDisable()
    {
        Unbind();
    }

    // 외부에서 최대 하중 갱신 가능
    public void SetMaxCapacity(float value)
    {
        maxCapacity = Mathf.Max(1f, value);
        if (boundInventory != null)
            boundInventory.SetBaseMaxWeight(maxCapacity);
        else
            Refresh();
    }

    void Update()
    {
        // 인벤토리 인스턴스가 바뀌면(재초기화 등) 자동 재바인딩
        var ctrl = InventoryController.Instance;
        var currentInv = ctrl != null ? ctrl.inventory : null;
        if (currentInv != boundInventory)
        {
            TryBind();
            Refresh();
        }
    }

    private void TryBind()
    {
        var ctrl = InventoryController.Instance;
        if (ctrl == null)
        {
            Debug.LogError("[WeightUI] InventoryController not found.");
            return;
        }

        // 인벤토리가 아직 없으면 생성
        if (ctrl.inventory == null)
        {
            Debug.Log("[WeightUI] Inventory null → Initialize()");
            ctrl.Initialize();
        }

        // 이전 구독 해제
        Unbind();

        // 새 인벤토리에 구독
        boundInventory = ctrl.inventory;
        if (boundInventory != null)
        {
            boundInventory.SetBaseMaxWeight(maxCapacity);
            boundInventory.OnChanged += Refresh;

            // 장비 착탈로 가방 보너스/슬롯 변동 시점을 캐치하고 싶으면 장비 모델에도 구독(선택)
            if (ctrl.equipment != null)
                ctrl.equipment.OnChanged += Refresh;
        }
    }

    private void Unbind()
    {
        var ctrl = InventoryController.Instance;
        if (boundInventory != null)
        {
            boundInventory.OnChanged -= Refresh;
        }
        if (ctrl != null && ctrl.equipment != null)
        {
            ctrl.equipment.OnChanged -= Refresh;
        }
        boundInventory = null;
    }

    private void Refresh()
    {
        var ctrl = InventoryController.Instance;
        if (ctrl == null || ctrl.inventory == null) return;

        var inv = ctrl.inventory;

        float current = inv.GetInventoryWeightOnly(); // 인벤토리만(장비 제외)
        float max = Mathf.Max(1f, inv.GetMaxWeightCapacity());

        if (label != null)
            label.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";

        if (fill != null)
            fill.fillAmount = Mathf.Clamp01(current / max);

        // ── 디버깅: 합/슬롯별 현황(에디터에서만)
#if UNITY_EDITOR
        Debug.Log($"[WeightUI] weight={current}, max={max}, fill={(fill ? fill.fillAmount : -1f)}");

        float recomputed = 0f;
        var debugInv = inv;
        for (int i = 0; i < debugInv.ActiveSlotCount; i++)
        {
            var s = debugInv.slots[i];
            if (!s.IsEmpty)
            {
                float w = s.item?.param?.weight ?? 0f;
                Debug.Log($"[WeightUI] slot {i}: id={s.item.id}, count={s.count}, weightPerItem={w}, subtotal={w * s.count}");
                recomputed += w * s.count;
            }
        }
        Debug.Log($"[WeightUI] recomputedSum={recomputed} (should match weight)");
#endif
    }
}
