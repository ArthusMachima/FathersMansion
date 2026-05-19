using UnityEngine;

public class ClosetObject : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        HideClosetBehavior.Instance.ShowClosetPanel(true);
        PlayerControls.Instance.ClosetHideMode(true);
    }
}
