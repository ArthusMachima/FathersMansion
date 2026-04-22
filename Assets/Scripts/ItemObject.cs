using System.Collections;
using UnityEngine;

public class ItemObject : MonoBehaviour, IInteractable
{
    public ItemClass item;
    [SerializeField] SpriteRenderer icon;
    [SerializeField] bool isKeyItem;


    public ItemObject(ItemClass item)
    {
        this.item = item;
    }

    public void Interact()
    {
        if (!isKeyItem)
        {
            InventoryManager.Instance.TakeItem(this);
        }
        else
        {
            InventoryManager.Instance.TransferItemToKey(item);
            Destroy(gameObject);
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


}
