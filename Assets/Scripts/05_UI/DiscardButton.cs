using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DiscardButton : MonoBehaviour, IDropHandler
{
    [Header("Refs")]
    [SerializeField] private EquipmentSlotUI slot; // 장비창의 경우만 사용
    [SerializeField] private Button button;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(OnClickDiscard);
    }

    private void OnClickDiscard()
    {
        TryDiscardDraggedItem();
    }

    public void OnDrop(PointerEventData eventData)
    {
        TryDiscardDraggedItem();
    }

    private void TryDiscardDraggedItem()
    {
        var drag = DragContext.Current;
        if (drag == null)
        {
            Debug.Log("[DiscardButton] 드래그 없음");
            return;
        }

        var inv = InventoryController.Instance?.inventory;
        var eq = InventoryController.Instance?.equipment;

        if (drag.source == DragSource.Inventory && inv != null && inv.IsValidIndex(drag.inventoryIndex))
        {
            var stack = inv.slots[drag.inventoryIndex];
            int count = stack.count;

            if (count <= 1)
            {
                inv.TakeAt(drag.inventoryIndex);
                inv.RaiseChanged();
                EndDrag();
            }
            else
            {
                var popup = DiscardQuantityPopup.EnsureInstance();
                if (popup != null)
                {
                    popup.Show(
                        max: count,
                        onConfirm: amt =>
                        {
                            Debug.Log($"[DiscardButton] 인벤토리 {amt}개 버림");
                            inv.SubtractAt(drag.inventoryIndex, amt);
                            inv.RaiseChanged();
                            EndDrag();
                        },
                        initialValue: 1
                    );
                }
                else
                {
                    Debug.LogWarning("[DiscardButton] DiscardQuantityPopup 인스턴스를 찾을 수 없어 전체 스택을 버립니다.");
                    inv.TakeAt(drag.inventoryIndex);
                    inv.RaiseChanged();
                    EndDrag();
                }
            }

            return;
        }

        if (drag.source == DragSource.Equipment && eq != null)
        {
            if (!eq.Discard(drag.equipSlot, out string reason))
                Debug.LogWarning($"[DiscardButton] 장비 버리기 실패: {reason}");

            EndDrag();
            slot?.Refresh();
            return;
        }

        Debug.Log("[DiscardButton] 처리 불가 드래그 상태");
    }

    private void EndDrag()
    {
        DragContext.End();
        InventoryDragHandler.Instance?.EndDrag();
    }
}
