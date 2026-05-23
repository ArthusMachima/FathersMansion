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
    [SerializeField] AudioSource bgmCrossfadeSource;
    public Slider bgmSlider;

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
    public AudioClip s_UIConfirm;
    public AudioClip s_UICancel;
    [Space]
    public AudioClip s_OfficeDoor;
    public AudioClip s_Paper1;
    public AudioClip s_Paper2;
    public AudioClip s_Puke;
    [Space]
    public AudioClip s_Heartbeat;
    public AudioClip s_Noise1;
    public AudioClip s_Noise2;
    public AudioClip s_Noise3;
    [Space]
    public AudioClip s_VaseBreak;
    public AudioClip s_BoxRip;


    private void Start()
    {
        bgmIntroSource.volume = PlayerPrefs.GetFloat("bgmVol", 0.7f);
        bgmLoopSource.volume = PlayerPrefs.GetFloat("bgmVol", 0.7f);
        if (bgmSlider!=null) bgmSlider.value = PlayerPrefs.GetFloat("bgmVol", 0.7f);

        sfxSource.volume = PlayerPrefs.GetFloat("sfxVol", 0.7f);
        sfxSoundTest.volume = PlayerPrefs.GetFloat("sfxVol", 0.7f);
        if (sfxSlider!= null) sfxSlider.value = PlayerPrefs.GetFloat("sfxVol", 0.7f);
    }

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

    public void FadePlayBGM(float dur)
    {
        float targetVolume = bgmSlider != null ? bgmSlider.value : 1f;

        if (!bgmIntroSource.isPlaying) bgmIntroSource.Play();
        if (!bgmLoopSource.isPlaying) bgmLoopSource.Play();

        LeanTween.value(gameObject, bgmIntroSource.volume, targetVolume, dur)
            .setOnUpdate(vol => bgmIntroSource.volume = vol);

        LeanTween.value(gameObject, bgmLoopSource.volume, targetVolume, dur)
            .setOnUpdate(vol => bgmLoopSource.volume = vol);
    }

    public void CrossFadeBGM(AudioClip loop, float dur, AudioClip intro = null, float volume = -1f)
    {
        if (loop == null) return;
        if (bgmCoroutine != null) StopCoroutine(bgmCoroutine);

        float targetVolume = (volume < 0f ? (bgmSlider != null ? bgmSlider.value : 1f) : volume);

        // Fade out current sources
        LeanTween.value(gameObject, bgmIntroSource.volume, 0f, dur)
            .setOnUpdate(vol => bgmIntroSource.volume = vol)
            .setOnComplete(() => bgmIntroSource.Stop());

        LeanTween.value(gameObject, bgmLoopSource.volume, 0f, dur)
            .setOnUpdate(vol => bgmLoopSource.volume = vol)
            .setOnComplete(() => bgmLoopSource.Stop());

        // Fade in on crossfade source, then hand off to the proper sources
        bgmCrossfadeSource.clip = loop;
        bgmCrossfadeSource.loop = intro == null; // loops immediately only if no intro
        bgmCrossfadeSource.volume = 0f;
        bgmCrossfadeSource.Play();

        LeanTween.value(gameObject, 0f, targetVolume, dur)
            .setOnUpdate(vol => bgmCrossfadeSource.volume = vol)
            .setOnComplete(() =>
            {
                bgmCrossfadeSource.Stop();
                bgmCrossfadeSource.clip = null;

                // Hand off to the normal intro+loop sources
                bgmIntroSource.volume = targetVolume;
                bgmLoopSource.volume = targetVolume;
                bgmCoroutine = StartCoroutine(PlayBGMSequence(intro, loop));
            });
    }

    public void SetBGMVolume(float volume, float dur)
    {
        LeanTween.value(gameObject, bgmIntroSource.volume, volume*bgmSlider.value, dur)
            .setOnUpdate(vol => bgmIntroSource.volume = vol);
        LeanTween.value(gameObject, bgmLoopSource.volume, volume*bgmSlider.value, dur)
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
        PlayerPrefs.SetFloat("bgmVol", bgmSlider.value);
    }

    public void SyncSFXVolume()
    {
        sfxSource.volume = sfxSlider.value;
        sfxSoundTest.volume = sfxSlider.value;
        PlayerPrefs.SetFloat("sfxVol", sfxSlider.value);
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

    public bool IsBGMPlaying()
    {
        return bgmLoopSource.isPlaying || bgmIntroSource.isPlaying;
    }
}
