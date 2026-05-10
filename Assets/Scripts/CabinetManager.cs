using System.Collections.Generic;
using UnityEngine;

public class CabinetManager : MonoBehaviour
{
    // Singleton
    public static CabinetManager Instance;
    private void OnEnable()
    {
        Instance = this;
    }


    [Header("Cabinet Panel")]
    public GameObject CabinetPanel;
    [SerializeField] Transform CabinetItemSlotList;
    [SerializeField] ItemSlot[] CabinetItemSlot;


    private void Start()
    {
        CabinetItemSlot = CabinetItemSlotList.GetComponentsInChildren<ItemSlot>();
        gameObject.SetActive(false);
    }


    public void ShowCabinet(bool show, List<ItemClass> items)
    {
        if (show)
        {
            if (items.Count > 4)
            {
                Debug.LogError("CABINET SHOULD ONLY HAVE FOUR ITEMS");
                return;
            }

            CabinetPanel.SetActive(true);
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null) CabinetItemSlot[i].PlaceItem(items[i]);
                else CabinetItemSlot[i].TakeItem();
            }
        }
        else
        {
            items.Clear();
            for (int i = 0; i < CabinetItemSlot.Length; i++)
            {
                if (CabinetItemSlot[i].HasItem()) items.Add(CabinetItemSlot[i].TakeItem());
            }
            PlayerControls.Instance.interactedObject = null;
            CabinetPanel.SetActive(false);
        }
    }

    public void ShowCabinet(bool show)
    {
        if (show)
        {

            CabinetPanel.SetActive(true);
        }
        else
        {
            PlayerControls.Instance.interactedObject = null;
            CabinetPanel.SetActive(false);
        }
    }
}
