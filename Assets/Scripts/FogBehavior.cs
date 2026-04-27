using UnityEngine;

public class FogBehavior : MonoBehaviour
{
    [SerializeField] float fadeTime=0.5f;
    [SerializeField] RendererGroupAlpha walls;
    [SerializeField] RendererGroupAlpha[] doors;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) Show();
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) Hide();
    }

    void Show()
    {
        LeanTween.cancel(gameObject);
        LeanTween.alpha(gameObject, 0, fadeTime).setEaseOutBack();

        if (walls!=null)
        {
            LeanTween.cancel(walls.gameObject);
            LeanTween.value(walls.gameObject, 0, 1, fadeTime)
                        .setOnUpdate(val => walls.alpha = val).setEaseOutBack();
            
        }


        if (doors != null)
        {
            LeanTween.delayedCall(0.2f, () =>
            {
                foreach (var door in doors)
                {
                    //if (door.doLog) Debug.Log(door.gameObject.name + " SHOW");
                    LeanTween.cancel(door.gameObject);
                    LeanTween.value(door.gameObject, door.alpha, 1, fadeTime)
                                .setOnUpdate(val => door.alpha = val).setEaseOutBack();
                }
            });
        }
    }

    void Hide()
    {
        LeanTween.cancel(gameObject);
        LeanTween.alpha(gameObject, 1, fadeTime).setEaseOutBack();

        if (walls != null)
        {
            LeanTween.cancel(walls.gameObject);
            LeanTween.value(walls.gameObject, 1, 0, fadeTime)
                        .setOnUpdate(val => walls.alpha = val).setEaseOutBack();
        }



        if (doors != null)
        {
            foreach (var door in doors)
            {
                if (door.doLog) Debug.Log(door.gameObject.name + " HIDE");
                LeanTween.cancel(door.gameObject);
                LeanTween.value(door.gameObject, door.alpha, 0, fadeTime)
                            .setOnUpdate(val => door.alpha = val).setEaseOutBack();
            }
        }
    }
}
