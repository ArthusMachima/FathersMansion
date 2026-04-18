using UnityEngine;
using UnityEngine.EventSystems;

public class PaintingPuzzlePiece : PaintingPuzzle, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public enum PaintingDirection { w, a, s, d }

    public PaintingDirection currentDirection = PaintingDirection.w;
    [SerializeField] int directionIndex = 0;
    [SerializeField] int rot;

    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalPosition;
    private Canvas rootCanvas;
    private bool isDragging = false;
    [SerializeField] ItemClass item;

    void OnEnable()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
        {
            rot += 90;
            directionIndex++;

            if (directionIndex <= 1) { directionIndex = 1; currentDirection = PaintingDirection.a; }
            else if (directionIndex == 2) { currentDirection = PaintingDirection.s; }
            else if (directionIndex == 3) { currentDirection = PaintingDirection.d; }
            else if (directionIndex >= 4) { directionIndex = 0; rot = 0; currentDirection = PaintingDirection.w; }

            LeanTween.cancel(gameObject);
            transform.LeanRotateZ(rot, 0.3f).setEaseOutQuint();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        originalParent = transform.parent;

        PaintingPuzzleSlot originalSlot = originalParent.GetComponent<PaintingPuzzleSlot>();
        if (originalSlot != null)
            originalSlot.heldPiece = null;

        originalPosition = GetComponent<RectTransform>().anchoredPosition;
        transform.SetParent(rootCanvas.transform);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        GetComponent<RectTransform>().localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        // Check for ItemSlot under cursor
        ItemSlot targetItemSlot = null;
        foreach (GameObject hoveredObject in eventData.hovered)
        {
            ItemSlot slot = hoveredObject.GetComponent<ItemSlot>();
            if (slot != null)
            {
                targetItemSlot = slot;
                break;
            }
        }

        if (targetItemSlot != null)
        {
            Debug.Log($"Dropped on ItemSlot: {targetItemSlot.name}");
            targetItemSlot.PlaceItem(item);
            Destroy(gameObject);
        }
        else if (HoveredPuzzleSlot != null)
        {
            transform.SetParent(HoveredPuzzleSlot.transform);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            HoveredPuzzleSlot.heldPiece = this;
            HoveredPuzzleSlot = null;
        }
        else
        {
            try
            {
                transform.SetParent(originalParent);
                GetComponent<RectTransform>().anchoredPosition = originalPosition;
                originalParent.GetComponent<PaintingPuzzleSlot>().heldPiece = this;
            }
            catch (MissingReferenceException)
            {
                Destroy(gameObject);
            }
        }
    }
}