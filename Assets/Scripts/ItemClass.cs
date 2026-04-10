using UnityEngine;

public abstract class ItemClass : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;

    public void UseItem()
    {
        Debug.Log("Item used");
    }
}
