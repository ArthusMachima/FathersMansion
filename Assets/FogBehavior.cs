using UnityEngine;

public class FogBehavior : MonoBehaviour
{
    [SerializeField] GameObject[] SurroundingFog;
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
        gameObject.LeanAlpha(0, fadeTime);
        if (SurroundingFog.Length == 0) return;
        foreach (GameObject fog in SurroundingFog)
        {
            LeanTween.cancel(fog);
            fog.LeanAlpha(0, fadeTime);
        }
    }

    void Hide()
    {
        LeanTween.cancel(gameObject);
        gameObject.LeanAlpha(1, fadeTime);
        if (SurroundingFog.Length == 0) return;
        foreach (GameObject fog in SurroundingFog)
        {
            LeanTween.cancel(fog);
            fog.LeanAlpha(1, fadeTime);
        }
    }
}
