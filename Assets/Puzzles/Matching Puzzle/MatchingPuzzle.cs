using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class MatchingPuzzle : PuzzleClass, IPointerEnterHandler
{
    public PuzzleObject puzzleObject;
    [SerializeField] MatchingPuzzleSlot[] matchingPuzzleSlots;
    [SerializeField] Transform slotParent;
    [SerializeField] MatchingPuzzleSlot FlippedA;
    [SerializeField] MatchingPuzzleSlot FlippedB;
    [SerializeField] Sprite[] SlicedPicture;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] int slotAmount = 16;
    public bool puzzleInteractable = true;
    MatchingPuzzleSlot hidPuzzleSlot;

    private void Start()
    {
        puzzleObject = PlayerControls.Instance.currentInteractedPuzzle;
        SlicedPicture = Resources.LoadAll<Sprite>(puzzleObject.PuzzleTexture.name);
        InstantiatePuzzleSlots();
        matchingPuzzleSlots = slotParent.GetComponentsInChildren<MatchingPuzzleSlot>();
        MissingPieceCheck();
        if (puzzleObject.isPuzzleFinished)
        {
            Debug.Log("already finished");
            foreach (var slot in matchingPuzzleSlots)
            {
                LeanTween.delayedCall(0.00000001f, () =>
                {
                    Debug.LogWarning("slot confiremd");
                    slot.cardSprite.sprite = slot.frontCard;
                });
            }
        }
    }

    void InstantiatePuzzleSlots()
    {
        // Build a paired list from sliced sprites (each sprite appears twice)
        List<(Sprite sprite, int type)> pool = new();
        for (int i = 0; i < slotAmount / 2; i++)
        {
            pool.Add((SlicedPicture[i + 1], i));
            pool.Add((SlicedPicture[i + 1], i));
        }

        // Shuffle the pool
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        // Instantiate slots
        foreach (var (sprite, type) in pool)
        {
            GameObject card = Instantiate(slotPrefab, slotParent);
            MatchingPuzzleSlot slot = card.GetComponent<MatchingPuzzleSlot>();
            slot.frontCard = sprite;
            slot.cardType = type;
            slot.backCard = SlicedPicture[0]; // assign back texture
        }

        
    }

    void MissingPieceCheck()
    {
        if (puzzleObject.isPuzzlePieceFound) return;
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
            if (!slot.gameObject.activeSelf) continue;
            if (!slot.isFlipped) allAreFlipped = false;
        }
        if (allAreFlipped)
        {
            puzzleObject.OnPuzzleComplete();
        }
    }

    public void InsertLostPiece()
    {
        if (!puzzleObject.isPuzzlePieceFound)
        {
            hidPuzzleSlot.gameObject.SetActive(true);
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

    public override void OnPuzzleEnter() { }
    public override void OnPuzzleExit() { }

    public override void OnDialogueEnd()
    {
    }
}