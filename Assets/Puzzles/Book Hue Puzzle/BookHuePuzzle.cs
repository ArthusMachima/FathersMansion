using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookHuePuzzle : PuzzleClass
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

        LeanTween.delayedCall(0.1f, Randomize);
    }

    // ── Hue setup ─────────────────────────────────────────────────────────
    void SetUpBookHues()
    {
        if (Books == null || Books.Length == 0) return;

        for (int i = 0; i < Books.Length; i++)
        {
            float hue = (float)i / Books.Length * 0.833f;
            Books[i].Sprite.color = Color.HSVToRGB(hue, 1f, 1f);
        }
    }

    // ── Puzzle ────────────────────────────────────────────────────────────
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

    // ── Drag callbacks ────────────────────────────────────────────────────
    public void OnBeginDrag(PuzzleBookData item)
    {
        draggedItem = item;

        LeanTween.cancel(item.gameObject);
        LeanTween.scale(item.gameObject, Vector3.one * LiftScale, LiftDuration).setEaseOutQuad();

        item.transform.SetAsLastSibling();
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

    // ── Sibling reorder mid-drag ──────────────────────────────────────────
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

    // ── Insert index ──────────────────────────────────────────────────────
    private int ComputeInsertIndex(Vector2 localPos, PuzzleBookData dragged)
    {
        int   best     = 0;
        float bestDist = float.MaxValue;

        for (int i = 0; i < parentRect.childCount; i++)
        {
            var child = parentRect.GetChild(i) as RectTransform;
            if (child == null || child == dragged.RT) continue;

            float dist = isVertical
                ? Mathf.Abs(child.localPosition.y - localPos.y)
                : Mathf.Abs(child.localPosition.x - localPos.x);

            if (dist < bestDist)
            {
                bestDist = dist;

                bool before = isVertical
                    ? localPos.y > child.localPosition.y
                    : localPos.x < child.localPosition.x;

                best = before ? i : i + 1;
            }
        }

        return Mathf.Clamp(best, 0, parentRect.childCount - 1);
    }

    // ── Helpers ───────────────────────────────────────────────────────────
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
}
