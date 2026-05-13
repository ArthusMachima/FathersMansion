using TMPro;
using UnityEngine;

public class PinPuzzle : PuzzleClass
{
    [SerializeField] PuzzleObject puzzleObject;
    string pass = "";
    [SerializeField] string correctCode;
    [SerializeField] TextMeshProUGUI text;



    public override void OnPuzzleEnter()
    {

    }

    public override void OnPuzzleExit()
    {

    }

    private void Start()
    {
        puzzleObject = PlayerControls.Instance.currentInteractedPuzzle;
    }

    public void Confirm()
    {
        if (pass == correctCode)
        {
            Debug.Log("correct");
            puzzleObject.OnPuzzleComplete();
        }
        else
        {
            Debug.LogWarning("incorrect");
        }
    }

    public void Backspace()
    {
        if (pass.Length>0) pass=pass[..^1];
        text.text = pass;
    }

    public void AddNumber(int num)
    {
        
        if (pass.Length<6)
        {
            Debug.Log("Inputed");
            pass += num.ToString();
            text.text = pass;
        }
    }

    public override void OnDialogueEnd()
    {
    }
}
