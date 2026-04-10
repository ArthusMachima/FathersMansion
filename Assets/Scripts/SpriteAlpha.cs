using UnityEngine;

public class SpriteAlpha : MonoBehaviour
{
    [SerializeField] float duration=0.5f; 
    SpriteRenderer render;
    private void Start()
    {
        render = GetComponent<SpriteRenderer>();
    }

    public void SetAlpha(float num)
    {
        LeanTween.cancel(gameObject);
        LeanTween.alpha(gameObject, num, duration).setEaseInBack();
    }
}
