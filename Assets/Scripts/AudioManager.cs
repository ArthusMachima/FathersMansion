using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private void OnEnable() => Instance = this;

    [Header("BGM")]
    [SerializeField] AudioSource bgmIntroSource;
    [SerializeField] AudioSource bgmLoopSource;
    [SerializeField] Slider bgmSlider;

    [Header("SFX")]
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource sfxSoundTest;
    [SerializeField] float sfxCooldown = 0.1f;
    [SerializeField] Slider sfxSlider;
    private float sfxCooldownTimer;
    private Coroutine bgmCoroutine;

    [Header("Clips")]
    public AudioClip m_lullaby;
    public AudioClip m_MainMenu;
    public AudioClip m_2ndFloorEnd;
    public AudioClip m_1stFloorEnd;
    public AudioClip m_BasementEnd;
    public AudioClip m_HE;
    public AudioClip m_SHE;
    public AudioClip m_Credits;
    [Space]
    public AudioClip s_jumpscare1;
    public AudioClip s_jumpscare2;
    public AudioClip s_jumpscare3;
    [Space]
    public AudioClip s_DoorOpen;
    public AudioClip s_DoorClose;
    public AudioClip s_DoorLocked;
    public AudioClip s_DoorUnlock;
    [Space]
    public AudioClip s_Pick;
    public AudioClip s_Place;
    [Space]
    public AudioClip s_ItemPickUp;
    public AudioClip s_ItemWorth;
    [Space]
    public AudioClip s_CardFlip;
    public AudioClip s_CardFlipBack;
    public AudioClip s_Padlock;
    public AudioClip s_PinType;
    public AudioClip s_PinCorrect;
    public AudioClip s_PinIncorrect;
    [Space]
    public AudioClip s_DialogueTyping;




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

    public void FadeStopBGM(float dur)
    {
        if (bgmCoroutine != null) StopCoroutine(bgmCoroutine);

        LeanTween.value(gameObject, bgmIntroSource.volume, 0f, dur)
            .setOnUpdate(vol => bgmIntroSource.volume = vol).setOnComplete(() =>
            {
                bgmIntroSource.Stop();
            });

        LeanTween.value(gameObject, bgmLoopSource.volume, 0f, dur)
            .setOnUpdate(vol => bgmLoopSource.volume = vol).setOnComplete(() =>
            {
                bgmLoopSource.Stop();
            });
    }

    public void SetBGMVolume(float volume, float dur)
    {
        LeanTween.value(gameObject, bgmIntroSource.volume, volume, dur)
            .setOnUpdate(vol => bgmIntroSource.volume = vol);
        LeanTween.value(gameObject, bgmLoopSource.volume, volume, dur)
            .setOnUpdate(vol => bgmLoopSource.volume = vol);
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

    public void SyncBGMVolume()
    {
        bgmIntroSource.volume = bgmSlider.value;
        bgmLoopSource.volume = bgmSlider.value;
    }

    public void SyncSFXVolume()
    {
        sfxSource.volume = sfxSlider.value;
        sfxSoundTest.volume = sfxSlider.value;
    }

    public void SFXSoundTest(bool play)
    {
        if (play)
        {
            if (!sfxSoundTest.isPlaying)
            {
                sfxSoundTest.loop = true;
                sfxSoundTest.Play();
            }
        }
        else sfxSoundTest.Stop();
    }
}
