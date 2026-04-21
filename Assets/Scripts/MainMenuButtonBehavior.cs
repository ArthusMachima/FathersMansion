using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MainMenuButtonBehavior : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    TextMeshProUGUI text;
    [SerializeField] UnityEvent OnClick;
    [SerializeField] float hoverScale = 1.5f;
    [SerializeField] bool isPressable=true;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LeanTween.cancel(text.gameObject);
        text.transform.LeanScale(new(hoverScale, hoverScale, hoverScale), 0.3f).setEaseOutQuint();
        LeanTween.value(text.gameObject, text.color, new(1,1,1), 0.2f)
            .setOnUpdate(val => { text.color = val; });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanTween.cancel(text.gameObject);
        text.transform.LeanScale(new(1, 1, 1), 0.3f).setEaseOutQuint();
        LeanTween.value(text.gameObject, text.color, new(0.4f, 0.4f, 0.4f), 0.2f)
            .setOnUpdate(val => { text.color = val; });
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isPressable) return;
        LeanTween.cancel(text.gameObject);
        text.transform.LeanScale(new(1, 1, 1), 0).setOnComplete(() =>
        {
            text.color = new(0.4f, 0.4f, 0.4f);
            LeanTween.value(text.gameObject, text.color, new(1, 1, 1), 0.2f)
                .setOnUpdate(val => { text.color = val; });
            OnClick.Invoke();
        });
    }
}
