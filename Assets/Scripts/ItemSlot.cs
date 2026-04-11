using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] ItemClass storedItem;
    [SerializeField] Image slotIcon;

    private void Start()
    {
        slotIcon = transform.GetChild(0).GetComponent<Image>();
        RefreshSlot();
    }

    public void RefreshSlot()
    {
        if (storedItem!=null)
        {
            slotIcon.enabled = true;
            slotIcon.sprite = storedItem.itemIcon;
        }
        else
        {
            slotIcon.enabled = false;
        }
    }

    public bool HasItem()
    {
        return storedItem!=null;
    }

    public void PlaceItem(ItemClass item)
    {
        storedItem = item;
        slotIcon.enabled = true;
        slotIcon.sprite = storedItem.itemIcon;
    }

    public void InsertItem(ItemClass item)
    {
        storedItem = item;
    }

    public ItemClass TakeItem()
    {
        slotIcon.enabled = false;
        ItemClass item = storedItem;
        storedItem = null;
        return item;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayerControls.Instance.hoveredSlot = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Hide item description
        //Debug.Log("Exit Slot");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (storedItem!=null)
        {
            PlayerControls.Instance.StartDragItem(storedItem);
            storedItem = null;
            RefreshSlot();
        }
    }
}
