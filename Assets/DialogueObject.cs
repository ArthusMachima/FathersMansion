using UnityEngine;

public class DialogueObject : MonoBehaviour, IInteractable
{
    [SerializeField] string[] dialogue;

    public void Interact()
    {
        UIManager.Instance.LoadDialogue(dialogue);
    }


}
