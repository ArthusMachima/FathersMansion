using System.Linq;
using UnityEngine;

public class RoomArea : MonoBehaviour
{
    [SerializeField] RendererGroupAlpha alpha;
    [SerializeField] float dur = 0.3f;
    [SerializeField] Collider2D[] colliders;


    private void Start()
    {
        alpha = GetComponent<RendererGroupAlpha>();
        colliders = GetComponentsInChildren<Collider2D>()
            .Where(t => t != gameObject.GetComponent<Collider2D>()).ToArray();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        LeanTween.value(gameObject, alpha.alpha, 1, dur)
                    .setOnUpdate(val => alpha.alpha = val).setOnComplete(() =>
                    {
                        foreach (var collider in colliders)
                        {
                            if (collider.TryGetComponent<BreakableObject>(out var breakable))
                            {
                                if (!breakable.isBroke)
                                {
                                    collider.enabled = true;
                                }
                                else
                                {
                                    collider.enabled = false;
                                }
                            }
                            else collider.enabled = true;
                        }
                    });
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        LeanTween.value(gameObject, alpha.alpha, 0, dur)
                    .setOnUpdate(val => alpha.alpha = val).setOnComplete(() =>
                    {
                        foreach (var collider in colliders) collider.enabled = false;
                    });
    }
}
