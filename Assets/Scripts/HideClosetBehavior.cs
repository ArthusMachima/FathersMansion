
using System.Collections;
using UnityEngine;

public class HideClosetBehavior : MonoBehaviour
{
    [Header("Hide Closet Panel")]
    [SerializeField] GameObject HideClosetPanel;
    [SerializeField] Transform ClosetDoor;
    [SerializeField] CanvasGroup MonsterSprite;
    public float ClosetHP = 100;
    [SerializeField] bool hasMonsterArrived;
    [SerializeField] bool MonsterAboutToPunch;
    [SerializeField] bool MonsterEntered;
    [SerializeField] float monsterMoveRange;

    //Singleton
    public static HideClosetBehavior Instance;
    private void OnEnable()
    {
        Instance = this;
    }

    private void Start()
    {
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
            if (PlayerControls.Instance.MonsterDistance<=0)
            {
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
        ClosetHP -= Random.Range(10, 65);
        //TODO: door punch sfx
        MonsterAboutToPunch = false;
    }

    IEnumerator MonsterLeaveTimer()
    {
        yield return new WaitForSeconds(Random.Range(4,10));
        PlayerControls.Instance.MonsterDistance = 1;
    }
}
