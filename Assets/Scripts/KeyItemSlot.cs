using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TextMeshProUGUI slotText;
    [SerializeField] Image slotIcon;
    [SerializeField] ItemClass storedItem;



    // Slot function
    public void RefreshSlot()
    {
        if (storedItem != null)
        {
            slotIcon.enabled = true;
            slotText.enabled = true;
            slotText.text = storedItem.itemName;
            slotIcon.sprite = storedItem.itemIcon;
        }
        else
        {
            slotIcon.enabled = false;
            slotText.enabled = false;
        }
    }

    public bool HasItem()
    {
        return storedItem != null;
    }

    public void InsertItem(ItemClass item)
    {
        storedItem = item;
    }

    public void PlaceItem(ItemClass item)
    {
        storedItem = item;
        slotIcon.enabled = true;
        slotIcon.sprite = storedItem.itemIcon;
        slotText.enabled = true;
        slotText.text = storedItem.itemName;
    }

    public ItemClass TakeItem()
    {
        slotIcon.enabled = false;
        slotText.enabled = false;
        ItemClass item = storedItem;
        storedItem = null;
        return item;
    }

    public ItemClass PeekItem()
    {
        return storedItem;
    }



    //Mouse functions
    public void OnPointerEnter(PointerEventData eventData)
    {
        InventoryManager.Instance.SetHoveredSlot(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryManager.Instance.SetHoveredSlot();
    }
}
