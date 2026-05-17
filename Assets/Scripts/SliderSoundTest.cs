using UnityEngine;
using UnityEngine.EventSystems;

public class SliderSoundTest : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.Instance.SFXSoundTest(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AudioManager.Instance.SFXSoundTest(false);
    }
}
