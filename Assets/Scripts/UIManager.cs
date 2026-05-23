using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] float animationTime = 0.5f;

    [Header("Dialogue Properties")]
    public GameObject DialoguePanelTop;
    public GameObject DialoguePanelBottom;
    public Queue<Dialogue> pendingDialogue = new();
    public float textSpeed;
    public TextMeshProUGUI currentDialogueText;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI ScreenText;
    [SerializeField] GameObject dialogueConfirmSprite;
    private Coroutine dialogueCoroutine;
    private bool suppressPanelClose;
    private float lastTypingSFXTime;

    [Header("Cutscene Panel")]
    [SerializeField] bool isCutscenePanelShown;
    [SerializeField] CanvasGroup CutscenePanel;
    [SerializeField] Image CutsceneImage;

    [Header("Puzzle Panel")]
    [SerializeField] CanvasGroup PuzzleCanvasGroup;
    public PuzzleClass CurrentPuzzlePanel;



    // Singleton
    public static UIManager Instance;
    private void OnEnable()
    {
        Instance = this;
    }

    private void Start()
    {
        currentDialogueText = dialogueText;
    }


    // Dialogue Panel
    public void ForceStopDialogue()
    {
        currentDialogueText.text = "";
        if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
        dialogueCoroutine = null;
        pendingDialogue.Clear();
        ShowDialoguePanel(false);
        CutscenePanel.alpha = 0;
        isCutscenePanelShown = false;
        CutsceneImage.sprite = null;
    }

    IEnumerator Dialogue()
    {
        PlayerControls.Instance.StopPlayer();
        bool nofade = false;
        while (pendingDialogue.Count > 0)
        {
            currentDialogueText.text = ""; 
            
            Debug.Log($"pending dialogue: {pendingDialogue.Peek().sentence}" +
                        $"\nnofade: {pendingDialogue.Peek().noFadeInTransition}");

            if (pendingDialogue.Peek().sound != null) AudioManager.Instance.PlaySFX(pendingDialogue.Peek().sound);

            if (pendingDialogue.Peek().methodCall != null && pendingDialogue.Peek().methodCall.GetPersistentEventCount() > 0)
            {
                pendingDialogue.Peek().methodCall.Invoke();
            }

            //cutscene check
            if (pendingDialogue.Peek().cutsceneImage != null)
            {
                if (!isCutscenePanelShown)
                {
                    // No image was showing — fade in (or instant)
                    isCutscenePanelShown = true;
                    CutsceneImage.sprite = pendingDialogue.Peek().cutsceneImage;

                    if (pendingDialogue.Peek().noFadeInTransition)
                    {
                        CutscenePanel.alpha = 1;
                        Debug.Log("1 true");
                    }
                    else
                    {
                        Debug.Log("1 false");
                        LeanTween.value(CutscenePanel.gameObject, 0, 1, 0.5f)
                            .setOnUpdate(val => CutscenePanel.alpha = val);
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                else
                {
                    // An image was already showing — swap to new one
                    if (pendingDialogue.Peek().noFadeInTransition)
                    {
                        // Instant swap
                        CutsceneImage.sprite = pendingDialogue.Peek().cutsceneImage;
                        Debug.Log("swap instant");
                    }
                    else
                    {
                        // Fade out, swap sprite, fade in
                        LeanTween.value(CutscenePanel.gameObject, 1, 0, 0.5f)
                            .setOnUpdate(val => CutscenePanel.alpha = val);
                        yield return new WaitForSeconds(0.5f);
                        CutsceneImage.sprite = pendingDialogue.Peek().cutsceneImage;
                        LeanTween.value(CutscenePanel.gameObject, 0, 1, 0.5f)
                            .setOnUpdate(val => CutscenePanel.alpha = val);
                        yield return new WaitForSeconds(0.5f);
                        Debug.Log("swap fade");
                    }
                }
            }
            else if (isCutscenePanelShown)
            {
                if (pendingDialogue.Peek().noFadeInTransition) 
                { 
                    CutscenePanel.alpha = 0;
                    Debug.Log("hide instant");
                }
                else
                {
                    Debug.Log("hide fade");
                    LeanTween.value(CutscenePanel.gameObject, 1, 0, 0.5f)
                        .setOnUpdate(val => CutscenePanel.alpha = val);
                    yield return new WaitForSeconds(0.5f);
                }
                isCutscenePanelShown = false;
                CutsceneImage.sprite = null;
            }

            foreach (char chars in pendingDialogue.Peek().sentence)
            {
                currentDialogueText.text += chars;
                if (chars != ' ' && Time.time - lastTypingSFXTime >= 0.05f)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.s_DialogueTyping);
                    lastTypingSFXTime = Time.time;
                }
                if (pendingDialogue.Peek().sentence == "") yield return null;
                else if (Input.GetKey(PlayerControls.Instance.Run)) yield return null;
                else if (chars == ',' || chars == '.' || chars == '?' || chars == '!' || chars == ':' || chars == '-')
                {
                    if (!pendingDialogue.Peek().disableTextSpecificDelays) yield return new WaitForSeconds(0.5f);
                    else yield return new WaitForSeconds(textSpeed);
                }   
                else yield return new WaitForSeconds(textSpeed);
            }

            nofade = pendingDialogue.Peek().noFadeInTransition;

            //dialog skip
            if (!pendingDialogue.Peek().willBeInterupted)
            {
                dialogueConfirmSprite.SetActive(true);
                yield return new WaitUntil(() =>
                    Input.GetKeyDown(PlayerControls.Instance.Interact) ||
                    Input.GetKey(PlayerControls.Instance.Run));
                dialogueConfirmSprite.SetActive(false);
                pendingDialogue.Dequeue();
                Debug.Log("000000000000");
            }
            else
            {
                yield return new WaitForSeconds(textSpeed);
                pendingDialogue.Dequeue();
                Debug.Log("11111111skip1111111111");
            }
            
        }

        if (isCutscenePanelShown)
        {
            if (nofade)
            {
                Debug.Log("3 true");
                CutscenePanel.alpha = 0;
            }
            else
            {
                Debug.Log("3 false");
                LeanTween.value(CutscenePanel.gameObject, 1, 0, 0.5f)
                .setOnUpdate(val => CutscenePanel.alpha = val);
                yield return new WaitForSeconds(0.5f);
            }

                
            isCutscenePanelShown = false;
            CutsceneImage.sprite = null;
        }
        if (suppressPanelClose)
        {
            suppressPanelClose = false;
            ShowDialoguePanelInstant(false);
            yield break;
        }
        ShowDialoguePanel(false);
        Debug.Log("-------------------------------");
    }

    public void LoadDialogue(Dialogue[] dialogueData)
    {
        pendingDialogue.Clear();
        for (int i = 0; i < dialogueData.Length; i++)
        {
            pendingDialogue.Enqueue(dialogueData[i]);
        }
        ShowDialoguePanel(true);
    }

    public void LoadDialogue(Dialogue[] dialogueData, TextMeshProUGUI textMesh, bool skipPanelAnimation = false)
    {
        currentDialogueText = textMesh;
        pendingDialogue.Clear();
        for (int i = 0; i < dialogueData.Length; i++)
        {
            pendingDialogue.Enqueue(dialogueData[i]);
        }
        if (skipPanelAnimation) suppressPanelClose = true;
        ShowDialoguePanel(true, skipPanelAnimation);
    }

    public void LoadDialogueDirect(Dialogue[] dialogueData, TextMeshProUGUI textMesh)
    {
        if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
        currentDialogueText = textMesh;
        pendingDialogue.Clear();
        foreach (var d in dialogueData)
            pendingDialogue.Enqueue(d);
        dialogueCoroutine = StartCoroutine(Dialogue());
    }

    public void ShowDialoguePanel(bool show, bool skipAnimation = false)
    {
        LeanTween.cancel(DialoguePanelTop);
        LeanTween.cancel(DialoguePanelBottom);
        PlayerControls player = PlayerControls.Instance;

        if (show)
        {
            if (!GameManager.Instance.cutsceneMode)
            {
                player.doPlayerControls = false;
                player.doPlayerAnimations = false;
                player.anim.SetBool("isMoving", false);
                player.anim.SetBool("isRunning", false);
            }

            if (skipAnimation)
            {
                dialogueText.gameObject.SetActive(true);
                if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
                dialogueCoroutine = StartCoroutine(Dialogue());
            }
            else
            {
                DialoguePanelTop   .LeanMoveY(Screen.height, animationTime).setEaseOutQuint();
                DialoguePanelBottom.LeanMoveY(0, animationTime).setEaseOutQuint().setOnComplete(() =>
                {
                    dialogueText.gameObject.SetActive(true);
                    if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
                    dialogueCoroutine = StartCoroutine(Dialogue());
                });
            }
        }
        else
        {
            if (!GameManager.Instance.cutsceneMode)
            {
                PlayerControls.Instance.doPlayerControls = true;
                PlayerControls.Instance.doPlayerAnimations = true;
            }
            dialogueText.gameObject.SetActive(false);
            DialoguePanelTop   .LeanMoveY( 150+Screen.height, animationTime).setEaseOutQuint();
            DialoguePanelBottom.LeanMoveY(-150, animationTime).setEaseOutQuint();
        }
    }

    // Instantly snaps panels on or off screen with no animation
    public void ShowDialoguePanelInstant(bool show)
    {
        LeanTween.cancel(DialoguePanelTop);
        LeanTween.cancel(DialoguePanelBottom);
        RectTransform top = DialoguePanelTop.GetComponent<RectTransform>();
        RectTransform bot = DialoguePanelBottom.GetComponent<RectTransform>();
        if (show)
        {
            top.anchoredPosition = new Vector2(top.anchoredPosition.x, Screen.height);
            bot.anchoredPosition = new Vector2(bot.anchoredPosition.x, 0);
            dialogueText.gameObject.SetActive(true);
        }
        else
        {
            top.anchoredPosition = new Vector2(top.anchoredPosition.x, 150 + Screen.height);
            bot.anchoredPosition = new Vector2(bot.anchoredPosition.x, -150);
            dialogueText.gameObject.SetActive(false);
        }
    }

    public void SetDialoguePanelsActive(bool active)
    {
        ShowDialoguePanelInstant(active);
    }

    // Puzzle Panel
    public void ShowPuzzlePanel(GameObject panel)
    {
        CurrentPuzzlePanel = Instantiate(panel, PuzzleCanvasGroup.transform).GetComponent<PuzzleClass>();
        PuzzleCanvasGroup.gameObject.SetActive(true);
        PlayerControls.Instance.PuzzleMode(true);
        LeanTween.value(PuzzleCanvasGroup.gameObject, 0, 1, 0.3f)
            .setOnUpdate(val => PuzzleCanvasGroup.alpha = val);
    }

    public void ShowPuzzlePanel()
    {
        InventoryManager.Instance.OpenInventory(false, false);
        LeanTween.value(PuzzleCanvasGroup.gameObject, 1, 0, 0.3f)
            .setOnUpdate(val => PuzzleCanvasGroup.alpha = val).setOnComplete(() =>
            {
                StartCoroutine(DelayedPuzzlePanelHide());
            });
    }

    IEnumerator DelayedPuzzlePanelHide()
    {
        yield return new WaitForSeconds(0.1f);
        PuzzleCanvasGroup.gameObject.SetActive(false);
        if (CurrentPuzzlePanel!=null) Destroy(CurrentPuzzlePanel.gameObject);
    }
}
