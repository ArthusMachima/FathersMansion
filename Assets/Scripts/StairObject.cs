using UnityEngine;
using UnityEngine.Events;

public class StairObject : MonoBehaviour, IInteractable
{
    [SerializeField] int GoToFloor;
    [SerializeField] UnityEvent onInteract;

    public void Interact()
    {
        if (onInteract.GetPersistentEventCount()>0)
        {
            onInteract.Invoke();
            return;
        }

        GameManager.Instance.SwitchFloors(GoToFloor);
    }

    public void doSwitchFloor()
    {
        GameManager.Instance.SwitchFloors(GoToFloor);
    }



}
