using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleObject : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject PuzzlePanel;
    [SerializeField] UnityEvent OnPuzzleCompleteMethod;
    [SerializeField] ItemClass rewardItem;
    public bool isPuzzlePieceFound;
    [SerializeField] bool isPuzzleFinished;

    public void Interact()
    {
        EnterPuzzle();
        PlayerControls.Instance.OpenInventory(true, false);
    }



    void EnterPuzzle()
    {
        UIManager.Instance.ShowPuzzlePanel(PuzzlePanel);
        PlayerControls.Instance.currentInteractedPuzzle = this;
    }



    public void OnPuzzleComplete()
    {
        StartCoroutine(PuzzleCompleteBehavior());
    }

    IEnumerator PuzzleCompleteBehavior()
    {
        UIManager.Instance.ShowPuzzlePanel();
        PlayerControls.Instance.OpenInventory(false, false);
        yield return new WaitForSeconds(0.5f);
        if (!isPuzzleFinished)
        {
            OnPuzzleCompleteMethod.Invoke();
            if (rewardItem != null) PlayerControls.Instance.TransferItemToKey(rewardItem);
            isPuzzleFinished = true;
        }
        PlayerControls.Instance.currentInteractedPuzzle = null;
    }
}
