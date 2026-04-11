using UnityEngine;

public class ItemObject : MonoBehaviour, IInteractable
{
    public ItemClass item;
    [SerializeField] Sprite icon;

    public void Interact()
    {
        Debug.Log("Interacted");
        PlayerControls.Instance.TakeItem(this);
    }

    private void Start()
    {
        icon = gameObject.GetComponent<Sprite>();
        icon = item.itemIcon;
    }


}
