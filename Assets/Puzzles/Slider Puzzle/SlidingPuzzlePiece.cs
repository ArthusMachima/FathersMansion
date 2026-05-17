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
    public int assignedPosIndex;
    public bool interactable;
    [SerializeField] AudioManager aud;

    private void Start()
    {
        aud = AudioManager.Instance;
        image = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable) return;
        aud.PlaySFX(aud.s_Pick);
        parentPuzzle.MovePiece(this);
    }
}