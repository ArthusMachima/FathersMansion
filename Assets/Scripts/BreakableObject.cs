using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [SerializeField] ItemClass storedItem;
    [SerializeField] Sprite openedBox;
    [SerializeField] ItemClass requiredItem;
    [SerializeField] AudioClip BreakSound;
    SpriteRenderer sprite;
    BoxCollider2D col;
    public bool isBroke;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
    }

    public void BreakContainer()
    {
        if (requiredItem!=null && InventoryManager.Instance.heldItem != requiredItem) return;

        col.enabled = false;
        sprite.sprite = openedBox;
        isBroke = true;
        AudioManager.Instance.PlaySFX(BreakSound);
        if (storedItem != null)
        {
            GameObject itemGameObj = new("Item");
            itemGameObj.AddComponent<SpriteRenderer>();
            itemGameObj.AddComponent<ItemObject>();
            itemGameObj.GetComponent<ItemObject>().item = storedItem;
            itemGameObj.AddComponent<BoxCollider2D>().GetComponent<BoxCollider2D>().size = new(1, 1);
            itemGameObj.transform.SetParent(transform, false);
            itemGameObj.GetComponent<ItemObject>().RefreshObject();
        }
    }
}
