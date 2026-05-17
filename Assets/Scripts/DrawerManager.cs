using System.Collections.Generic;
using UnityEngine;
public class DrawerManager : MonoBehaviour
{
    // Singleton
    public static DrawerManager Instance;
    private void OnEnable()
    {
        Instance = this;
    }
    [Header("Drawer Panel")]
    public CanvasGroup DrawerPanel;
    [SerializeField] Transform DrawerItemSlotList;
    [SerializeField] ItemSlot[] DrawerItemSlot;
    private void Start()
    {
        DrawerItemSlot = DrawerItemSlotList.GetComponentsInChildren<ItemSlot>();
    }
    public void ShowDrawer(bool show, List<ItemClass> items)
    {
        if (show)
        {
            if (items.Count > 4)
            {
                Debug.LogError("DRAWER SHOULD ONLY HAVE FOUR ITEMS");
                return;
            }
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null) DrawerItemSlot[i].PlaceItem(items[i]);
                else DrawerItemSlot[i].TakeItem();
            }
        }
        else
        {
            items.Clear();
            for (int i = 0; i < DrawerItemSlot.Length; i++)
            {
                if (DrawerItemSlot[i].HasItem()) items.Add(DrawerItemSlot[i].TakeItem());
            }
            PlayerControls.Instance.interactedObject = null;
        }

        LeanTween.cancel(gameObject);
        LeanTween.value(gameObject, DrawerPanel.alpha, show?1:0, 0.3f)
                    .setOnUpdate(val => DrawerPanel.alpha = val);
        DrawerPanel.interactable = show;
        DrawerPanel.blocksRaycasts = show;
    }
    public void ShowDrawer(bool show)
    {
        if (!show) PlayerControls.Instance.interactedObject = null;

        LeanTween.cancel(gameObject);
        LeanTween.value(gameObject, DrawerPanel.alpha, show ? 1 : 0, 0.3f)
                    .setOnUpdate(val => DrawerPanel.alpha = val);
        DrawerPanel.interactable = show;
        DrawerPanel.blocksRaycasts = show;
    }
}