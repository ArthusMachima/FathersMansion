using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleObject : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject PuzzlePanel;
    public Texture2D PuzzleTexture;
    [SerializeField] Sprite CompletedPuzzleSprite;
    [SerializeField] UnityEvent OnPuzzleEnterMethod;
    [SerializeField] UnityEvent OnPuzzleExitMethod;
    [SerializeField] UnityEvent OnPuzzleCompleteMethod;
    public ItemClass missingPieceReq;
    [SerializeField] ItemClass rewardItem;
    [SerializeField] bool isRewardKey;
    public bool isPuzzlePieceFound;
    public bool isPuzzleFinished;

    private void Start()
    {
        PlayerPrefs.DeleteKey("paintingPuzzle");
    }

    public void Interact()
    {
        UIManager.Instance.ShowPuzzlePanel(PuzzlePanel);
        PlayerControls.Instance.currentInteractedPuzzle = this;
        OnPuzzleEnter();
        if (OnPuzzleEnterMethod.GetPersistentEventCount()==0)
        {
            OnDialogueEnd();
        }
    }

    public void OnDialogueEnd()
    {
        InventoryManager.Instance.OpenInventory(true, false);
        UIManager.Instance.CurrentPuzzlePanel.OnDialogueEnd();
    }

    public void OnPuzzleComplete()
    {
        PlayerControls.Instance.doPlayerControls = false;
        UIManager.Instance.ShowPuzzlePanel();
        InventoryManager.Instance.OpenInventory(false, false);
        if (CompletedPuzzleSprite!=null) GetComponent<SpriteRenderer>().sprite = CompletedPuzzleSprite;

        LeanTween.delayedCall(0.6f, () =>
        {
            if (!isPuzzleFinished)
            {
                OnPuzzleCompleteMethod.Invoke();
                if (rewardItem != null)
                {
                    if (isRewardKey) InventoryManager.Instance.TransferItem(rewardItem, true);
                    else InventoryManager.Instance.TransferItem(rewardItem, false);
                }
                isPuzzleFinished = true;
            }
            PlayerControls.Instance.currentInteractedPuzzle = null;

            if (UIManager.Instance.pendingDialogue.Count == 0)
                PlayerControls.Instance.PuzzleMode(false);
        });
    }

    public void OnPuzzleEnter()
    {
        UIManager.Instance.CurrentPuzzlePanel.OnPuzzleEnter();
        OnPuzzleEnterMethod.Invoke();
    }

    public void OnPuzzleExit()
    {
        OnPuzzleExitMethod.Invoke();
    }
}
