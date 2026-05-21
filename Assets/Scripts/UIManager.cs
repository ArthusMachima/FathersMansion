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
        StopCoroutine(Dialogue());
        pendingDialogue.Clear();
        ShowDialoguePanel(false);
        CutscenePanel.alpha = 0;
        isCutscenePanelShown = false;
        CutsceneImage.sprite = null;
    }

    IEnumerator Dialogue()
    {
        PlayerControls.Instance.StopPlayer();
        while (pendingDialogue.Count > 0)
        {
            currentDialogueText.text = "";
            //AudioManager.Instance.PlaySFX(AudioManager.Instance.s_DialogueTyping);

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
                    isCutscenePanelShown = true;
                    CutsceneImage.sprite = pendingDialogue.Peek().cutsceneImage;


                    if (pendingDialogue.Peek().willBeInterupted)
                    {
                        CutscenePanel.alpha = 1;
                    }
                    else
                    {
                        LeanTween.value(CutscenePanel.gameObject, 0, 1, 0.5f)
                        .setOnUpdate(val => CutscenePanel.alpha = val);
                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }
            else if (isCutscenePanelShown)
            {
                if (pendingDialogue.Peek().willBeInterupted)
                {
                    Debug.Log("2a");
                    CutscenePanel.alpha = 0;
                }
                else
                {
                    Debug.Log("2b");
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
                AudioManager.Instance.PlaySFX(AudioManager.Instance.s_DialogueTyping);
                if (pendingDialogue.Peek().sentence == "") yield return null;
                else if (Input.GetKey(PlayerControls.Instance.Run)) yield return null;
                else if (chars == ',' || chars == '.' || chars == '?' || chars == '!' || chars == ':' || chars == '-')
                {
                    if (!pendingDialogue.Peek().disableTextSpecificDelays) yield return new WaitForSeconds(0.5f);
                    else yield return new WaitForSeconds(textSpeed);
                }   
                else yield return new WaitForSeconds(textSpeed);
            }

            if (!pendingDialogue.Peek().willBeInterupted)
            {
                Debug.Log("3a");
                dialogueConfirmSprite.SetActive(true);
                yield return new WaitUntil(() =>
                    Input.GetKeyDown(PlayerControls.Instance.Interact) ||
                    Input.GetKey(PlayerControls.Instance.Run));
                dialogueConfirmSprite.SetActive(false);
                pendingDialogue.Dequeue();
            }
            else
            {
                Debug.Log("3b");
                yield return new WaitForSeconds(textSpeed);
                pendingDialogue.Dequeue();
            }
            
        }

        if (isCutscenePanelShown)
        {
            if (pendingDialogue.Count>0 && !pendingDialogue.Peek().willBeInterupted)
            {
                Debug.Log("a");
                CutscenePanel.alpha = 0;
            }
            else
            {
                Debug.Log("b");
                LeanTween.value(CutscenePanel.gameObject, 1, 0, 0.5f)
                .setOnUpdate(val => CutscenePanel.alpha = val);
                yield return new WaitForSeconds(0.5f);
            }

                
            isCutscenePanelShown = false;
            CutsceneImage.sprite = null;
        }
        ShowDialoguePanel(false);
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

    public void LoadDialogue(Dialogue[] dialogueData, TextMeshProUGUI textMesh)
    {
        currentDialogueText = textMesh;
        pendingDialogue.Clear();
        for (int i = 0; i < dialogueData.Length; i++)
        {
            pendingDialogue.Enqueue(dialogueData[i]);
        }
        ShowDialoguePanel(true);
    }

    public void ShowDialoguePanel(bool show)
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

            DialoguePanelTop   .LeanMoveY(Screen.height, animationTime).setEaseOutQuint();
            DialoguePanelBottom.LeanMoveY(0, animationTime).setEaseOutQuint().setOnComplete(() =>
            {
                dialogueText.gameObject.SetActive(true);
                StartCoroutine(Dialogue());
            });
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
