using UnityEngine;

public class ClosetObject : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        HideClosetBehavior.Instance.gameObject.SetActive(true);
        PlayerControls.Instance.ClosetHideMode(true);
    }
}
