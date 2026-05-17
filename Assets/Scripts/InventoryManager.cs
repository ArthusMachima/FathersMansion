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
    [SerializeField] GameObject KeyItemDescriptionPanel;
    [SerializeField] TextMeshProUGUI KeyItemDescriptionText;
    [SerializeField] Image ClueDisplay;
    [SerializeField] GameObject PocketList;
    public ItemSlot[] items;
    [SerializeField] GameObject KeyItemList;
    public KeyItemSlot[] keyItems;

    [Header("Item Drag")]
    public ItemClass heldItem;
    public Image draggedItem;
    [SerializeField] Canvas mainCanvas;
    [SerializeField] RectTransform canvasRectTransform;
    public ItemSlot prevSlot;
    public ItemSlot hoveredSlot;
    [SerializeField] bool isHoveredOverSlot;
    [SerializeField] LayerMask breakables;
    [SerializeField] AudioManager aud;


    private void Start()
    {
        aud = AudioManager.Instance;
        items = PocketList.GetComponentsInChildren<ItemSlot>();
        keyItems = KeyItemList.GetComponentsInChildren<KeyItemSlot>();
        mainCanvas = InventoryPanel.GetComponentInParent<Canvas>(true).rootCanvas;
        canvasRectTransform = mainCanvas.GetComponent<RectTransform>();
        draggedItem.gameObject.SetActive(false);
        InventoryPanel.SetActive(false);
        ItemDescriptionPanel.SetActive(false);
        KeyItemDescriptionPanel.SetActive(false);
        ClueDisplay.gameObject.SetActive(false);
    }


    // Inventory Panel
    public void OpenInventory(bool open, bool notpuzzle)
    {
        if (open)
        {
            InventoryPanel.SetActive(true);
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
        foreach (KeyItemSlot slot in keyItems) slot.RefreshSlot();
    }

    public void TakeItem(ItemObject item)
    {
        foreach (ItemSlot slot in items)
        {
            if (!slot.HasItem())
            {
                slot.InsertItem(item.item);
                Destroy(item.gameObject);
                return;
            }
            
        }

        Dialogue[] msg = { new("I can't take anymore items.", null) };
        UIManager.Instance.LoadDialogue(msg);
    }

    public void TransferItem(ItemClass item, bool isKey)
    {
        if (isKey)
        {
            foreach (KeyItemSlot slot in keyItems)
            {
                if (!slot.HasItem())
                {
                    slot.InsertItem(item);
                    break;
                }
            }
        }
        else
        {
            foreach (ItemSlot slot in items)
            {
                if (!slot.HasItem())
                {
                    SideScreenMessage.Instance.DisplayMessage("Obtained", item.itemName, 0.6f);
                    slot.InsertItem(item);
                    break;
                }
            }
        }
    }

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
        aud.PlaySFX(aud.s_Pick);
        while (Input.GetMouseButton(0) || Input.GetMouseButton(1))
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
        aud.PlaySFX(aud.s_Place);

        draggedItem.gameObject.SetActive(false);

        if (hoveredSlot != null)
        {
            if (isHoveredOverSlot)
            {
                if (hoveredSlot.HasItem()) prevSlot.PlaceItem(heldItem);
                else hoveredSlot.PlaceItem(heldItem);
            }
            else
            {
                if (heldItem is PuzzlePiece) OnPuzzlePieceDropOnUI(heldItem);
                else if (heldItem is ItemToolClass) OnItemDrop();
                else prevSlot.PlaceItem(heldItem);
            }
        }
        else prevSlot.PlaceItem(heldItem);



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

    void OnItemDrop()
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, breakables);
        

        if (hit.collider != null)
        {
            Debug.Log(hit.collider.gameObject.name);

            if (hit.collider.TryGetComponent<BreakableObject>(out var breakable))
            {
                breakable.BreakContainer();
            }
        }
        else
        {
            Debug.Log("empty");
        }

        prevSlot.PlaceItem(heldItem);
    }


    // Hover functions
    public void SetHoveredSlot(ItemSlot slot)
    {
        if (slot != null)
        {
            hoveredSlot = slot;
            isHoveredOverSlot = true;
            if (hoveredSlot.HasItem())
            {
                ItemDescriptionPanel.SetActive(true);
                if (hoveredSlot.PeekItem() is MysteryItemClass item)
                {
                    if (item.isRealized) ItemDescriptionText.text = $"{item.realName} - {item.realDescription}";
                    else ItemDescriptionText.text = $"{item.itemName} - {item.itemDescription}";
                }
                else ItemDescriptionText.text = $"{slot.PeekItem().itemName} - {slot.PeekItem().itemDescription}";

                if (hoveredSlot.PeekItem() is ItemClueClass clue)
                {
                    ClueDisplay.gameObject.SetActive(true);
                    if (GameManager.Instance.colorblindMode && clue.clueColorblindAlt!=null)
                        ClueDisplay.sprite = clue.clueColorblindAlt;
                    else ClueDisplay.sprite = clue.clue;
                }
            }
        }
    }

    public void SetHoveredSlot(KeyItemSlot slot)
    {
        if (slot != null)
        {
            KeyItemDescriptionPanel.SetActive(true);
            if (hoveredSlot.PeekItem() is MysteryItemClass item)
            {
                if (item.isRealized) KeyItemDescriptionText.text = $"{item.realName} - {item.realDescription}";
                else KeyItemDescriptionText.text = $"{item.itemName} - {item.itemDescription}";
            }
            else KeyItemDescriptionText.text = $"{slot.PeekItem().itemName} - {slot.PeekItem().itemDescription}";
        }
    }

    public void SetHoveredSlot()
    {
        ItemDescriptionText.text = "";
        ItemDescriptionPanel.SetActive(false);
        KeyItemDescriptionText.text = "";
        KeyItemDescriptionPanel.SetActive(false);
        ClueDisplay.gameObject.SetActive(false);
        isHoveredOverSlot = false;
    }


    // Utility
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
