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
    public bool isPlayerHiddenInCloset;

    public enum GameControlState
    {
        TopDownControls,
        InventoryPanel,
        SolvingPuzzle,
        HidingCloset
    }

    [Header("Player Controls")]
    public KeyCode MoveUp          = KeyCode.W;
    public KeyCode MoveLeft        = KeyCode.A;
    public KeyCode MoveDown        = KeyCode.S;
    public KeyCode MoveRight       = KeyCode.D;
    public KeyCode ActionPrimary   = KeyCode.F;
    public KeyCode ActionSecondary = KeyCode.LeftShift;
    public KeyCode ActionInventory = KeyCode.E;

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
        body = GetComponent<Rigidbody2D>();
    }


    private void Update()
    {
        if (!doPlayerControls) return;
        if (gameControlState == GameControlState.TopDownControls)
        {
            if (Input.GetKeyDown(ActionInventory) && !up && !left && !down && !right)
            {
                InventoryManager.Instance.OpenInventory(true, true);
                up = false; left = false; down = false; right = false;
            }

            //Keyboard Controls
            if (body != null)
            {
                if (Input.GetKey(MoveUp))    up   =true; else up   =false;
                if (Input.GetKey(MoveLeft))  left =true; else left =false;
                if (Input.GetKey(MoveDown))  down =true; else down =false;
                if (Input.GetKey(MoveRight)) right=true; else right=false;
            }

            if (Input.GetKeyDown(ActionPrimary))
            {
                var hits = Physics2D.RaycastAll(transform.position, facingDirection, 1, ~excludePlayer);

                foreach (var hit in hits)
                {
                    if (hit.collider == null || ((excludeMask.value & (1 << hit.collider.gameObject.layer)) != 0)) continue;

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

            }
        }
        else if (gameControlState == GameControlState.InventoryPanel)
        {
            if (Input.GetKeyDown(ActionInventory))
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
            if (Input.GetKeyDown(ActionPrimary))
            {
                ClosetHideMode(false);
            }
        }
    }


    // Physics-Based Movement
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
        }
        else
        {
            if (MonsterDistance==0)
            {
                //TODO: closet opens and gameover
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
