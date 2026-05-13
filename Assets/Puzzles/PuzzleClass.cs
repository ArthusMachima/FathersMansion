using UnityEngine;

public abstract class PuzzleClass : MonoBehaviour
{
    public abstract void OnPuzzleEnter();
    public abstract void OnPuzzleExit();
    public abstract void OnDialogueEnd();
}
