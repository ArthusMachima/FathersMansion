using UnityEngine;

public class StairObject : MonoBehaviour, IInteractable
{
    [SerializeField] int GoToFloor;
    public void Interact()
    {
        GameManager.Instance.SwitchFloors(GoToFloor);
    }
}
