using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlidingPuzzle : PuzzleClass, IPointerEnterHandler
{
    [SerializeField] PuzzleObject puzzleObject;
    [SerializeField] Sprite[] SlicedPuzzlePicture;

    [SerializeField] Transform PiecePositionParent;
    [SerializeField] Transform[] PiecePosition;

    [SerializeField] CanvasGroup Slots;
    [SerializeField] SlidingPuzzlePiece[] PuzzlePieces;

    [SerializeField] int EmptySpace = 8;

    [SerializeField] bool useSaveData = true; // Toggle saving/loading in Inspector

    private const string SaveKey = "slidingPuzzle";

    private void Start()
    {
        puzzleObject = PlayerControls.Instance.currentInteractedPuzzle;
        PiecePosition = PiecePositionParent.GetComponentsInChildren<Transform>()
                                           .Where(t => t != PiecePositionParent)
                                           .ToArray();
        PuzzlePieces = GetComponentsInChildren<SlidingPuzzlePiece>();
        SlicedPuzzlePicture = Resources.LoadAll<Sprite>(puzzleObject.PuzzleTexture.name);
        LeanTween.delayedCall(0.1f, () =>
        {
            ScramblePieces();
            LeanTween.value(Slots.gameObject, 0, 1, 0.1f).setOnUpdate(val => Slots.alpha = val);
        });
    }

    public void CheckAnswer()
    {
        bool correct = PuzzlePieces.All(p => p.pieceCodeNumber == p.assignedPosIndex);
        if (correct)
        {
            puzzleObject.OnPuzzleComplete();
            PlayerPrefs.DeleteKey(SaveKey);
        }
    }

    public void ScramblePieces()
    {
        SlicedPuzzlePicture = null;
        SlicedPuzzlePicture = Resources.LoadAll<Sprite>(puzzleObject.PuzzleTexture.name);

        for (int i = 0; i < PuzzlePieces.Length; i++)
        {
            PuzzlePieces[i].assignedPosIndex = i;
            PuzzlePieces[i].setUpped = true;
            PuzzlePieces[i].gameObject.SetActive(true);

            if (i < SlicedPuzzlePicture.Length)
            {
                PuzzlePieces[i].image.sprite = SlicedPuzzlePicture[i];
                PuzzlePieces[i].pieceCodeNumber = i;
            }
        }
        EmptySpace = PuzzlePieces.Length - 1;

        if (puzzleObject.isPuzzleFinished)
        {
            PuzzlePieces[^1].gameObject.SetActive(false);
            PuzzleAlreadyFinishedDialogue();
            return;
        }

        // Try loading saved data first
        if (useSaveData && TryLoadPuzzleState())
            return;

        // No save data found — do normal scramble
        int grid = 3;
        int[] firstColumn = new int[grid];
        int[] lastColumn = new int[grid];
        for (int i = 0; i < grid; i++)
        {
            firstColumn[i] = grid * i;
            lastColumn[i] = grid * (i + 1) - 1;
        }

        int lastMoved = -1;
        int shuffleMoves = 200;

        for (int s = 0; s < shuffleMoves; s++)
        {
            List<int> candidates = new();

            int left = EmptySpace - 1;
            int right = EmptySpace + 1;
            int down = EmptySpace - grid;
            int up = EmptySpace + grid;

            if (left >= 0 && !firstColumn.Any(i => EmptySpace == i) && left != lastMoved) candidates.Add(left);
            if (right <= grid * grid - 1 && !lastColumn.Any(i => EmptySpace == i) && right != lastMoved) candidates.Add(right);
            if (down >= 0 && down != lastMoved) candidates.Add(down);
            if (up <= grid * grid - 1 && up != lastMoved) candidates.Add(up);

            int chosen = candidates[Random.Range(0, candidates.Count)];

            SlidingPuzzlePiece movingPiece = System.Array.Find(PuzzlePieces, p => p.assignedPosIndex == chosen);
            if (movingPiece != null)
            {
                lastMoved = EmptySpace;
                movingPiece.assignedPosIndex = EmptySpace;
                EmptySpace = chosen;
                PuzzlePieces[^1].assignedPosIndex = EmptySpace;
            }
        }

        ApplyPositionsAndVisibility();
    }

    // Saves each piece as "pieceCodeNumber:assignedPosIndex", joined by commas,
    // with the EmptySpace index appended at the end (e.g. "0:4,1:2,...,8")
    public void SavePuzzleState()
    {
        if (!useSaveData) return;

        var parts = PuzzlePieces.Select(p => $"{p.pieceCodeNumber}:{p.assignedPosIndex}").ToList();
        parts.Add(EmptySpace.ToString());
        PlayerPrefs.SetString(SaveKey, string.Join(",", parts));
        PlayerPrefs.Save();
    }

    // Returns true and applies state if valid save data exists, false otherwise
    private bool TryLoadPuzzleState()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return false;

        string data = PlayerPrefs.GetString(SaveKey);
        string[] parts = data.Split(',');

        // Last entry is EmptySpace; all others are pieceCode:assignedPos pairs
        if (parts.Length != PuzzlePieces.Length + 1)
        {
            Debug.LogWarning("SlidingPuzzle: Save data length mismatch, scrambling instead.");
            return false;
        }

        if (!int.TryParse(parts[^1], out int savedEmptySpace))
        {
            Debug.LogWarning("SlidingPuzzle: Could not parse EmptySpace from save data.");
            return false;
        }

        EmptySpace = savedEmptySpace;

        for (int i = 0; i < PuzzlePieces.Length; i++)
        {
            string[] pair = parts[i].Split(':');
            if (pair.Length != 2 ||
                !int.TryParse(pair[0], out int pieceCode) ||
                !int.TryParse(pair[1], out int assignedPos))
            {
                Debug.LogWarning($"SlidingPuzzle: Could not parse piece data at index {i}, scrambling instead.");
                return false;
            }

            PuzzlePieces[i].pieceCodeNumber = pieceCode;
            PuzzlePieces[i].assignedPosIndex = assignedPos;
        }

        ApplyPositionsAndVisibility();
        return true;
    }

    // Shared logic for positioning and visibility after scramble or load
    private void ApplyPositionsAndVisibility()
    {
        foreach (var piece in PuzzlePieces)
        {
            piece.gameObject.transform.position = PiecePosition[piece.assignedPosIndex].position;
            if (puzzleObject.isPuzzlePieceFound) piece.interactable = true;

            if (piece.pieceCodeNumber == 7 && !puzzleObject.isPuzzlePieceFound)
            {
                piece.gameObject.SetActive(false);
            }
            else if (piece.pieceCodeNumber == 8)
            {
                piece.assignedPosIndex = 8;
                piece.gameObject.SetActive(false);
            }
        }
    }

    public void MovePiece(SlidingPuzzlePiece piece)
    {
        int grid = 3;
        int[] firstColumn = new int[grid];
        int[] lastColumn = new int[grid];
        for (int i = 0; i < grid; i++)
        {
            firstColumn[i] = grid * i;
            lastColumn[i] = grid * (i + 1) - 1;
        }

        bool found = false;
        if (piece.assignedPosIndex - 1 == EmptySpace && !firstColumn.Any(i => piece.assignedPosIndex == i)) found = true;
        if (piece.assignedPosIndex + 1 == EmptySpace && !lastColumn.Any(i => piece.assignedPosIndex == i)) found = true;
        if (piece.assignedPosIndex - grid == EmptySpace && piece.assignedPosIndex - grid >= 0) found = true;
        if (piece.assignedPosIndex + grid == EmptySpace && piece.assignedPosIndex + grid <= grid * grid - 1) found = true;
        if (!found) return;

        piece.gameObject.LeanMove(PiecePosition[EmptySpace].position, 0.3f).setEaseInOutQuint().setOnComplete(() =>
        {
            CheckAnswer();
        });
        (piece.assignedPosIndex, EmptySpace) = (EmptySpace, piece.assignedPosIndex);
    }

    public void InsertLostPiece()
    {
        if (!puzzleObject.isPuzzlePieceFound)
        {
            PuzzlePieces[7].gameObject.SetActive(true);
            foreach (var piece in PuzzlePieces) piece.interactable = true;
            puzzleObject.isPuzzlePieceFound = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InventoryManager.Instance.heldItem == puzzleObject.missingPieceReq)
        {
            InventoryManager.Instance.heldItem = null;
            InventoryManager.Instance.draggedItem.gameObject.SetActive(false);
            InsertLostPiece();
        }
    }

    void PuzzleAlreadyFinishedDialogue()
    {
        Dialogue[] msg = new Dialogue[]
        {
            new("I already solved this sliding puzzle.", null)
        };
        UIManager.Instance.LoadDialogue(msg);
    }

    public override void OnPuzzleEnter() { }

    public override void OnPuzzleExit()
    {
        SavePuzzleState();
    }

    public override void OnDialogueEnd() { }
}