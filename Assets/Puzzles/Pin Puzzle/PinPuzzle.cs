using TMPro;
using UnityEngine;

public class PinPuzzle : PuzzleClass
{
    [SerializeField] PuzzleObject puzzleObject;
    string pass = "";
    [SerializeField] string correctCode;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] bool interactable;
    [SerializeField] AudioManager aud;


    public void MakePinInteractive()
    {
        interactable=true;
    }

    public override void OnPuzzleEnter()
    {

    }

    public override void OnPuzzleExit()
    {

    }

    private void Start()
    {
        aud = AudioManager.Instance;
        puzzleObject = PlayerControls.Instance.currentInteractedPuzzle;
    }

    public void Confirm()
    {
        if (pass == correctCode)
        {
            aud.PlaySFX(aud.s_PinCorrect);
            text.color = Color.green;
            puzzleObject.OnPuzzleComplete();
        }
        else
        {
            aud.PlaySFX(aud.s_PinIncorrect);
            text.color = Color.red;
            LeanTween.delayedCall(0.5f, () =>
            {
                text.color = Color.white;
            });
        }
    }

    public void Backspace()
    {
        if (pass.Length>0) pass=pass[..^1];
        text.text = pass;
    }

    public void AddNumber(int num)
    {
        if (!interactable) return;
        aud.PlaySFX(aud.s_PinType);
        if (pass.Length<6)
        {
            pass += num.ToString();
            text.text = pass;
        }
        else aud.PlaySFX(aud.s_PinIncorrect);
    }

    public override void OnDialogueEnd()
    {
        MakePinInteractive();
    }
}
