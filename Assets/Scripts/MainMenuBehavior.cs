using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuBehavior : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] CanvasGroup MainMenuPanel;
    [SerializeField] CanvasGroup SettingsPanel;
    [SerializeField] bool colorblindMode;

    [Header("Toggles")]
    [SerializeField] GameObject ColorblindToggle;

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
        if (SettingsPanel != null) if (SettingsPanel.alpha>=0.5f) ShowPanel(SettingsPanel, !true);
    }

    public void ShowMainMenu()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.s_UIConfirm);
        HideAllPanel();
        LeanTween.delayedCall(0.5f, () =>
        {
            ShowPanel(MainMenuPanel, true);
        });
    }

    public void ShowSettings()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.s_UICancel);
        HideAllPanel();
        LeanTween.delayedCall(0.5f, () =>
        {
            ShowPanel(SettingsPanel, true);
        });
    }

    

    //MainMenu Functions
    public void PlayGame()
    {
        PlayerPrefs.DeleteKey("savedFloor");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.s_UIConfirm);
        HideAllPanel();
        LeanTween.delayedCall(0.5f, () =>
        {
            SceneManager.LoadScene("MainGame");
        });
    }

    public void RetryGame()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.s_UIConfirm);
        PlayerPrefs.SetInt("PlayCount", PlayerPrefs.GetInt("PlayCount", 0) + 1);
        HideAllPanel();
        LeanTween.delayedCall(0.5f, () =>
        {
            GameManager.Instance.RespawnPlayer();
        });
    }

    public void ExitGame()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.s_UICancel);
        HideAllPanel();
        LeanTween.delayedCall(0.5f, () =>
        {
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        });
    }

    public void GoToMainMenu()
    {
        HideAllPanel();
        LeanTween.delayedCall(0.5f, () =>
        {
            SceneManager.LoadScene("MainMenu");
        });
    }

    



    //Other
    public void ToggleColorblindMode()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.s_UIConfirm);
        if (!colorblindMode)
        {
            colorblindMode = true;
            ColorblindToggle.SetActive(false);
        }
        else
        {
            colorblindMode = false;
            ColorblindToggle.SetActive(true);
        }
        PlayerPrefs.SetInt("colorblindMode", colorblindMode ? 1 : 0);
    }
}
