using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PadlockPuzzle : PuzzleClass, IPointerEnterHandler
{
    [SerializeField] Rigidbody2D lockPhys;
    [SerializeField] PuzzleObject puzzleObject;
    [SerializeField] ItemClass correctItem;
    [SerializeField] Sprite unlockedSprite;
    [SerializeField] Image sprite;
    [SerializeField] AudioManager aud;


    private void Start()
    {
        aud = AudioManager.Instance;
        puzzleObject = PlayerControls.Instance.currentInteractedPuzzle;
        if (puzzleObject.isPuzzleFinished) sprite.sprite = unlockedSprite;
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        if (puzzleObject.isPuzzleFinished) return;
        if (InventoryManager.Instance.heldItem == correctItem)
        {
            InventoryManager.Instance.heldItem = null;
            InventoryManager.Instance.draggedItem.gameObject.SetActive(false);
            Unlock();
        }
    }



    void Unlock()
    {
        aud.PlaySFX(aud.s_Padlock);
        sprite.sprite = unlockedSprite;
        lockPhys.gravityScale = 150;
        lockPhys.bodyType = RigidbodyType2D.Dynamic;
        lockPhys.AddTorque(Random.Range(-2, 2), ForceMode2D.Impulse);
        LeanTween.delayedCall(1f, () =>
        {
            puzzleObject.OnPuzzleComplete();
        });
    }

    public override void OnPuzzleEnter()
    {
    }

    public override void OnPuzzleExit()
    {
    }

    public override void OnDialogueEnd()
    {
    }
}
