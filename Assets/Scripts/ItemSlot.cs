using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] ItemClass storedItem;
    [SerializeField] Image slotIcon;

    private void Awake()
    {
        slotIcon = transform.GetChild(0).GetComponent<Image>();
    }

    private void Start()
    {
        RefreshSlot();
    }



    //Slot Functions
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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Right Click Function");
            return;
        }

        if (storedItem!=null)
        {
            InventoryManager.Instance.StartDragItem(storedItem);
            storedItem = null;
            RefreshSlot();
        }
    }
}
