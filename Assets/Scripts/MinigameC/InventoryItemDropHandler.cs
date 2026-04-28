using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attached to each item display in the inventory UI.
/// Right-clicking drops one instance of the item from the inventory.
/// </summary>
public class InventoryItemDropHandler : MonoBehaviour, IPointerClickHandler
{
    public int itemId;
    public InventoryController controller;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && controller != null)
        {
            controller.DropItem(itemId);
            MinigameCAudioManager.PlayItemDrop();
        }
    }
}
