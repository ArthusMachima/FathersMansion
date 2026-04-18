using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerControls : MonoBehaviour
{
    [Header("Intialization")]
    Rigidbody2D body;
    public GameControlState gameControlState = GameControlState.TopDownControls;

    [Header("Movement")]
    public bool doPlayerControls=true;
    [SerializeField] float speed;
    bool up, left, down, right;
    Vector3 direction = new(0, -1);

    [Header("Interaction")]
    Vector3 facingDirection = new(0, -1);
    [SerializeField] LayerMask excludeMask;
    [SerializeField] LayerMask excludePlayer;
    public IInteractable interactedObject;
    public PuzzleObject currentInteractedPuzzle;

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
    [SerializeField] KeyItemSlot[] keyItems;

    [Header("Item Drag")]
    public ItemClass heldItem;
    public Image draggedItem;
    [SerializeField] Canvas mainCanvas;
    [SerializeField] RectTransform canvasRectTransform;
    public ItemSlot prevSlot;
    public ItemSlot hoveredSlot;


    public enum GameControlState
    {
        TopDownControls,
        InventoryPanel,
        SolvingPuzzle
    }

    [Header("Player Controls")]
    public KeyCode MoveUp          = KeyCode.W;
    public KeyCode MoveLeft        = KeyCode.A;
    public KeyCode MoveDown        = KeyCode.S;
    public KeyCode MoveRight       = KeyCode.D;
    public KeyCode ActionPrimary   = KeyCode.F;           // Interaction, ect
    public KeyCode ActionSecondary = KeyCode.LeftShift;   // Run, Fast-forward dialogue, ect
    public KeyCode ActionInventory = KeyCode.E;



    //Singleton
    public static PlayerControls Instance;
    private void OnEnable()
    {
        Instance = this;
    }


    private void Start()
    {
        items = PocketList.GetComponentsInChildren<ItemSlot>();
        keyItems = KeyItemList.GetComponentsInChildren<KeyItemSlot>();
        body = GetComponent<Rigidbody2D>();
        canvasRectTransform = mainCanvas.GetComponent<RectTransform>();
        draggedItem.gameObject.SetActive(false);
        InventoryPanel.SetActive(false);
        ItemDescriptionPanel.SetActive(false);
    }



    private void Update()
    {
        if (!doPlayerControls) return;
        if (gameControlState == GameControlState.TopDownControls)
        {
            if (Input.GetKeyDown(ActionInventory) && !up && !left && !down && !right)
            {
                OpenInventory(true, true);
                up = false; left = false; down = false; right = false;
            }

            //Keyboard Controls
            if (body != null)
            {
                if (Input.GetKey(MoveUp))    up   =true; else up   =false;
                if (Input.GetKey(MoveLeft))  left =true; else left =false;
                if (Input.GetKey(MoveDown))  down =true; else down =false;
                if (Input.GetKey(MoveRight)) right=true; else right=false;
            } //Movement

            if (Input.GetKeyDown(ActionPrimary))
            {
                var hits = Physics2D.RaycastAll(transform.position, facingDirection, 1, ~excludePlayer);

                foreach (var hit in hits)
                {
                    //Ignore if it's not interactable
                    if (hit.collider == null || ((excludeMask.value & (1 << hit.collider.gameObject.layer)) != 0)) continue; 

                    //If interactable
                    if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                    {
                        interactedObject = interactable;
                        interactable.Interact();
                        break;
                    }
                }
            }

            if (Input.GetKeyDown(ActionSecondary))
            {

            } //empty

            //Cursor Controls
            if (Input.GetMouseButtonDown(0)) 
            {



                /*
                // Exiting ExaminePanel by pressing anywhere outside the panel
                if (UIManager.Instance.isExaminePanelShown)
                {
                    PointerEventData pointerData = new(EventSystem.current) { position = Input.mousePosition };
                    List<RaycastResult> results = new();
                    EventSystem.current.RaycastAll(pointerData, results);

                    bool exists = false;
                    foreach (RaycastResult result in results) if (result.gameObject == UIManager.Instance.ExaminePanel) exists = true;
                    if (exists == false) UIManager.Instance.CloseExamineGui(null);
                }
                */
            } //empty
        }
        else if (gameControlState == GameControlState.InventoryPanel)
        {
            if (Input.GetKeyDown(ActionInventory))
            {
                OpenInventory(false, true);
                if (interactedObject is CabinetObject openedCabinet && interactedObject!=null)
                {
                    UIManager.Instance.ShowCabinet(false, openedCabinet.storedItems);
                }
            }

            if (Input.GetKeyDown(MoveUp)   ||
                Input.GetKeyDown(MoveLeft) ||
                Input.GetKeyDown(MoveDown) ||
                Input.GetKeyDown(MoveRight))
            {
                OpenInventory(false, true);
                if (interactedObject is CabinetObject openedCabinet && interactedObject != null)
                {
                    UIManager.Instance.ShowCabinet(false, openedCabinet.storedItems);
                }
            }


        }
        else if (gameControlState == GameControlState.SolvingPuzzle)
        {
            if (Input.GetKeyDown(MoveUp) ||
                Input.GetKeyDown(MoveLeft) ||
                Input.GetKeyDown(MoveDown) ||
                Input.GetKeyDown(MoveRight))
            {
                doPlayerControls = false;
                UIManager.Instance.ShowPuzzlePanel();
            }
        }
    }



    //Physics-Based Movement
    private void FixedUpdate()
    {
        if (!doPlayerControls) return;

        if (body == null) return;
        direction = new(
            (right ? 1 : 0) - (left ? 1 : 0),
            (up    ? 1 : 0) - (down ? 1 : 0));

        if (up || left || down || right)
        {
            facingDirection = direction.normalized;
            if (Input.GetKey(ActionSecondary))
                body.AddForce(10 * 2 * speed * direction.normalized);
            else
                body.AddForce(10 * speed * direction.normalized);
        }
    }



    //Inventory
    public void OpenInventory(bool open, bool notpuzzle)
    {
        if (open)
        {
            InventoryPanel.SetActive(true);
            ShowPocketPanel();
            if (notpuzzle) gameControlState = GameControlState.InventoryPanel;
            RefreshInventoryPanel();
        }
        else
        {
            InventoryPanel.SetActive(false);
            if (notpuzzle) gameControlState = GameControlState.TopDownControls;
        }
    }

    void RefreshInventoryPanel()
    {
        foreach (ItemSlot slot in items) slot.RefreshSlot();
    }

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
        while (Input.GetMouseButton(0))
        {
            draggedItem.transform.localPosition = GetCursorPositionOnCanvas();
            yield return null;
            if (heldItem==null) yield break;
        }

        draggedItem.gameObject.SetActive(false);
        if (hoveredSlot.HasItem()) prevSlot.PlaceItem(heldItem);
        else hoveredSlot.PlaceItem(heldItem);
        heldItem = null;
    }

    public void SetHoveredSlot(ItemSlot slot)
    {
        if (slot!=null)
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



    //Utility
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

    public void PuzzleMode(bool show)
    {
        if (show)
        {
            gameControlState = GameControlState.SolvingPuzzle;
        }
        else
        {
            doPlayerControls = true;
            gameControlState = GameControlState.TopDownControls;
        }
    }



    void OnDrawGizmos()
    {
        if (Physics2D.Raycast(transform.position, facingDirection, 1, ~excludePlayer) is RaycastHit2D hit && hit.collider != null)
        {
            Gizmos.color = Color.yellow;
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                Gizmos.color = Color.green;
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    interactable.Interact();
                }
            }
        }
        else Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, facingDirection * 1f);
    }
}