using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ItemObject : MonoBehaviour, IInteractable
{
    public ItemClass item;
    [SerializeField] SpriteRenderer icon;
    [SerializeField] bool isKeyItem;
    [SerializeField] UnityEvent onInteract = new();
    [SerializeField] bool forceTake;
    [SerializeField] AudioManager aud;


    public ItemObject(ItemClass item)
    {
        this.item = item;
    }

    public void Interact()
    {
        if (onInteract != null && onInteract.GetPersistentEventCount() > 0 && !forceTake)
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
            aud.PlaySFX(aud.s_ItemPickUp);
        }
    }

    private void Start()
    {
        aud = AudioManager.Instance;
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
