using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class PlayerControls : MonoBehaviour
{
    [Header("Intialization")]
    Rigidbody2D body;
    public bool doMove = true;
    public bool doInteract = true;
    [SerializeField] float speed;
    [SerializeField] LayerMask excludePlayer;
    bool up, left, down, right;
    Vector3 direction = new(0, -1);
    Vector3 facingDirection = new(0, -1);

    [Header("Player Control")]
    public KeyCode MoveUp          = KeyCode.W;
    public KeyCode MoveLeft        = KeyCode.A;
    public KeyCode MoveDown        = KeyCode.S;
    public KeyCode MoveRight       = KeyCode.D;
    public KeyCode ActionPrimary   = KeyCode.F;           // Interaction, ect
    public KeyCode ActionSecondary = KeyCode.LeftShift;   // Run, Fast-forward dialogue, ect
    public KeyCode ActionThird     = KeyCode.Space;       // Pause, ect

    //Singleton
    public static PlayerControls Instance;
    private void OnEnable()
    {
        Instance = this;
    }

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }



    private void Update()
    {
        //Keyboard Controls
        if (body != null)
        {
            up    = Input.GetKey(MoveUp   );
            left  = Input.GetKey(MoveLeft );
            down  = Input.GetKey(MoveDown );
            right = Input.GetKey(MoveRight);
        }

        if (Input.GetKeyDown(ActionPrimary) && doInteract)
        {

            // Interactive object detection
            if (Physics2D.Raycast(transform.position, facingDirection, 1, ~excludePlayer) is RaycastHit2D hit && hit.collider != null) 
            {
                if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                {
                    interactable.Interact();
                }
            }
        }

        if (Input.GetKeyDown(ActionSecondary))
        {
            Debug.Log("Secondary Action");
        }

        if (Input.GetKeyDown(ActionThird))
        {
            Debug.Log("Third Action");
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

                bool exists=false;
                foreach (RaycastResult result in results) if (result.gameObject == UIManager.Instance.ExaminePanel) exists = true;
                if (exists == false) UIManager.Instance.CloseExamineGui(null);
            }
        }


    }



    //Movement
    private void FixedUpdate()
    {
        if (!doMove) return;

        if (body == null) return;
        direction = new(
            (right ? 1 : 0) - (left ? 1 : 0),
            (up    ? 1 : 0) - (down ? 1 : 0));

        if (up || left || down || right)
        {
            facingDirection = direction.normalized;
            body.AddForce(direction.normalized * speed * 10);
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