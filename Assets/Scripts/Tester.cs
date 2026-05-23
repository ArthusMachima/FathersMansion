using System.Collections;
using UnityEngine;

public class Tester : MonoBehaviour
{
    void Update()
    {
        // Debug
        if (Input.GetKeyDown(KeyCode.F1)) PlayerControls.Instance.MonsterDistance = 0;

        if (Input.GetKeyDown(KeyCode.F2)) GameManager.Instance.Play1stFloorFinalPainting();
        if (Input.GetKeyDown(KeyCode.F3)) PlayerControls.Instance.currentInteractedPuzzle.OnPuzzleComplete();
        if (Input.GetKeyDown(KeyCode.F4)) GameManager.Instance.DoBasementScreenEffects(true);
        if (Input.GetKeyDown(KeyCode.F5)) GameManager.Instance.DoBasementScreenEffects(false);
        if (Input.GetKeyDown(KeyCode.F6)) GameManager.Instance.InstaStopBasementEffects();


        // Scene tester
        //if (Input.GetKeyDown(KeyCode.F11)) GameManager.Instance.PlaySpecialRoom();
    }

    IEnumerator testCorou()
    {
        yield return null;
    }
}
