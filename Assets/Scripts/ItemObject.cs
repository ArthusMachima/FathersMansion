using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ItemObject : MonoBehaviour, IInteractable
{
    public ItemClass item;
    [SerializeField] SpriteRenderer icon;
    [SerializeField] bool isKeyItem;
    [SerializeField] UnityEvent onInteract;
    [SerializeField] bool forceTake;


    public ItemObject(ItemClass item)
    {
        this.item = item;
    }

    public void Interact()
    {
        if (onInteract.GetPersistentEventCount()>0 && !forceTake)
        {
            onInteract.Invoke();
        }
        else
        {
            if (!isKeyItem)
            {
                InventoryManager.Instance.TakeItem(this);
            }
            else
            {
                InventoryManager.Instance.TransferItem(item, true);
                Destroy(gameObject);
            }
        }
    }

    private void Start()
    {
        RefreshObject();
    }

    public void RefreshObject()
    {
        icon = gameObject.GetComponent<SpriteRenderer>();
        if (icon!=null) icon.sprite = item.itemIcon;
    }


    public void DoForceTake()
    {
        forceTake = true;
        Interact();
    }
}
