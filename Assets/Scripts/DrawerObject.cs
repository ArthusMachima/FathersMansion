using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DrawerObject : MonoBehaviour, IInteractable
{
    public List<ItemClass> storedItems = new();
    [SerializeField] UnityEvent onInteract;
    [SerializeField] bool forceOpenDrawer;

    public void Interact()
    {
        if (onInteract.GetPersistentEventCount()>0 && !forceOpenDrawer)
        {
            onInteract.Invoke();
        } 
        else
        {
            CabinetManager.Instance.ShowCabinet(true, storedItems);
            InventoryManager.Instance.OpenInventory(true, true);
        }
    }

    public void DoForceOpen()
    {
        forceOpenDrawer=true;
        Interact();
    }
}
