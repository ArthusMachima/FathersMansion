using TMPro;
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
    public TextMeshProUGUI letter;
    public bool interactable;
    [SerializeField] AudioManager aud;

    private void Awake()
    {
        aud = AudioManager.Instance;
        letter = GetComponentInChildren<TextMeshProUGUI>();
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
        aud.PlaySFX(aud.s_Pick);
        if (!interactable) return;
        if (parentList == null) return;
        parentList.OnBeginDrag(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!interactable) return;
        if (parentList == null) return;
        parentList.OnDrag(this, eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        aud.PlaySFX(aud.s_Place);
        if (!interactable) return;
        if (parentList == null) return;
        parentList.OnEndDrag(this);
    }
}
