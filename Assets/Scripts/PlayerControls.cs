
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [Header("Intialization")]
    Rigidbody2D body;
    public GameControlState gameControlState = GameControlState.TopDownControls;

    [Header("Movement")]
    public Animator anim;
    [SerializeField] bool isMoving;
    [SerializeField] bool isRunning;
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
    public bool isPlayerHiddenInCloset;

    [Header("Stamina")]
    [SerializeField] bool RegenStamina;
    [SerializeField] GameObject StaminaPanel;
    public float MaxStamina=100;
    public float Stamina;
    [SerializeField] Transform StaminaBar;


    public enum GameControlState
    {
        TopDownControls,
        InventoryPanel,
        SolvingPuzzle,
        HidingCloset
    }

    [Header("Player Controls")]
    public KeyCode MoveUp    = KeyCode.W;
    public KeyCode MoveLeft  = KeyCode.A;
    public KeyCode MoveDown  = KeyCode.S;
    public KeyCode MoveRight = KeyCode.D;
    public KeyCode Interact  = KeyCode.F;
    public KeyCode Run       = KeyCode.LeftShift;
    public KeyCode Inventory = KeyCode.E;

    [Header("Other Behavior")]
    public int MonsterDistance=5;


    // Singleton
    public static PlayerControls Instance;
    private void OnEnable()
    {
        Instance = this;
    }


    private void Start()
    {
        Stamina = 100;
        body = GetComponent<Rigidbody2D>();
    }


    private void Update()
    {
        if (!doPlayerControls) return;
        if (gameControlState == GameControlState.TopDownControls)
        {
            if (Input.GetKeyDown(Inventory) && !up && !left && !down && !right)
            {
                InventoryManager.Instance.OpenInventory(true, true);
                up = false; left = false; down = false; right = false;
            }

            if (body != null)
            {
                if (Input.GetKey(MoveUp))    up   =true; else up   =false;
                if (Input.GetKey(MoveLeft))  left =true; else left =false;
                if (Input.GetKey(MoveDown))  down =true; else down =false;
                if (Input.GetKey(MoveRight)) right=true; else right=false;
            }

            if (Input.GetKeyDown(Interact))
            {
                var hits = Physics2D.RaycastAll(transform.position, facingDirection, 1, ~excludePlayer);

                foreach (var hit in hits)
                {
                    if (hit.collider == null || ((excludeMask.value & (1 << hit.collider.gameObject.layer)) != 0)) continue;

                    if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                    {
                        interactedObject = interactable;
                        up=false; left=false; down=false; right=false;
                        anim.SetBool("isMoving", false);
                        interactable.Interact();
                        break;
                    }
                }
            }

            if (Input.GetKey(Run))
            {
                isRunning = isMoving;
                RegenStamina = false;
            }

            if (Input.GetKeyUp(Run))
            {
                isRunning = false;
                LeanTween.delayedCall(0.5f, () =>
                {
                    RegenStamina = true;
                });
            }

            if (Stamina < MaxStamina && RegenStamina)
            {
                Stamina++;
            }

            if (StaminaBar!=null) StaminaBar.localScale = new((float)(Stamina / MaxStamina), 1, 1);

            if (Stamina==MaxStamina)
                StaminaPanel.SetActive(false);
            else
                StaminaPanel.SetActive(true);


        }
        else if (gameControlState == GameControlState.InventoryPanel)
        {
            if (Input.GetKeyDown(Inventory))
            {
                InventoryManager.Instance.OpenInventory(false, true);
                if (interactedObject is DrawerObject openedCabinet && interactedObject != null)
                {
                    CabinetManager.Instance.ShowCabinet(false, openedCabinet.storedItems);
                }
            }

            if (Input.GetKeyDown(MoveUp)   ||
                Input.GetKeyDown(MoveLeft) ||
                Input.GetKeyDown(MoveDown) ||
                Input.GetKeyDown(MoveRight))
            {
                InventoryManager.Instance.OpenInventory(false, true);
                if (interactedObject is DrawerObject openedCabinet && interactedObject != null)
                {
                    CabinetManager.Instance.ShowCabinet(false, openedCabinet.storedItems);
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
                if (currentInteractedPuzzle != null)
                    UIManager.Instance.CurrentPuzzlePanel.OnPuzzleExit();

                UIManager.Instance.ShowPuzzlePanel();
                PuzzleMode(false); // handles both doPlayerControls and gameControlState
            }
        }
        else if (gameControlState == GameControlState.HidingCloset)
        {
            if (Input.GetKeyDown(Interact))
            {
                ClosetHideMode(false);
            }
        }


        
    }


    // Physics-Based Movement
    private void FixedUpdate()
    {
        if (!doPlayerControls) return;

        //Set Move Direction
        if (body == null) return;
        direction = new(
            (right ? 1 : 0) - (left ? 1 : 0),
            (up    ? 1 : 0) - (down ? 1 : 0));
        isMoving = direction.magnitude > 0.1f || direction.magnitude < -0.1f;


        //Animation
        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isRunning", isRunning);
        if (isMoving)
        {
            anim.SetFloat("x", direction.x);
            anim.SetFloat("y", direction.y);
        }

        //Apply Movement
        if (up || left || down || right)
        {
            facingDirection = direction.normalized;
            if (isRunning && 0 < Stamina)
            {
                body.AddForce(10 * 2 * speed * direction.normalized); // Run
                Stamina--;
            }
            else
            {
                body.AddForce(10 * speed * direction.normalized); // Walk
            }
        }
    }


    // Utility
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

    public void ClosetHideMode(bool hide)
    {
        if (hide)
        {
            isPlayerHiddenInCloset = true;
            gameControlState = GameControlState.HidingCloset;
            GameManager.Instance.Monster.agent.isStopped = true;
        }
        else
        {
            if (MonsterDistance==0)
            {
                GameManager.Instance.Jumpscare();
            }
            isPlayerHiddenInCloset = false;
            doPlayerControls = true;
            gameControlState = GameControlState.TopDownControls;
            HideClosetBehavior.Instance.gameObject.SetActive(false);
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
