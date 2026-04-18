using TMPro;
using UnityEngine;

public class SideScreenMessage : MonoBehaviour
{
    [SerializeField] Transform LinePanel;
    [SerializeField] CanvasGroup LinePanelCanvas;
    [SerializeField] TextMeshProUGUI TitleText;
    [SerializeField] TextMeshProUGUI DescriptionText;

    //Singleton
    public static SideScreenMessage Instance;
    private void OnEnable()
    {
        Instance = this;
    }

    public void DisplayMessage(string title, string descrition, float time)
    {
        LeanTween.cancel(LinePanel.gameObject);
        TitleText.text = title;
        DescriptionText.text = descrition;

        LeanTween.value(LinePanelCanvas.gameObject, 0, 1, 0.3f)
            .setOnUpdate(val => LinePanelCanvas.alpha = val);
        LinePanel.LeanMoveY((Screen.height/2)-200, 0).setOnComplete(() =>
        {
            LinePanel.LeanMoveY(Screen.height / 2, 1f).setEaseOutQuint().setOnComplete(() =>
            {
                LeanTween.value(LinePanelCanvas.gameObject, 1, 0, 0.3f)
                    .setOnUpdate(val => LinePanelCanvas.alpha = val).setDelay(time);
            });
        });
    }
}
