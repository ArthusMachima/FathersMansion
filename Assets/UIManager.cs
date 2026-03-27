using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class UIManager : MonoBehaviour
{
    [SerializeField] float animationTime = 0.5f;

    public GameObject ExaminePanel;
    public GameObject ExamineDoorPanel;
    public GameObject ExamineCabinetPanel;
    public bool isExaminePanelShown;

    public GameObject DialoguePanel;
    public Queue<string> pendingDialogue = new();
    public float textSpeed;
    public TextMeshProUGUI dialogueText;





    // Singleton
    public static UIManager Instance;
    private void OnEnable()
    {
        Instance = this;
    }



    IEnumerator Dialogue()
    {
        PlayerControls.Instance.doMove = false;
        PlayerControls.Instance.doInteract = false;

        while (pendingDialogue.Count > 0)
        {
            dialogueText.text = "";
            //a.dialogue.clip = a.sfx_dialogue;    *insert sound effects*
            foreach (char chars in pendingDialogue.Peek())
            {
                dialogueText.text += chars;
                //if (!a.dialogue.isPlaying) { a.dialogue.Play(); }
                if (Input.GetKey(PlayerControls.Instance.ActionSecondary))
                    yield return null;
                else if (chars == ',' || chars == '.' || chars == '?' || chars == '!' || chars == ':' || chars == '-')
                    yield return new WaitForSeconds(0.5f);
                else
                    yield return new WaitForSeconds(textSpeed);
            }
            yield return new WaitUntil(()=>
            Input.GetKeyDown(PlayerControls.Instance.ActionPrimary) || 
                Input.GetKey(PlayerControls.Instance.ActionSecondary));
            pendingDialogue.Dequeue();
        }
        ShowDialoguePanel(false);

        PlayerControls.Instance.doMove = true;
        PlayerControls.Instance.doInteract = true;
    }



    public void LoadDialogue(string[] dialogueData)
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
        LeanTween.cancel(DialoguePanel);
        if (show)
        {
            DialoguePanel.LeanMoveY(0, animationTime).setEaseOutQuint().setOnComplete(() =>
            {
                dialogueText.gameObject.SetActive(true);
                StartCoroutine(Dialogue());
            });
        }
        else
        {
            dialogueText.gameObject.SetActive(false);
            DialoguePanel.LeanMoveY(-Screen.height + Screen.height - Screen.height / 3, animationTime).setEaseOutQuint();
        }
    }



    public void ShowExamineDoor(bool show)
    {
        if (show) OpenExamineGui(ExamineDoorPanel);
        else     CloseExamineGui(ExamineDoorPanel);
    }



    public void ShowExamineCabinet(bool show)
    {
        if (show) OpenExamineGui(ExamineCabinetPanel);
        else     CloseExamineGui(ExamineCabinetPanel);
    }



    public void OpenExamineGui(GameObject panel) //Examine UI Animation
    {
        PlayerControls.Instance.doMove = false;
        PlayerControls.Instance.doInteract = false;

        if (isExaminePanelShown) return;
        LeanTween.cancel(ExaminePanel); // reset animation

        ExaminePanel.transform.LeanScale(new(0, 0, 0), 0).setOnComplete(() =>
        {
            panel.SetActive(true);
            ExaminePanel.SetActive(true);
            ExaminePanel.transform.LeanScale(new(1, 1, 1), animationTime).setEaseOutQuint();
        });
        isExaminePanelShown = true;
    }



    public void CloseExamineGui(GameObject panel) //Examine UI Animation
    {
        if (!isExaminePanelShown) return;
        LeanTween.cancel(ExaminePanel);

        ExaminePanel.transform.LeanScale(new(0, 0, 0), animationTime).setEaseOutQuint().setOnComplete(() =>
        {
            if (panel==null)
            {
                foreach (Transform child in ExaminePanel.transform) child.gameObject.SetActive(false);
            }
            else panel.SetActive(false);
            ExaminePanel.SetActive(false);
            PlayerControls.Instance.doMove = true;
            PlayerControls.Instance.doInteract = true;
        });
        isExaminePanelShown = false;
    }
}
