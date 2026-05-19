
using System.Collections;
using UnityEngine;

public class HideClosetBehavior : MonoBehaviour
{
    [Header("Hide Closet Panel")]
    [SerializeField] CanvasGroup HideClosetPanel;
    [SerializeField] Transform ClosetDoor;
    [SerializeField] CanvasGroup MonsterSprite;
    public float ClosetHP = 100;
    [SerializeField] bool hasMonsterArrived;
    [SerializeField] bool MonsterAboutToPunch;
    [SerializeField] bool MonsterEntered;
    [SerializeField] float monsterMoveRange;

    //Singleton
    public static HideClosetBehavior Instance;
    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }


    private void Update()
    {

        ClosetDoor.transform.position = Vector3.Lerp(ClosetDoor.position, new(-(Screen.width * ((100 - ClosetHP) / 100)), 0, 0), Time.deltaTime * 2);
        MonsterSprite.transform.position = Vector3.Lerp(MonsterSprite.transform.position, new(monsterMoveRange, 0, 0), Time.deltaTime * 5);

        if (!PlayerControls.Instance.isPlayerHiddenInCloset || MonsterEntered) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClosetHP += 2;
            if (ClosetHP > 100) ClosetHP = 100;
        }

        if (hasMonsterArrived)
        {
            if (PlayerControls.Instance.MonsterDistance > 0)
            {
                AudioManager.Instance.PlayBGM(AudioManager.Instance.m_lullaby);
                LeanTween.value(MonsterSprite.gameObject, 1, 0, 0.3f)
                    .setOnUpdate(val => MonsterSprite.alpha = val);
                hasMonsterArrived = false;
            }

            if (ClosetHP<=0)
            {
                monsterMoveRange = Screen.width / 2;
                LeanTween.delayedCall(1, () =>
                {
                    GameManager.Instance.Jumpscare();
                });
                MonsterEntered = true;
            }
            else ClosetHP -= 10f * Time.deltaTime;

            if (!MonsterAboutToPunch)
            {
                MonsterAboutToPunch = true;
                StartCoroutine(MonsterPunch(Random.Range(0.5f, 3f)));
            }
        }
        else
        {
            ClosetHP = 100;
            if (PlayerControls.Instance.MonsterDistance<=0)
            {
                AudioManager.Instance.PlayBGM(AudioManager.Instance.s_Heartbeat);
                LeanTween.value(MonsterSprite.gameObject, 0, 1, 2)
                    .setOnUpdate(val => MonsterSprite.alpha = val);
                StartCoroutine(MonsterLeaveTimer());
                hasMonsterArrived = true;
            }
        }
    }

    IEnumerator MonsterPunch(float delay)
    {
        if (MonsterEntered) yield break;
        monsterMoveRange = (Screen.width/2)+Random.Range(-400, 400);
        yield return new WaitForSeconds(delay);
        ClosetHP -= Random.Range(10, 40);
        switch (Random.Range(1,4))
        {
            case 1:
                AudioManager.Instance.PlaySFX(AudioManager.Instance.s_Noise1); break;
            case 2:
                AudioManager.Instance.PlaySFX(AudioManager.Instance.s_Noise2); break;
            case 3:
                AudioManager.Instance.PlaySFX(AudioManager.Instance.s_Noise3); break;
            default:
                Debug.Log("ERROR OUT OF RANGE");
                break;
        }
        MonsterAboutToPunch = false;
    }

    IEnumerator MonsterLeaveTimer()
    {
        yield return new WaitForSeconds(Random.Range(4,10));
        PlayerControls.Instance.MonsterDistance = 5;
    }

    public void ShowClosetPanel(bool show)
    {
        if (show) gameObject.SetActive(true);
        ClosetHP = 100;
        LeanTween.value(gameObject, HideClosetPanel.alpha, show?1:0, 0.3f)
            .setOnUpdate(val => HideClosetPanel.alpha = val).setOnComplete(() =>
            {
                if (!show) gameObject.SetActive(false);
            });
        HideClosetPanel.blocksRaycasts = show;
        HideClosetPanel.interactable = show;
        if (show)
        {
            hasMonsterArrived = false;
            MonsterEntered = false;
            MonsterAboutToPunch = false;
        }
    }
}
