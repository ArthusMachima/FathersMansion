using UnityEngine;

public class FogBehavior : MonoBehaviour
{
    [SerializeField] float fadeTime=0.5f;



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Show();
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Hide();
        }
    }

    void Show()
    {
        LeanTween.cancel(gameObject);
        LeanTween.alpha(gameObject, 0, fadeTime).setEaseOutBack();
    }

    void Hide()
    {
        LeanTween.cancel(gameObject);
        LeanTween.alpha(gameObject, 1, fadeTime).setEaseOutBack();
    }
}
