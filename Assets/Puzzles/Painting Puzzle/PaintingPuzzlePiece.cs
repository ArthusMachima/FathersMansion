using UnityEngine;
using UnityEngine.EventSystems;

public class PaintingPuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public PaintingPuzzle parentPuzzle;
    static readonly char[] directions = { 'w', 'a', 's', 'd' };
    [SerializeField] int directionIndex = 0;
    public char currentDirection = 'w';
    public char colorCode;
    [SerializeField] int rot;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Canvas rootCanvas;
    private bool isDragging = false;
    public bool canBeMoved = true;
    [SerializeField] ItemClass item;

    void OnEnable()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    Canvas GetRootCanvas()
    {
        if (rootCanvas != null) return rootCanvas;
        rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
        return rootCanvas;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canBeMoved) return;
        isDragging = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!canBeMoved) return;
        if (!isDragging)
        {
            directionIndex = (directionIndex + 1) % directions.Length;
            currentDirection = directions[directionIndex];
            rot = directionIndex * 90;
            LeanTween.cancel(gameObject);
            transform.LeanRotateZ(rot, 0.3f).setEaseOutQuint().setOnComplete(() =>
            {
                parentPuzzle.CheckAnswer();
            });
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canBeMoved) return;
        isDragging = true;
        rootCanvas = GetRootCanvas();

        originalParent = transform.parent;
        if (originalParent.TryGetComponent<PaintingPuzzleSlot>(out var originalSlot))
            originalSlot.heldPiece = null;

        transform.SetParent(rootCanvas.transform);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        GetComponent<RectTransform>().localPosition = localPoint;

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canBeMoved) return;
        rootCanvas = GetRootCanvas();
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
        if (!canBeMoved) return;
        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        ItemSlot targetItemSlot = null;
        foreach (GameObject hoveredObject in eventData.hovered)
        {
            if (hoveredObject.TryGetComponent<ItemSlot>(out var slot))
            {
                targetItemSlot = slot;
                break;
            }
        }

        if (targetItemSlot != null)
        {
            if (!targetItemSlot.HasItem())
            {
                targetItemSlot.PlaceItem(item);
                Destroy(gameObject);
            }
            else
            {
                PlaceOntoSlot(originalParent.GetComponent<PaintingPuzzleSlot>());
            }
        }
        else if (parentPuzzle.HoveredPuzzleSlot != null)
        {
            PlaceOntoSlot(parentPuzzle.HoveredPuzzleSlot);
            parentPuzzle.HoveredPuzzleSlot = null;
        }
        else
        {
            try
            {
                PlaceOntoSlot(originalParent.GetComponent<PaintingPuzzleSlot>());
            }
            catch (MissingReferenceException)
            {
                Destroy(gameObject);
            }
        }
    }

    public void PlaceOntoSlot(PaintingPuzzleSlot slot, bool checkAnswer = true)
    {
        transform.SetParent(slot.transform);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        slot.heldPiece = this;
        parentPuzzle = GetComponentInParent<PaintingPuzzle>();
        if (checkAnswer) parentPuzzle.CheckAnswer();
    }

    public void SetRotation(float rot)
    {
        LeanTween.cancel(gameObject);
        transform.LeanRotateZ(rot, 0);
    }
}
