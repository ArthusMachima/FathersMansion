using UnityEngine;

public class ItemGiver : MonoBehaviour
{
    [SerializeField] ItemClass itemToGive;
    [SerializeField] bool isKey;

    public void GiveItem()
    {
        InventoryManager.Instance.TransferItem(itemToGive, isKey);
    }
}
