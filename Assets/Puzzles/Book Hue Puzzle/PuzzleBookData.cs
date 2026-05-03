using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PuzzleBookData : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image Sprite;
    public int CurrentIndex { get; private set; }
    public int CodeIndex { get; private set; }
    private BookHuePuzzle parentList;
    public RectTransform RT { get; private set; }

    private void Awake()
    {
        Sprite = GetComponent<Image>();
        RT = GetComponent<RectTransform>();
    }

    public void Init(BookHuePuzzle list, int codeIndex)
    {
        parentList = list;
        CodeIndex = codeIndex;
        CurrentIndex = codeIndex;   // starts in the correct position
    }

    public void SetCurrentIndex(int index)
    {
        CurrentIndex = index;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parentList == null) return;
        parentList.OnBeginDrag(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentList == null) return;
        parentList.OnDrag(this, eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parentList == null) return;
        parentList.OnEndDrag(this);
    }
}
