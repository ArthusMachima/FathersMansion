using UnityEngine;

public class DoorObject : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        //UIManager.Instance.ShowExamineDoor(true);
    }




    void OpenDoor()
    {
        gameObject.SetActive(false);
    }
}
