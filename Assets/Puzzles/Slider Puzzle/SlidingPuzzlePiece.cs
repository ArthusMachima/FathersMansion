using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlidingPuzzlePiece : MonoBehaviour, IPointerClickHandler
{
    public SlidingPuzzle parentPuzzle;
    public int pieceCodeNumber;
    public Image image;
    public bool setUpped;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        parentPuzzle.MovePiece(this);
    }
}