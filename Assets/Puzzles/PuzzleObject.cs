using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleObject : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject PuzzlePanel;
    [SerializeField] UnityEvent OnPuzzleEnterMethod;
    [SerializeField] UnityEvent OnPuzzleExitMethod;
    [SerializeField] UnityEvent OnPuzzleCompleteMethod;
    [SerializeField] ItemClass rewardItem;
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
        InventoryManager.Instance.OpenInventory(true, false);
    }

    public void OnPuzzleComplete()
    {
        PlayerControls.Instance.doPlayerControls = false;
        UIManager.Instance.ShowPuzzlePanel();
        InventoryManager.Instance.OpenInventory(false, false);

        LeanTween.delayedCall(0.6f, () =>
        {
            if (!isPuzzleFinished)
            {
                OnPuzzleCompleteMethod.Invoke();
                if (rewardItem != null) InventoryManager.Instance.TransferItemToKey(rewardItem);
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
