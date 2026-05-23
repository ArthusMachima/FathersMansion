using System.Collections;
using UnityEngine;

public class Tester : MonoBehaviour
{
    void Update()
    {
        // Debug
        if (Input.GetKeyDown(KeyCode.F1)) PlayerControls.Instance.MonsterDistance = 0;

        if (Input.GetKeyDown(KeyCode.F2)) GameManager.Instance.Play1stFloorFinalPainting();
        //if (Input.GetKeyDown(KeyCode.F3)) GameManager.Instance.PlaySecondFloorEnd();
        if (Input.GetKeyDown(KeyCode.F4)) PlayerControls.Instance.currentInteractedPuzzle.OnPuzzleComplete();

        // Scene tester
        //if (Input.GetKeyDown(KeyCode.F11)) GameManager.Instance.PlaySpecialRoom();
    }

    IEnumerator testCorou()
    {
        yield return null;
    }
}
