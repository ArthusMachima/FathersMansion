using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BookHuePuzzle : PuzzleClass, IPointerEnterHandler
{
    [SerializeField] PuzzleObject puzzleObject;
    [SerializeField] PuzzleBookData[] Books;
    [SerializeField] Transform BooksParent;

    private const float SnapDuration = 0.22f;
    private const float LiftScale    = 1.06f;
    private const float LiftDuration = 0.12f;

    private LayoutGroup   layoutGroup;
    private RectTransform containerRect;
    private RectTransform parentRect;
    private Camera        uiCamera;
    private bool          isVertical = true;
    private bool          isGrid     = false;

    private PuzzleBookData draggedItem;
    // Cached rest positions of non-dragged books, snapshotted when drag begins.
    // Used by ComputeInsertIndex so stale localPositions (layout disabled) never
    // cause the edge-oscillation bug.
    private Vector2[] slotPositions;

    // ─────────────────────────────────────────────────────────────────────
    private void Start()
    {
        puzzleObject = PlayerControls.Instance.currentInteractedPuzzle;

        containerRect = GetComponent<RectTransform>();
        parentRect    = BooksParent.GetComponent<RectTransform>();

        layoutGroup = BooksParent.GetComponent<LayoutGroup>();
        if (layoutGroup == null)
            layoutGroup = GetComponent<LayoutGroup>();

        var canvas = GetComponentInParent<Canvas>();
        uiCamera = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? canvas.worldCamera
            : null;

        Books = BooksParent.GetComponentsInChildren<PuzzleBookData>();

        DetectLayout();
        InitItems();
        SetUpBookHues();

        if (!puzzleObject.isPuzzleFinished) LeanTween.delayedCall(0.1f, Randomize);
    }

    void SetUpBookHues()
    {
        if (Books == null || Books.Length == 0) return;

        string code = "fokmhbadejligcn";

        for (int i = 0; i < Books.Length; i++)
        {
            if (GameManager.Instance.colorblindMode)
            {
                float hue = (float)i / Books.Length * 0.833f;
                Books[i].Sprite.color = Color.HSVToRGB(hue, 1f, 1f);
                Books[i].letter.text = code[i].ToString();
            }
            else
            {
                float hue = (float)i / Books.Length * 0.833f;
                Books[i].Sprite.color = Color.HSVToRGB(hue, 1f, 1f);
                Books[i].letter.gameObject.SetActive(false);
            }
        }
    }

    public void Randomize()
    {
        if (Books == null || Books.Length < 2) return;

        // Sattolo cycle — guarantees every item moves to a DIFFERENT index.
        // Unlike do-while retrying, this is O(n) with zero risk of looping.
        for (int i = Books.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i);   // note: excludes i, so no item stays in place
            (Books[i], Books[j]) = (Books[j], Books[i]);
        }

        SetLayout(false);
        for (int i = 0; i < Books.Length; i++)
        {
            Books[i].transform.SetSiblingIndex(i);
            Books[i].SetCurrentIndex(i);
        }
        SetLayout(true);
        ForceRebuild();

        if (puzzleObject.isPuzzlePieceFound)
        {
            layoutGroup.enabled = true;
            Books[2].gameObject.SetActive(true);
            foreach (var book in Books) book.interactable = true;
        }
        else
        {
            layoutGroup.enabled = false;
            foreach (var book in Books) book.interactable = false;
            Books[2].gameObject.SetActive(false);
        }
    }

    public void InsertLostPiece()
    {
        if (!puzzleObject.isPuzzlePieceFound)
        {
            layoutGroup.enabled = true;
            Books[2].gameObject.SetActive(true);
            foreach (var book in Books) book.interactable = true;
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

    public void CheckSolved()
    {
        if (!IsCurrentlySolved()) 
        {
            Debug.Log("Not solved");
            return;
        }
        Debug.Log("[BookHuePuzzle] Solved!");
        puzzleObject.OnPuzzleComplete();
    }

    private bool IsCurrentlySolved()
    {
        if (Books == null || Books.Length == 0) return false;

        foreach (var item in Books)
        {
            if (item == null) continue;
            if (item.CurrentIndex != item.CodeIndex) return false;
        }
        return true;
    }

    public void OnBeginDrag(PuzzleBookData item)
    {
        draggedItem = item;

        LeanTween.cancel(item.gameObject);
        LeanTween.scale(item.gameObject, Vector3.one * LiftScale, LiftDuration).setEaseOutQuad();

        item.transform.SetAsLastSibling();

        // Snapshot positions BEFORE disabling the layout so ComputeInsertIndex
        // always has stable, up-to-date reference points for non-dragged children.
        SnapshotSlotPositions(item);
        SetLayout(false);
    }

    public void OnDrag(PuzzleBookData item, Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, screenPos, uiCamera, out Vector2 localPt);

        item.RT.localPosition = localPt;

        int newIndex = ComputeInsertIndex(localPt, item);
        ReorderSiblings(item, newIndex);
    }

    public void OnEndDrag(PuzzleBookData item)
    {
        LeanTween.cancel(item.gameObject);
        LeanTween.scale(item.gameObject, Vector3.one, LiftDuration).setEaseOutQuad();

        SetLayout(true);
        ForceRebuild();

        draggedItem = null;
        SyncArrayToHierarchy();
        CheckSolved();
    }

    private void ReorderSiblings(PuzzleBookData item, int targetIndex)
    {
        if (item.transform.GetSiblingIndex() == targetIndex) return;

        item.transform.SetSiblingIndex(targetIndex);

        SetLayout(true);
        ForceRebuild();

        foreach (var child in Books)
        {
            if (child == null || child == item) continue;
            LeanTween.cancel(child.gameObject);
            LeanTween.move(child.RT, child.RT.anchoredPosition, SnapDuration * 0.65f)
                     .setEaseOutCubic();
        }

        SetLayout(false);
    }

    // Snapshots the localPosition of every child except the one being dragged.
    // Called once at drag-start while the layout is still active, so positions are authoritative.
    private void SnapshotSlotPositions(PuzzleBookData dragged)
    {
        int nonDraggedCount = 0;
        for (int i = 0; i < parentRect.childCount; i++)
        {
            var child = parentRect.GetChild(i) as RectTransform;
            if (child != null && child != dragged.RT) nonDraggedCount++;
        }

        slotPositions = new Vector2[nonDraggedCount];
        int idx = 0;
        for (int i = 0; i < parentRect.childCount; i++)
        {
            var child = parentRect.GetChild(i) as RectTransform;
            if (child == null || child == dragged.RT) continue;
            slotPositions[idx++] = child.localPosition;
        }
    }

    // Projects the cursor onto the ordered slot axis and returns the insertion index.
    // Using cached slot positions eliminates the oscillation that occurred when
    // localPositions were read after the layout was disabled (stale data) and the
    // nearest-neighbour + before/after heuristic kept flipping at the boundaries.
    private int ComputeInsertIndex(Vector2 localPos, PuzzleBookData dragged)
    {
        if (slotPositions == null || slotPositions.Length == 0) return 0;

        // Find the slot whose axis coordinate is closest to the cursor.
        int   nearestSlot = 0;
        float bestDist    = float.MaxValue;

        for (int i = 0; i < slotPositions.Length; i++)
        {
            float dist = isVertical
                ? Mathf.Abs(slotPositions[i].y - localPos.y)
                : Mathf.Abs(slotPositions[i].x - localPos.x);

            if (dist < bestDist) { bestDist = dist; nearestSlot = i; }
        }

        // Decide whether to insert before or after the nearest slot.
        // The half-slot-width dead zone prevents the cursor flickering across
        // a boundary from triggering repeated sibling swaps.
        float slotSpacing = slotPositions.Length > 1
            ? Mathf.Abs(isVertical
                ? slotPositions[1].y - slotPositions[0].y
                : slotPositions[1].x - slotPositions[0].x)
            : 0f;

        float halfSlot = slotSpacing * 0.5f;

        float cursorAxis  = isVertical ? localPos.y         : localPos.x;
        float slotAxis    = isVertical ? slotPositions[nearestSlot].y : slotPositions[nearestSlot].x;

        // For vertical layouts positive-y is "before" (higher up); for horizontal,
        // positive-x is "after" (further right).
        bool insertAfter = isVertical
            ? (cursorAxis < slotAxis - halfSlot * 0.5f)
            : (cursorAxis > slotAxis + halfSlot * 0.5f);

        int targetIndex = insertAfter ? nearestSlot + 1 : nearestSlot;
        return Mathf.Clamp(targetIndex, 0, parentRect.childCount - 1);
    }

    private void DetectLayout()
    {
        if      (layoutGroup is GridLayoutGroup)       { isGrid = true;  isVertical = true;  }
        else if (layoutGroup is HorizontalLayoutGroup) { isGrid = false; isVertical = false; }
        else                                           { isGrid = false; isVertical = true;  }
    }

    private void InitItems()
    {
        for (int i = 0; i < Books.Length; i++)
        {
            if (Books[i] == null) continue;
            Books[i].transform.SetSiblingIndex(i);
            Books[i].Init(this, codeIndex: i);
        }
    }

    private void SetLayout(bool enabled)
    {
        if (layoutGroup != null) layoutGroup.enabled = enabled;
    }

    private void ForceRebuild()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
    }

    private void SyncArrayToHierarchy()
    {
        var sorted = new List<PuzzleBookData>(Books.Length);
        for (int i = 0; i < parentRect.childCount; i++)
        {
            if (parentRect.GetChild(i).TryGetComponent<PuzzleBookData>(out var ri))
                sorted.Add(ri);
        }
        Books = sorted.ToArray();

        for (int i = 0; i < Books.Length; i++)
            Books[i].SetCurrentIndex(i);
    }

    public PuzzleBookData[] GetCurrentOrder() => Books;

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
