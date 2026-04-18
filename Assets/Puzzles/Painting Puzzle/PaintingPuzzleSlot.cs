using UnityEngine;
using UnityEngine.EventSystems;

public class PaintingPuzzleSlot : PaintingPuzzle, IDropHandler
{
    public PaintingPuzzlePiece heldPiece;

    private void Start()
    {
        heldPiece = GetComponentInChildren<PaintingPuzzlePiece>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        PaintingPuzzlePiece piece = eventData.pointerDrag.GetComponent<PaintingPuzzlePiece>();

        if (piece != null && heldPiece == null)
        {
            heldPiece = piece;
            HoveredPuzzleSlot = this;
        }
    }
}