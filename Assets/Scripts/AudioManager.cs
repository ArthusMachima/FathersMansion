using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private void OnEnable() => Instance = this;

    [Header("BGM")]
    [SerializeField] AudioSource bgmIntroSource;
    [SerializeField] AudioSource bgmLoopSource;

    [Header("SFX")]
    [SerializeField] AudioSource sfxSource;
    [SerializeField] float sfxCooldown = 0.1f;
    private float sfxCooldownTimer;
    private Coroutine bgmCoroutine;

    [Header("Clips")]
    public AudioClip m_lullaby;




    private void Update()
    {
        if (sfxCooldownTimer > 0)
            sfxCooldownTimer -= Time.deltaTime;
    }


    // BGM
    public void PlayBGM(AudioClip intro, AudioClip loop, float volume = 1f)
    {
        if (loop == null) return;
        if (bgmCoroutine != null) StopCoroutine(bgmCoroutine);

        bgmIntroSource.Stop();
        bgmLoopSource.Stop();

        bgmIntroSource.volume = volume;
        bgmLoopSource.volume = volume;
        bgmLoopSource.loop = true;

        bgmCoroutine = StartCoroutine(PlayBGMSequence(intro, loop));
    }

    // Overload for BGM with no intro
    public void PlayBGM(AudioClip loop, float volume = 1f)
    {
        PlayBGM(null, loop, volume);
    }

    IEnumerator PlayBGMSequence(AudioClip intro, AudioClip loop)
    {
        if (intro != null)
        {
            bgmIntroSource.clip = intro;

            // Schedule both clips so they are sample-accurate with no gap
            double startTime = AudioSettings.dspTime + 0.1;
            bgmIntroSource.PlayScheduled(startTime);
            bgmLoopSource.clip = loop;
            bgmLoopSource.PlayScheduled(startTime + intro.length);

            yield return new WaitForSeconds((float)(startTime - AudioSettings.dspTime) + intro.length);
        }
        else
        {
            bgmLoopSource.clip = loop;
            bgmLoopSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmCoroutine != null) StopCoroutine(bgmCoroutine);
        bgmIntroSource.Stop();
        bgmLoopSource.Stop();
    }

    public void SetBGMVolume(float volume)
    {
        bgmIntroSource.volume = Mathf.Clamp01(volume);
        bgmLoopSource.volume = Mathf.Clamp01(volume);
    }


    // SFX
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        if (sfxCooldownTimer > 0) return;

        sfxSource.PlayOneShot(clip, volume);
        sfxCooldownTimer = sfxCooldown;
    }

    public void PlaySFX(AudioClip clip, float volume, float cooldownOverride)
    {
        if (clip == null) return;
        if (sfxCooldownTimer > 0) return;

        sfxSource.PlayOneShot(clip, volume);
        sfxCooldownTimer = cooldownOverride;
    }
}