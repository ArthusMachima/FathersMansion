using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CabinetObject : MonoBehaviour, IInteractable
{
    public List<ItemClass> storedItems = new();


    public void Interact()
    {
        UIManager.Instance.ShowCabinet(true, storedItems);
        PlayerControls.Instance.OpenInventory(true, true);
    }


}
