using System.Collections;
using UnityEngine;

public class ItemObject : MonoBehaviour, IInteractable
{
    public ItemClass item;
    [SerializeField] SpriteRenderer icon;
    [SerializeField] bool isKeyItem;

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
        icon = gameObject.GetComponent<SpriteRenderer>();
        icon.sprite = item.itemIcon;
    }


}
