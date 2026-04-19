using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CabinetObject : MonoBehaviour, IInteractable
{
    public List<ItemClass> storedItems = new();


    public void Interact()
    {
        CabinetManager.Instance.ShowCabinet(true, storedItems);
        InventoryManager.Instance.OpenInventory(true, true);
    }


}
