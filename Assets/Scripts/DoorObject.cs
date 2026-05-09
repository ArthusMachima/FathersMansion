using UnityEngine;
using UnityEngine.Events;

public class DoorObject : MonoBehaviour, IInteractable
{
    [SerializeField] AudioManager aud;
    [SerializeField] BoxCollider2D Collider;
    public LockDirection currentLockDirection;
    public SpriteRenderer sprite;
    public Sprite openSprite;
    public Sprite closeSprite;
    [SerializeField] bool isUnlocked;
    [SerializeField] ItemClass requiredKeys;
    [SerializeField] UnityEvent onDoorLocked;
    [SerializeField] DoorObject connectedDoor;

    private void Start()
    {
        aud = AudioManager.Instance;
        sprite = GetComponent<SpriteRenderer>();
    }

    public enum LockDirection
    {
        Up,
        Left,
        Down,
        Right
    }


    public void Interact()
    {
        OpenDoor(true);
    }

    public void OpenDoor(bool open)
    {
        if (open)
        {
            if (!isUnlocked)
            {
                Vector3 offset = transform.position - PlayerControls.Instance.transform.position;
                bool isLockedSide = currentLockDirection switch
                {
                    LockDirection.Up    => offset.y-1 < 0,
                    LockDirection.Down  => offset.y-1 > 0,
                    LockDirection.Right => offset.x   < 0,
                    LockDirection.Left  => offset.x   > 0,
                    _ => false
                };
                if (isLockedSide)
                {
                    TryUnlockingDoor();
                    return;
                }
                UnlockDoor();
            }

            Collider.enabled = false;
            sprite.sprite = openSprite;
            aud.PlaySFX(aud.sfxOpen);
        }
        else
        {
            Collider.enabled = true;
            sprite.sprite = closeSprite;
            aud.PlaySFX(aud.sfxClose);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        OpenDoor(false);
    }

    public void TryUnlockingDoor()
    {
        foreach (var slot in InventoryManager.Instance.keyItems)
        {
            if (requiredKeys == null) break;
            if (slot.storedItem == requiredKeys)
            {
                //slot.TakeItem();
                UnlockDoor();
                return;
            }
        }

        aud.PlaySFX(aud.sfxLocked);
        Dialogue[] msg = new Dialogue[1];
        if (requiredKeys!=null)
        {
            msg = new Dialogue[] { new("The door is locked from the other side.", null),
                                   new("I might need a key for this.", null)};
        }
        else
        {
            msg = new Dialogue[] { new("The door is locked from the other side.", null) };
        }
        if (onDoorLocked.GetPersistentEventCount()==0) UIManager.Instance.LoadDialogue(msg);
        else onDoorLocked.Invoke();
    }

    public void UnlockDoor()
    {
        isUnlocked = true;
        if (connectedDoor!=null) connectedDoor.isUnlocked = true;
        aud.PlaySFX(aud.sfxUnlock);
        Dialogue[] msg = { new("I unlocked the door.", null) };
        UIManager.Instance.LoadDialogue(msg);
    }
}
