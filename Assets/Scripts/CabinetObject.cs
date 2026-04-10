using UnityEngine;

public class CabinetObject : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        UIManager.Instance.ShowExamineCabinet(true);
    }




}
