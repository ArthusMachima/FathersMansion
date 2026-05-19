using UnityEngine;
using UnityEngine.Events;

public class ItemExchangeObject : MonoBehaviour, IInteractable
{
    [SerializeField] ItemClass itemOutput;
    [SerializeField] ItemClass itemRequired;
    [SerializeField] UnityEvent onExchangeSucess;
    [SerializeField] UnityEvent onExchangeFail;


    public void Interact()
    {
        ExchangeItem();
    }

    public void ExchangeItem()
    {
        //item check
        foreach (var item in InventoryManager.Instance.items)
            if (item.PeekItem() == itemRequired)
            {
                //result
                item.TakeItem();
                item.InsertItem(itemOutput);
                onExchangeSucess?.Invoke();
                return;
            }

        //fail indicator
        onExchangeFail?.Invoke();
    }
}
