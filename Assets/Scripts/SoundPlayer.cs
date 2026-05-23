using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] AudioClip sound;

    public void PlaySound()
    {
        AudioManager.Instance.PlaySFX(sound);
    }
}
