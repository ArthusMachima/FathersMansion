using UnityEngine;
using UnityEngine.U2D;

public class SpriteAutoSorter : MonoBehaviour
{
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] int backLayerIndex;
    [SerializeField] int frontLayerIndex;
    [SerializeField] Vector3 pivotOffset;

    private void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 offset = transform.position - PlayerControls.Instance.transform.position + pivotOffset;
        if (offset.y > 0) sprite.sortingOrder = frontLayerIndex;
        else sprite.sortingOrder = backLayerIndex;
    }




}
