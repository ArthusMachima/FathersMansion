using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class MatchingPuzzle : PuzzleClass, IPointerEnterHandler
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
        for (int i = 0; i < slotAmount; i++)
        {
            // Guard against infinite loop if all variants are exhausted
            bool anyAvailable = false;
            foreach (var v in variants) if (v.amount > 0) { anyAvailable = true; break; }
            if (!anyAvailable) break;

            GameObject card = Instantiate(slotPrefab, slotParent);
            MatchingPuzzleSlot slot = card.GetComponent<MatchingPuzzleSlot>();

            int chance;
            do { chance = Random.Range(0, variants.Length); }
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
            if (!slot.gameObject.activeSelf) continue; // skip hidden slot
            if (!slot.isFlipped) allAreFlipped = false;
        }
        if (allAreFlipped)
        {
            Debug.Log("PUZZLE COMPLETE");
            PlayerControls.Instance.currentInteractedPuzzle.OnPuzzleComplete();
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
        if (InventoryManager.Instance.heldItem == pieceRequirement)
        {
            InventoryManager.Instance.heldItem = null;
            InventoryManager.Instance.draggedItem.gameObject.SetActive(false);
            InsertLostPiece();
        }
    }

    public override void OnPuzzleEnter()
    {

    }

    // Removed call to currentInteractedPuzzle.OnPuzzleExit() here —
    // PuzzleObject already handles that in PlayerControls' SolvingPuzzle exit block
    public override void OnPuzzleExit()
    {

    }
}

[Serializable]
public class MatchingPuzzleSlotVariant
{
    public Sprite frontCard;
    public int amount = 2;
}
