using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

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
    [SerializeField] TextMeshProUGUI[] ItemText;
    [SerializeField] int inventoryPanelIndex;
    [SerializeField] GameObject SelectionMark;
    [SerializeField] List<ItemClass> items = new();

    public enum GameControlState
    {
        TopDownControls,
        InventoryPanel
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
        body = GetComponent<Rigidbody2D>();
        ItemText = List.GetComponentsInChildren<TextMeshProUGUI>();
        InventoryPanel.SetActive(false);
    }



    private void Update()
    {

        if (Input.GetKeyDown(OpenInventory))
        {
            if (!isInvertoryOpened)
            {
                InventoryPanel.gameObject.SetActive(true);
                gameControlState = GameControlState.InventoryPanel;
                RefreshInventoryPanel();
                isInvertoryOpened = true;
            }
            else
            {
                InventoryPanel.gameObject.SetActive(false);
                gameControlState = GameControlState.TopDownControls;
                isInvertoryOpened = false;
            }
        }



        if (gameControlState == GameControlState.TopDownControls)
        {

            //Keyboard Controls
            if (body != null)
            {
                up = Input.GetKey(MoveUp);
                left = Input.GetKey(MoveLeft);
                down = Input.GetKey(MoveDown);
                right = Input.GetKey(MoveRight);
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
                    PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
                    List<RaycastResult> results = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointerData, results);

                    bool exists = false;
                    foreach (RaycastResult result in results) if (result.gameObject == UIManager.Instance.ExaminePanel) exists = true;
                    if (exists == false) UIManager.Instance.CloseExamineGui(null);
                }
            }



        }
        else if (gameControlState == GameControlState.InventoryPanel)
        {

            if (Input.GetKeyDown(MoveUp))
            {
                MoveInventorySelection(MoveUp);
            }
            else 
            if (Input.GetKeyDown(MoveDown))
            {
                MoveInventorySelection(MoveDown);
            }

            if (Input.GetKeyDown(ActionPrimary))
            {
                items[inventoryPanelIndex].UseItem();
                items.Remove(items[inventoryPanelIndex]);
                RefreshInventoryPanel();
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
                body.AddForce(direction.normalized * speed* 2 * 10);
            else
                body.AddForce(direction.normalized * speed* 10);
        }
    }



    void MoveInventorySelection(KeyCode key)
    {
        if (key == MoveUp)
        {
            if (inventoryPanelIndex > 0)
            {
                inventoryPanelIndex--;
                SelectionMark.transform.parent = ItemText[inventoryPanelIndex].transform;
                SelectionMark.transform.localPosition = new(SelectionMark.transform.localPosition.x, 0, 0);
            }
        }
        else if (key == MoveDown)
        {
            if (inventoryPanelIndex < items.Count-1)
            {
                inventoryPanelIndex++;
                SelectionMark.transform.parent = ItemText[inventoryPanelIndex].transform;
                SelectionMark.transform.localPosition = new(SelectionMark.transform.localPosition.x, 0, 0);
            }
        }
    }

    void RefreshInventoryPanel()
    {
        SelectionMark.transform.localPosition = new(SelectionMark.transform.localPosition.x, 0, 0);
        for (int i = 0; i < ItemText.Length; i++)
        {
            if (i < items.Count && items[i] != null)
            {
                ItemText[i].gameObject.SetActive(true);
            }
            else
            {
                ItemText[i].gameObject.SetActive(false);
            }
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