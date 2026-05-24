using System.Collections;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField] bool disableTriggers;

    void Update()
    {
        if (disableTriggers) return;

        // Debug
        if (Input.GetKeyDown(KeyCode.F1)) PlayerControls.Instance.MonsterDistance = 0;
        if (Input.GetKeyDown(KeyCode.F2)) GameManager.Instance.Play1stFloorFinalPainting();
        if (Input.GetKeyDown(KeyCode.F3)) GameManager.Instance.PlaySecondFloorEnd();
        if (Input.GetKeyDown(KeyCode.F4)) PlayerControls.Instance.currentInteractedPuzzle.OnPuzzleComplete();
        if (Input.GetKeyDown(KeyCode.F5)) GameManager.Instance.PlayNormalEscape();


        if (Input.GetKeyDown(KeyCode.F6)) GameManager.Instance.DoBasementScreenEffects(true);
        if (Input.GetKeyDown(KeyCode.F7)) GameManager.Instance.DoBasementScreenEffects(false);

    }

    IEnumerator testCorou()
    {
        yield return null;
    }
}
