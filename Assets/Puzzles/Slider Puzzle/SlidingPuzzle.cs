using UnityEngine;
using UnityEngine.U2D;

public class SlidingPuzzle : PuzzleClass
{
    [SerializeField] Texture2D PuzzlePicture;
    [SerializeField] Sprite[] SlicedPuzzlePicture;

    [SerializeField] Transform PiecePositionParent;
    [SerializeField] Transform[] PiecePosition;

    [SerializeField] CanvasGroup Slots;
    [SerializeField] SlidingPuzzlePiece[] PuzzlePieces; // should be about the length of 9 (8)

    [SerializeField] int EmptySpace=9;

    [Header("Debug")]
    [SerializeField] bool TriggerScramblePieces;
    [SerializeField] bool TriggerCheckAnswer;

    private void Update()
    {
        //Debugs
        if (TriggerScramblePieces)
        {
            ScramblePieces();
            TriggerScramblePieces = false;
        }

        if (TriggerCheckAnswer)
        {
            CheckAnswer();
            TriggerCheckAnswer = false;
        }
    }

    private void Start()
    {
        PiecePosition = PiecePositionParent.GetComponentsInChildren<Transform>(); //Starting number is 1
        PuzzlePieces = GetComponentsInChildren<SlidingPuzzlePiece>();
        SlicedPuzzlePicture = Resources.LoadAll<Sprite>(PuzzlePicture.name);
        LeanTween.delayedCall(0.1f, () =>
        {
            ScramblePieces();
            LeanTween.value(Slots.gameObject, 0, 1, 0.1f).setOnUpdate(val => Slots.alpha = val);
        });
    }

    public void CheckAnswer()
    {
        bool correct = true;
        for (int i=0; i< PuzzlePieces.Length; i++)
        {
            Debug.Log($"{PuzzlePieces[i].pieceCodeNumber} = {i}");
            if (PuzzlePieces[i].pieceCodeNumber!=i) correct = false;
        }

        if (correct) Debug.Log("YAAAAYYYYYYYYYYYYYYYYYYYYYY!!!!!!!!!!!!!!!!!!");
        else Debug.LogError("wrong");
        //Todo: PlayerControls.Instance.currentInteractedPuzzle.OnPuzzleComplete();
    }

    public void ScramblePieces()
    {
        SlicedPuzzlePicture = null;
        SlicedPuzzlePicture = Resources.LoadAll<Sprite>(PuzzlePicture.name);

        foreach (var piece in PuzzlePieces) piece.setUpped = false;
        int num = 0;
        for (int i=0; i<SlicedPuzzlePicture.Length; i++)
        {
            do num = Random.Range(0, PuzzlePieces.Length);
            while (PuzzlePieces[num].setUpped);
            PuzzlePieces[num].image.sprite = SlicedPuzzlePicture[i];
            PuzzlePieces[num].pieceCodeNumber = i;
            PuzzlePieces[num].setUpped = true;

            if (i==SlicedPuzzlePicture.Length-1) PuzzlePieces[i].gameObject.SetActive(false);
        }
    }

    public void MovePiece(SlidingPuzzlePiece piece)
    {
        piece.gameObject.LeanMove(PiecePosition[EmptySpace].position, 0.3f).setEaseInOutQuint();


        CheckAnswer();
    }








    public override void OnPuzzleEnter()
    {

    }

    public override void OnPuzzleExit()
    {

    }
}
