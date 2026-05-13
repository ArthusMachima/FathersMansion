using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class SlidingPuzzle : PuzzleClass, IPointerEnterHandler
{
    [SerializeField] PuzzleObject puzzleObject;
    [SerializeField] Sprite[] SlicedPuzzlePicture;

    [SerializeField] Transform PiecePositionParent;
    [SerializeField] Transform[] PiecePosition;

    [SerializeField] CanvasGroup Slots;
    [SerializeField] SlidingPuzzlePiece[] PuzzlePieces; // should be about the length of 9 (8)

    [SerializeField] int EmptySpace=8;


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
        if (correct) puzzleObject.OnPuzzleComplete();
    }

    public void ScramblePieces()
    {
        //Apply sliced texture
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
        EmptySpace = PuzzlePieces.Length - 1; // = 8

        if (puzzleObject.isPuzzleFinished)
        {
            PuzzlePieces[^1].gameObject.SetActive(false);
            PuzzleAlreadyFinishedDialogue();
            return;
        }


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
        // Set up constraints
        int grid = 3;
        int[] firstColumn = new int[grid];
        int[] lastColumn = new int[grid];
        for (int i = 0; i < grid; i++)
        {
            firstColumn[i] = grid * i;           // 0, 3, 6
             lastColumn[i] = grid * (i + 1) - 1; // 2, 5, 8
        }

        // Check pieces
        bool found = false; 
        if (piece.assignedPosIndex -    1 == EmptySpace && !firstColumn.Any(i => piece.assignedPosIndex == i)) found = true; // check left
        if (piece.assignedPosIndex +    1 == EmptySpace && !lastColumn.Any(i => piece.assignedPosIndex == i))  found = true; // check right
        if (piece.assignedPosIndex - grid == EmptySpace && piece.assignedPosIndex - grid >= 0)                 found = true; // check down
        if (piece.assignedPosIndex + grid == EmptySpace && piece.assignedPosIndex + grid <= grid * grid - 1)   found = true; // check up
        if (!found) return;

        //Move piece
        piece.gameObject.LeanMove(PiecePosition[EmptySpace].position, 0.3f).setEaseInOutQuint().setOnComplete(() =>
        {
            CheckAnswer();
        });
        (piece.assignedPosIndex, EmptySpace) = (EmptySpace, piece.assignedPosIndex); // Tupple
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
