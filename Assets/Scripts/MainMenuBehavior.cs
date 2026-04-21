using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuBehavior : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] CanvasGroup MainMenuPanel;
    [SerializeField] CanvasGroup SettingsPanel;

    private void Start()
    {
        ShowMainMenu();
    }

    void ShowPanel(CanvasGroup panel, bool show)
    {
        LeanTween.value(panel.gameObject, show ?0:1, show ?1:0, 0.5f).setOnUpdate(val => panel.alpha = val);
        panel.interactable = show;
        panel.blocksRaycasts = show;
    }

    void HideAllPanel()
    {
        if (MainMenuPanel.alpha>=0.5f) ShowPanel(MainMenuPanel, !true);
        if (SettingsPanel.alpha>=0.5f) ShowPanel(SettingsPanel, !true);
    }

    public void ShowMainMenu()
    {
        HideAllPanel();
        LeanTween.delayedCall(0.5f, () =>
        {
            ShowPanel(MainMenuPanel, true);
        });
    }

    public void ShowSettings()
    {
        HideAllPanel();
        LeanTween.delayedCall(0.5f, () =>
        {
            ShowPanel(SettingsPanel, true);
        });
    }

    

    //MainMenu Functions
    public void PlayGame()
    {
        HideAllPanel();
        LeanTween.delayedCall(0.5f, () =>
        {
            SceneManager.LoadScene("MainGame");
        });
    }

    public void ExitGame()
    {
        HideAllPanel();
        LeanTween.delayedCall(0.5f, () =>
        {
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        });
    }
}
