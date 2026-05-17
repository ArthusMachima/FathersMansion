using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MatchingPuzzleSlot : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] float flipTime = 0.3f;
    [SerializeField] MatchingPuzzle matchingPuzzle;
    public int cardType;
    public Image cardSprite;
    public Sprite frontCard;
    public Sprite backCard;
    public bool isFlipped;
    [SerializeField] AudioManager aud;

    private void Start()
    {
        aud = AudioManager.Instance;
        cardSprite = GetComponent<Image>();
        matchingPuzzle = GetComponentInParent<MatchingPuzzle>();
        cardSprite.sprite = backCard;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isFlipped && matchingPuzzle.puzzleInteractable && matchingPuzzle.puzzleObject.isPuzzlePieceFound
            && !matchingPuzzle.puzzleObject.isPuzzleFinished) Flip();
    }

    public void Flip()
    {
        if (!isFlipped)
        {
            matchingPuzzle.puzzleInteractable = false;
            aud.PlaySFX(aud.s_CardFlip);
            transform.LeanRotateY(90, flipTime).setEaseOutQuint().setOnComplete(() =>
            {
                cardSprite.sprite = frontCard;
                transform.LeanRotateY(180, flipTime).setEaseOutQuint().setOnComplete(() =>
                {
                    isFlipped = true;
                    StartCoroutine(matchingPuzzle.OnSlotFlip(this));
                });
            });
        }
        else
        {
            aud.PlaySFX(aud.s_CardFlipBack);
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
