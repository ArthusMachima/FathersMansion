using UnityEngine;
using UnityEngine.EventSystems;

public class PaintingPuzzleSlot : MonoBehaviour, IDropHandler
{
    public PaintingPuzzle parentPuzzle;
    public PaintingPuzzlePiece heldPiece;

    private void Start()
    {
        parentPuzzle = GetComponentInParent<PaintingPuzzle>();
        heldPiece = GetComponentInChildren<PaintingPuzzlePiece>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        PaintingPuzzlePiece piece = eventData.pointerDrag.GetComponent<PaintingPuzzlePiece>();

        if (piece != null && heldPiece == null)
        {
            heldPiece = piece;
            parentPuzzle.HoveredPuzzleSlot = this;
        }
    }
}
