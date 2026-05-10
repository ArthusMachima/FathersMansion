using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;

public class ItemChecker : MonoBehaviour
{
    [SerializeField] ItemClass itemToCheck;
    [SerializeField] UnityEvent onItemMatch;
    [SerializeField] bool makeItTransparent;
    SpriteRenderer render;

    private void Start()
    {
        render = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (render != null && makeItTransparent) render.color = new(0, 0, 0, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.gameObject.TryGetComponent<PlayerControls>(out PlayerControls player);
        if (player != null)
        {
            bool found = false;
            InventoryManager inv = InventoryManager.Instance;
            foreach (var slot in inv.items)
            {
                if (slot.PeekItem()==itemToCheck)
                {
                    found=true;
                }
            }

            if (found)
            {
                Debug.Log("Found");
                onItemMatch.Invoke();
            }
        }
    }
}
