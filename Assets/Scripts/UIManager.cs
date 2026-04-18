using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static GeneralEventsListeners;

public class UIManager : MonoBehaviour
{
    [SerializeField] float animationTime = 0.5f;

    [Header("Dialogue Properties")]
    public GameObject DialoguePanelTop;
    public GameObject DialoguePanelBottom;
    public Queue<Dialogue> pendingDialogue = new();
    public float textSpeed;
    public TextMeshProUGUI dialogueText;
    public GameObject dialogueConfirmSprite;

    [Header("Cutscene Panel")]
    [SerializeField] bool isCutscenePanelShown;
    [SerializeField] CanvasGroup CutscenePanel;
    [SerializeField] Image CutsceneImage;

    [Header("Cabinet Panel")]
    public GameObject CabinetPanel;
    [SerializeField] Transform CabinetItemSlotList;
    [SerializeField] ItemSlot[] CabinetItemSlot;

    [Header("Puzzle Panel")]
    [SerializeField] CanvasGroup PuzzleCanvasGroup;
    [SerializeField] GameObject CurrentPuzzlePanel;

    private void Start()
    {
        CabinetItemSlot = CabinetItemSlotList.GetComponentsInChildren<ItemSlot>();
    }




    // Singleton
    public static UIManager Instance;
    private void OnEnable()
    {
        Instance = this;
    }


    // Dialogue Panel
    IEnumerator Dialogue()
    {
        while (pendingDialogue.Count > 0)
        {
            dialogueText.text = "";
            //a.dialogue.clip = a.sfx_dialogue;    *insert audio source*

            if (pendingDialogue.Peek().methodCall != null && pendingDialogue.Peek().methodCall.GetPersistentEventCount() > 0)
            {
                pendingDialogue.Peek().methodCall.Invoke();
            }

            if (pendingDialogue.Peek().cutsceneImage != null && !isCutscenePanelShown)  // Does the current pendingDialogue queue have an image
            {
                Debug.Log("cutscene detected");

                isCutscenePanelShown = true;
                CutsceneImage.sprite = pendingDialogue.Peek().cutsceneImage;
                LeanTween.value(CutscenePanel.gameObject, 0, 1, 0.5f)
                    .setOnUpdate(val => CutscenePanel.alpha = val);
                yield return new WaitForSeconds(0.5f);
            }
            else if (isCutscenePanelShown) // Does the current pendingDialogue queue have NO image
            {
                LeanTween.value(CutscenePanel.gameObject, 1, 0, 0.5f)
                .setOnUpdate(val => CutscenePanel.alpha = val);
                yield return new WaitForSeconds(0.5f);
                isCutscenePanelShown = false;
                CutsceneImage.sprite = null;
            }

            foreach (char chars in pendingDialogue.Peek().sentence) // Putting individual characters each from pendingDialogue before a set slight delay.
            {
                dialogueText.text += chars;
                //if (!a.dialogue.isPlaying) { a.dialogue.Play(); } *insert sound effects*
                if (Input.GetKey(PlayerControls.Instance.ActionSecondary)) //Fast forward function
                    yield return null;
                else if (chars == ',' || chars == '.' || chars == '?' || chars == '!' || chars == ':' || chars == '-')  //Text delays
                    yield return new WaitForSeconds(0.5f);
                else
                    yield return new WaitForSeconds(textSpeed);
            }

            dialogueConfirmSprite.SetActive(true);
            yield return new WaitUntil(()=>
            Input.GetKeyDown(PlayerControls.Instance.ActionPrimary) || 
                Input.GetKey(PlayerControls.Instance.ActionSecondary));
            pendingDialogue.Dequeue();
            dialogueConfirmSprite.SetActive(false);
        }

        if (isCutscenePanelShown)
        {
            LeanTween.value(CutscenePanel.gameObject, 1, 0, 0.5f)
                .setOnUpdate(val => CutscenePanel.alpha = val);
            yield return new WaitForSeconds(0.5f);
            isCutscenePanelShown = false;
            CutsceneImage.sprite = null;
        }
        ShowDialoguePanel(false);

        PlayerControls.Instance.doPlayerControls = true;
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

    public void ShowDialoguePanel(bool show)
    {
        LeanTween.cancel(DialoguePanelTop);
        LeanTween.cancel(DialoguePanelBottom);
        if (show)
        {
            PlayerControls.Instance.doPlayerControls = false;
            DialoguePanelTop   .LeanMoveY(Screen.height, animationTime).setEaseOutQuint();
            DialoguePanelBottom.LeanMoveY(0, animationTime).setEaseOutQuint().setOnComplete(() =>
            {
                dialogueText.gameObject.SetActive(true);
                StartCoroutine(Dialogue());
            });
        }
        else
        {
            PlayerControls.Instance.doPlayerControls = true;
            dialogueText.gameObject.SetActive(false);

            DialoguePanelTop   .LeanMoveY( 150+Screen.height, animationTime).setEaseOutQuint();
            DialoguePanelBottom.LeanMoveY(-150, animationTime).setEaseOutQuint();
        }
    }



    // Cabinet Panel
    public void ShowCabinet(bool show, List<ItemClass> items)
    {
        if (show)
        {
            CabinetPanel.SetActive(true);
            for (int i=0; i<items.Count; i++)
            {
                if (items.Count>4)
                {
                    Debug.LogError("CABINET SHOULD ONLY HAVE FOUR ITEMS");
                    return;
                }

                if (items[i]!=null) CabinetItemSlot[i].PlaceItem(items[i]);
                else CabinetItemSlot[i].TakeItem();
            }
        }
        else
        {
            items.Clear();
            for (int i = 0; i < CabinetItemSlot.Length; i++)
            {
                if (CabinetItemSlot[i].HasItem()) items.Add(CabinetItemSlot[i].TakeItem());
            }
            PlayerControls.Instance.interactedObject = null;
            CabinetPanel.SetActive(false);
        }
    }



    // Puzzle Panel
    public void ShowPuzzlePanel(GameObject panel)
    {
        CurrentPuzzlePanel = Instantiate(panel, PuzzleCanvasGroup.transform);
        PuzzleCanvasGroup.gameObject.SetActive(true);
        PlayerControls.Instance.PuzzleMode(true);
        LeanTween.value(PuzzleCanvasGroup.gameObject, 0, 1, 0.3f)
            .setOnUpdate(val => PuzzleCanvasGroup.alpha = val);
    }

    public void ShowPuzzlePanel()
    {
        PlayerControls.Instance.OpenInventory(false, false);
        PlayerControls.Instance.doPlayerControls = false;
        LeanTween.value(PuzzleCanvasGroup.gameObject, 1, 0, 0.3f)
                .setOnUpdate(val => PuzzleCanvasGroup.alpha = val).setOnComplete(() =>
                {
                    StartCoroutine(DelayedPuzzlePanelHide());
                });
    }

    IEnumerator DelayedPuzzlePanelHide( )
    {
        yield return new WaitForSeconds(0.1f);
        PlayerControls.Instance.doPlayerControls = true;
        PlayerControls.Instance.PuzzleMode(false);
        PuzzleCanvasGroup.gameObject.SetActive(false);
        Destroy(CurrentPuzzlePanel);
    }
}
