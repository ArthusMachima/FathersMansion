using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class MatchingPuzzle : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] MatchingPuzzleSlot[] matchingPuzzleSlots;
    [SerializeField] Transform slotParent;
    [SerializeField] MatchingPuzzleSlot FlippedA;
    [SerializeField] MatchingPuzzleSlot FlippedB;
    [SerializeField] MatchingPuzzleSlotVariant[] variants;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] int slotAmount = 16;
    public bool puzzleInteractable = true;
    public bool isMissingPieceFound;
    MatchingPuzzleSlot hidPuzzleSlot;
    [SerializeField] ItemClass pieceRequirement;

    private void Start()
    {
        InstantiatePuzzleSlots();
        matchingPuzzleSlots = slotParent.GetComponentsInChildren<MatchingPuzzleSlot>();
        MissingPieceCheck();
    }

    void InstantiatePuzzleSlots()
    {
        int chance;
        for (int i = 0; i < slotAmount; i++)
        {
            GameObject card = Instantiate(slotPrefab, slotParent);
            MatchingPuzzleSlot slot = card.GetComponent<MatchingPuzzleSlot>();
            do
            {
                chance = Random.Range(1, 9);
            }
            while (variants[chance].amount == 0);
            
            slot.frontCard = variants[chance].frontCard;
            slot.cardType = chance;
            variants[chance].amount--;
        }
    }

    void MissingPieceCheck()
    {
        isMissingPieceFound = PlayerControls.Instance.currentInteractedPuzzle.isPuzzlePieceFound;
        if (isMissingPieceFound) return;
        int chance = Random.Range(0, matchingPuzzleSlots.Length);
        hidPuzzleSlot = matchingPuzzleSlots[chance];
        hidPuzzleSlot.gameObject.SetActive(false);
    }

    public IEnumerator OnSlotFlip(MatchingPuzzleSlot slot)
    {
        puzzleInteractable = false;
        if (FlippedA == null)
        {
            FlippedA = slot;
        }
        else if (FlippedB == null)
        {
            FlippedB = slot;
            if (FlippedA.cardType == FlippedB.cardType)
            {
                FlippedA = null;
                FlippedB = null;
            }
            else
            {
                yield return new WaitForSeconds(0.3f);
                FlippedA.Flip();
                FlippedB.Flip();
                FlippedA = null;
                FlippedB = null;
            }
        }
        puzzleInteractable = true;
        CheckPuzzleSlots();
    }

    public void CheckPuzzleSlots()
    {
        bool allAreFlipped = true;
        foreach (var slot in matchingPuzzleSlots)
        {
            if (!slot.isFlipped) allAreFlipped=false;
        }
        if (allAreFlipped)
        {
            Debug.Log("PUZZLE COMPLETE");
            PlayerControls.Instance.currentInteractedPuzzle.OnPuzzleComplete();
        }
        else
        {

        }
    }

    public void InsertLostPiece()
    {
        if (!isMissingPieceFound)
        {
            hidPuzzleSlot.gameObject.SetActive(true);
            isMissingPieceFound = true;
            PlayerControls.Instance.currentInteractedPuzzle.isPuzzlePieceFound = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (PlayerControls.Instance.heldItem == pieceRequirement)
        {
            PlayerControls.Instance.heldItem = null;
            PlayerControls.Instance.draggedItem.gameObject.SetActive(false);
            InsertLostPiece();
        }
    }
}

[Serializable]
public class MatchingPuzzleSlotVariant
{
    public Sprite frontCard;
    public int amount=2;
}
