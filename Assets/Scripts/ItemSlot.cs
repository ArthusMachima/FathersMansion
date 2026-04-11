using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public ItemClass storedItem;
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
