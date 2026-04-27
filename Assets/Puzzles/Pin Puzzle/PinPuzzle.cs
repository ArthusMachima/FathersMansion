using TMPro;
using UnityEngine;

public class PinPuzzle : PuzzleClass
{
    string pass = "";
    [SerializeField] string correctCode;
    [SerializeField] TextMeshProUGUI text;



    public override void OnPuzzleEnter()
    {

    }

    public override void OnPuzzleExit()
    {

    }

    public void Confirm()
    {
        if (pass == correctCode)
        {
            Debug.Log("correct");
            PlayerControls.Instance.currentInteractedPuzzle.OnPuzzleComplete();
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
}
