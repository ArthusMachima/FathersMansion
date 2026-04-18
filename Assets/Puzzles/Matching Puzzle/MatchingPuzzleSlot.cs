using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MatchingPuzzleSlot : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] float flipTime = 0.3f;
    [SerializeField] MatchingPuzzle matchingPuzzle;
    public int cardType;
    [SerializeField] Image cardSprite;
    public Sprite frontCard;
    [SerializeField] Sprite backCard;
    public bool isFlipped;

    private void Start()
    {
        cardSprite = GetComponent<Image>();
        matchingPuzzle = GetComponentInParent<MatchingPuzzle>();
        cardSprite.sprite = backCard;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isFlipped && matchingPuzzle.puzzleInteractable && matchingPuzzle.isMissingPieceFound) Flip();
    }

    public void Flip()
    {
        if (!isFlipped)
        {
            transform.LeanRotateY(90, flipTime).setEaseOutQuint().setOnComplete(() =>
            {
                cardSprite.sprite = frontCard;
                transform.LeanRotateY(180, flipTime).setEaseOutQuint().setOnComplete(() =>
                {
                    StartCoroutine(matchingPuzzle.OnSlotFlip(this));
                });
            });
            isFlipped = true;
        }
        else
        {
            transform.LeanRotateY(90, flipTime).setEaseOutQuint().setOnComplete(() =>
            {
                cardSprite.sprite = backCard;
                transform.LeanRotateY(0, flipTime).setEaseOutQuint().setOnComplete(() =>
                {
                    isFlipped = false;
                });
            });
        }
    }
}
