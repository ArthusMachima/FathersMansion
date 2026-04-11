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
    [SerializeField] GameControlState gameControlState = GameControlState.TopDownControls;

    [Header("Movement")]
    public bool doPlayerControls=true;
    [SerializeField] float speed;
    bool up, left, down, right;
    Vector3 direction = new(0, -1);

    [Header("Interaction")]
    Vector3 facingDirection = new(0, -1);
    [SerializeField] LayerMask excludeMask;
    [SerializeField] LayerMask excludePlayer;

    [Header("Inventory")]
    [SerializeField] GameObject InventoryPanel;
    [SerializeField] bool isInvertoryOpened = false;
    [SerializeField] GameObject List;
    [SerializeField] ItemSlot[] items;

    [Header("Item Drag")]
    [SerializeField] ItemClass heldItem;
    [SerializeField] Image draggedItem;
    [SerializeField] Canvas mainCanvas;
    [SerializeField] RectTransform canvasRectTransform;
    public ItemSlot hoveredSlot;


    public enum GameControlState
    {
        TopDownControls,
        InventoryPanel,
        DraggingItem
    }

    [Header("Player Controls")]
    public KeyCode MoveUp          = KeyCode.W;
    public KeyCode MoveLeft        = KeyCode.A;
    public KeyCode MoveDown        = KeyCode.S;
    public KeyCode MoveRight       = KeyCode.D;
    public KeyCode ActionPrimary   = KeyCode.F;           // Interaction, ect
    public KeyCode ActionSecondary = KeyCode.LeftShift;   // Run, Fast-forward dialogue, ect
    public KeyCode OpenInventory   = KeyCode.E;



    //Singleton
    public static PlayerControls Instance;
    private void OnEnable()
    {
        Instance = this;
    }



    private void Start()
    {
        items = List.GetComponentsInChildren<ItemSlot>();
        body = GetComponent<Rigidbody2D>();
        InventoryPanel.SetActive(false);
        canvasRectTransform = mainCanvas.GetComponent<RectTransform>();
        draggedItem.gameObject.SetActive(false);
    }



    private void Update()
    {

        if (Input.GetKeyDown(OpenInventory))
        {
            if (!isInvertoryOpened)
            {
                InventoryPanel.SetActive(true);
                gameControlState = GameControlState.InventoryPanel;
                RefreshInventoryPanel();
                isInvertoryOpened = true;
                up = false; left = false; down = false; right = false;
            }
            else
            {
                InventoryPanel.SetActive(false);
                gameControlState = GameControlState.TopDownControls;
                isInvertoryOpened = false;
            }
        }



        if (gameControlState == GameControlState.TopDownControls)
        {

            //Keyboard Controls
            if (body != null)
            {
                if (Input.GetKey(MoveUp))    up   =true; else up   =false;
                if (Input.GetKey(MoveLeft))  left =true; else left =false;
                if (Input.GetKey(MoveDown))  down =true; else down =false;
                if (Input.GetKey(MoveRight)) right=true; else right=false;
            }

            if (Input.GetKeyDown(ActionPrimary) && doPlayerControls)
            {
                var hits = Physics2D.RaycastAll(transform.position, facingDirection, 1, ~excludePlayer);

                foreach (var hit in hits)
                {
                    if (hit.collider == null || ((excludeMask.value & (1 << hit.collider.gameObject.layer)) != 0)) continue;

                    if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                    {
                        interactable.Interact();
                        break;
                    }
                }
            }

            if (Input.GetKeyDown(ActionSecondary))
            {

            }

            //Cursor Controls
            if (Input.GetMouseButtonDown(0))
            {




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
            }



        }
        else if (gameControlState == GameControlState.InventoryPanel)
        {

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
    void RefreshInventoryPanel()
    {
        foreach (ItemSlot slot in items) slot.RefreshSlot();
    }

    public void TakeItem(ItemObject item)
    {
        foreach (ItemSlot slot in items)
        {
            if (slot.storedItem == null)
            {
                slot.storedItem = item.item;
                Destroy(item.gameObject);
                break;
            }
        }
    }

    public void StartDragItem(ItemClass dragItem)
    {
        heldItem = dragItem;
        draggedItem.gameObject.SetActive(true);
        draggedItem.sprite = heldItem.itemIcon; //line 232
        StartCoroutine(DragItem());
    }

    IEnumerator DragItem()
    {
        while (Input.GetMouseButton(0))
        {
            draggedItem.transform.localPosition = GetCursorPositionOnCanvas();
            yield return null;
        }

        draggedItem.gameObject.SetActive(false);
        yield return null;
        hoveredSlot.storedItem = heldItem;
        hoveredSlot.RefreshSlot();
        yield return null;
        heldItem = null;
    }

    public Vector2 GetCursorPositionOnCanvas()
    {
        Vector2 screenPosition = Input.mousePosition;
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            screenPosition,
            mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out localPosition
        );
        return localPosition;
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