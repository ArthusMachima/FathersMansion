using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    // Singleton
    public static InventoryManager Instance;
    private void OnEnable()
    {
        Instance = this;
    }


    [Header("Inventory")]
    [SerializeField] GameObject InventoryPanel;
    [SerializeField] GameObject ItemDescriptionPanel;
    [SerializeField] TextMeshProUGUI ItemDescriptionText;

    [Header("Pocket Panel")]
    [SerializeField] GameObject PocketPanel;
    [SerializeField] GameObject PocketList;
    [SerializeField] ItemSlot[] items;

    [Header("KeyItem Panel")]
    [SerializeField] GameObject KeyItemPanel;
    [SerializeField] GameObject KeyItemList;
    public KeyItemSlot[] keyItems;

    [Header("Item Drag")]
    public ItemClass heldItem;
    public Image draggedItem;
    [SerializeField] Canvas mainCanvas;
    [SerializeField] RectTransform canvasRectTransform;
    public ItemSlot prevSlot;
    public ItemSlot hoveredSlot;
    [SerializeField] LayerMask breakables;


    private void Start()
    {
        items = PocketList.GetComponentsInChildren<ItemSlot>();
        keyItems = KeyItemList.GetComponentsInChildren<KeyItemSlot>();
        mainCanvas = InventoryPanel.GetComponentInParent<Canvas>(true).rootCanvas;
        canvasRectTransform = mainCanvas.GetComponent<RectTransform>();
        draggedItem.gameObject.SetActive(false);
        InventoryPanel.SetActive(false);
        ItemDescriptionPanel.SetActive(false);
    }


    // ── Inventory Panel ────────────────────────────────────────────────────

    public void OpenInventory(bool open, bool notpuzzle)
    {
        if (open)
        {
            InventoryPanel.SetActive(true);
            ShowPocketPanel();
            if (notpuzzle) PlayerControls.Instance.gameControlState = PlayerControls.GameControlState.InventoryPanel;
            RefreshInventoryPanel();
        }
        else
        {
            if (heldItem != null)
            {
                prevSlot.PlaceItem(heldItem);
                heldItem = null;
                draggedItem.gameObject.SetActive(false);
                draggedItem.gameObject.LeanScale(Vector3.one, 0);
            }
            InventoryPanel.SetActive(false);
            if (notpuzzle) PlayerControls.Instance.gameControlState = PlayerControls.GameControlState.TopDownControls;
        }
    }

    void RefreshInventoryPanel()
    {
        foreach (ItemSlot slot in items) slot.RefreshSlot();
    }

    public void ShowPocketPanel()
    {
        PocketPanel.SetActive(false);
        KeyItemPanel.SetActive(false);
        PocketPanel.SetActive(true);
    }

    public void ShowKeyItemPanel()
    {
        PocketPanel.SetActive(false);
        KeyItemPanel.SetActive(false);
        KeyItemPanel.SetActive(true);
        foreach (KeyItemSlot slot in keyItems) slot.RefreshSlot();
    }


    // ── Items ──────────────────────────────────────────────────────────────

    public void TakeItem(ItemObject item)
    {
        foreach (ItemSlot slot in items)
        {
            if (!slot.HasItem())
            {
                SideScreenMessage.Instance.DisplayMessage("Obtained", item.item.itemName, 0.6f);
                slot.InsertItem(item.item);
                Destroy(item.gameObject);
                break;
            }
        }
    }

    public void TransferItemToKey(ItemClass item)
    {
        foreach (KeyItemSlot slot in keyItems)
        {
            if (!slot.HasItem())
            {
                SideScreenMessage.Instance.DisplayMessage("Obtained", item.itemName, 0.6f);
                slot.InsertItem(item);
                break;
            }
        }
    }


    // ── Drag ───────────────────────────────────────────────────────────────

    public void StartDragItem(ItemClass dragItem)
    {
        prevSlot = hoveredSlot;
        heldItem = dragItem;
        draggedItem.gameObject.SetActive(true);
        draggedItem.sprite = heldItem.itemIcon;
        StartCoroutine(DragItem());
    }

    IEnumerator DragItem()
    {
        if (heldItem is ItemClueClass)
        {
            draggedItem.gameObject.LeanScale(new(5, 5, 5), 0);
            draggedItem.sprite = (heldItem as ItemClueClass).clue;
        }

        while (Input.GetMouseButton(0))
        {
            if (!InventoryPanel.activeSelf)
            {
                prevSlot.PlaceItem(heldItem);
                heldItem = null;
                draggedItem.gameObject.SetActive(false);
                draggedItem.gameObject.LeanScale(Vector3.one, 0);
                yield break;
            }

            draggedItem.transform.localPosition = GetCursorPositionOnCanvas();
            yield return null;

            if (heldItem == null)
            {
                draggedItem.gameObject.LeanScale(Vector3.one, 0);
                yield break;
            }
        }

        draggedItem.gameObject.SetActive(false);

        if (heldItem is PuzzlePiece)
        {
            OnPuzzlePieceDropOnUI(heldItem);
        }
        else if (heldItem is ItemToolClass)
        {
            OnPuzzlePieceDrop(heldItem);
        }
        else
        {
            if (hoveredSlot != null)
            {
                if (hoveredSlot.HasItem()) prevSlot.PlaceItem(heldItem);
                else hoveredSlot.PlaceItem(heldItem);
            }
            else
            {
                prevSlot.PlaceItem(heldItem);
            }
        }

        if (heldItem is ItemClueClass)
        {
            draggedItem.gameObject.LeanScale(Vector3.one, 0);
            draggedItem.sprite = heldItem.itemIcon;
        }

        heldItem = null;
    }

    void OnPuzzlePieceDropOnUI(ItemClass item)
    {
        PointerEventData pointerData = new(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.TryGetComponent<ItemSlot>(out var itemSlot))
            {
                if (itemSlot.HasItem()) prevSlot.PlaceItem(heldItem);
                else itemSlot.PlaceItem(heldItem);
                return;
            }
            if (result.gameObject.TryGetComponent<PaintingPuzzleSlot>(out var slot))
            {
                if (slot.heldPiece == null)
                {
                    PuzzlePiece piece = (PuzzlePiece)item;
                    Instantiate(piece.puzzlePiecePrefab)
                        .GetComponent<PaintingPuzzlePiece>()
                        .PlaceOntoSlot(slot);
                }
                return;
            }
            if (result.gameObject.TryGetComponent<BreakableObject>(out var breakable))
            {
                breakable.BreakContainer();
            }
        }

        prevSlot.PlaceItem(heldItem);
    }

    void OnPuzzlePieceDrop(ItemClass item)
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, breakables);

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<BreakableObject>(out var breakable))
            {
                breakable.BreakContainer();
            }
        }

        prevSlot.PlaceItem(heldItem);
    }


    // ── Hover / Description ────────────────────────────────────────────────

    public void SetHoveredSlot(ItemSlot slot)
    {
        if (slot != null)
        {
            hoveredSlot = slot;
            if (hoveredSlot.HasItem())
            {
                ItemDescriptionPanel.SetActive(true);
                ItemDescriptionText.text = $"{slot.PeekItem().itemName} - {slot.PeekItem().itemDescription}";
            }
        }
    }

    public void SetHoveredSlot(KeyItemSlot slot)
    {
        if (slot != null)
        {
            ItemDescriptionPanel.SetActive(true);
            ItemDescriptionText.text = $"{slot.PeekItem().itemName} - {slot.PeekItem().itemDescription}";
        }
    }

    public void SetHoveredSlot()
    {
        ItemDescriptionText.text = "";
        ItemDescriptionPanel.SetActive(false);
    }


    // ── Utility ────────────────────────────────────────────────────────────

    public Vector2 GetCursorPositionOnCanvas()
    {
        Vector2 screenPosition = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            screenPosition,
            mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPosition
        );
        return localPosition;
    }
}
