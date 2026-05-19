using UnityEngine;

public class PaintingPuzzle : PuzzleClass
{
    [SerializeField] Transform paintingPuzzleSlotParent;
    public PaintingPuzzleSlot[] paintingPuzzleSlots;
    public PaintingPuzzleSlot HoveredPuzzleSlot;
    public PaintingPuzzleSlot PrevPuzzleSlot;
    [SerializeField] string startingCode;
    public string CorrectCode = "odraisysbdgw";
    public GameObject[] PaintingPrefabs;

    private void Awake()
    {
        if (paintingPuzzleSlotParent != null)
            paintingPuzzleSlots = paintingPuzzleSlotParent.GetComponentsInChildren<PaintingPuzzleSlot>();
    }

    public string GetPuzzleStateString()
    {
        string code = "";
        foreach (var slot in paintingPuzzleSlots)
        {
            if (slot.heldPiece != null)
                code += $"{slot.heldPiece.colorCode}{slot.heldPiece.currentDirection}";
            else
                code += "##";
        }
        return code;
    }

    public void LoadPaintings()
    {
        string code = PlayerPrefs.GetString("paintingPuzzle", startingCode);
        Debug.Log(code);
        int slotIndex = 0;

        for (int i = 0; i + 1 < code.Length; i += 2)
        {
            char colorChar     = code[i];
            char directionChar = code[i + 1];

            // Color
            int colorIndex = "roygbiv#".IndexOf(colorChar);
            if (colorIndex < 0)
            {
                Debug.LogError($"No set definition for color '{colorChar}'");
                slotIndex++;
                continue;
            }

            PaintingPuzzlePiece piece;
            if (colorIndex == 7) // '#' = empty slot
            {
                piece = null;
            }
            else
            {
                piece = Instantiate(PaintingPrefabs[colorIndex]).GetComponent<PaintingPuzzlePiece>();
                piece.parentPuzzle = this;
            }

            // Direction
            int directionIndex = "wasd#".IndexOf(directionChar);
            if (directionIndex < 0)
            {
                Debug.LogError($"No set definition for direction '{directionChar}'");
            }
            else if (directionIndex != 4 && piece != null) // '#' = no rotation
            {
                piece.currentDirection = directionChar;
                piece.SetRotation(90 * directionIndex);
            }

            // Place without triggering CheckAnswer during load
            if (piece != null)
                piece.PlaceOntoSlot(paintingPuzzleSlots[slotIndex], false);

            slotIndex++;
        }
    }

    public override void OnPuzzleEnter()
    {
        Debug.Log("PUZZLE STARTED");
        LoadPaintings();
        if (PlayerControls.Instance.currentInteractedPuzzle.isPuzzleFinished)
        {
            foreach (var slot in paintingPuzzleSlots)
            {
                if (slot.heldPiece != null)
                    slot.heldPiece.canBeMoved = false;
            }
        }
    }

    public override void OnPuzzleExit()
    {
        PlayerPrefs.SetString("paintingPuzzle", GetPuzzleStateString());
    }

    public void CheckAnswer()
    {
        string code = GetPuzzleStateString();
        if (string.Equals(code, CorrectCode) && !PlayerControls.Instance.currentInteractedPuzzle.isPuzzleFinished)
        {
            Debug.Log("PUZZLE SOLVED");
            PlayerPrefs.DeleteKey("paintingPuzzle");
            LeanTween.delayedCall(0.5f, () =>
            {
                PlayerControls.Instance.currentInteractedPuzzle.OnPuzzleComplete();
            });
        }
        else Debug.Log(GetPuzzleStateString());
    }

    public override void OnDialogueEnd()
    {
    }
}
