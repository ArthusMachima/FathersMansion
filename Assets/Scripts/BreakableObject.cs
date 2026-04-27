using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [SerializeField] ItemClass storedItem;
    [SerializeField] Sprite openedBox;
    SpriteRenderer sprite;
    BoxCollider2D col;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
    }

    public void BreakContainer()
    {
        col.enabled = false;
        sprite.sprite = openedBox;
        if (storedItem != null)
        {
            GameObject itemGameObj = new("Item");
            itemGameObj.AddComponent<SpriteRenderer>();
            itemGameObj.AddComponent<ItemObject>();
            itemGameObj.GetComponent<ItemObject>().item = storedItem;
            itemGameObj.AddComponent<BoxCollider2D>().GetComponent<BoxCollider2D>().size = new(1, 1);
            itemGameObj.transform.position = transform.position;
            itemGameObj.GetComponent<ItemObject>().RefreshObject();
        }
    }
}
