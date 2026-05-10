using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject[] Floors;
    [SerializeField] int floorIndex;
    [SerializeField] GameObject JumpscarePanel;
    [SerializeField] bool isJumpscared;
    [SerializeField] bool isMonsterSpawned;
    [SerializeField] CanvasGroup FloorTransitionBlack;
    [SerializeField] CanvasGroup MonsterDarkness;
    [SerializeField] GameObject GameUI;
    public MonsterBehavior Monster;
    [SerializeField] Transform MonsterSpawnPoint;
    [SerializeField] float monsterApproachCooldown=10;
    [SerializeField] int prevDistance;
    [SerializeField] bool doCutscene=true;

    [Header("2nd Floor")]
    [SerializeField] int keyCounter=0;

    [Header("Cutscene Elements")]
    [SerializeField] GameObject Player;
    [SerializeField] Transform[] TransPoints;


    void Start()
    {
        if (PlayerPrefs.GetInt("PlayCount", 0)==0)
        {
            if (doCutscene) StartCoroutine(SceneSecondFloorStart());
        }
        if (GameUI != null) GameUI.SetActive(true);
        AudioManager.Instance.PlayBGM(AudioManager.Instance.m_lullaby);
    }



    public static GameManager Instance;
    private void OnEnable()
    {
        Instance = this;
    }



    private void Update()
    {
        if (PlayerControls.Instance.MonsterDistance!=prevDistance)
        {
            if (MonsterDarkness!=null)
            {
                Debug.Log("value changed");
                float o = 1f - ((float)PlayerControls.Instance.MonsterDistance / 5) - 0.05f;
                LeanTween.cancel(PlayerControls.Instance.gameObject);
                LeanTween.value(MonsterDarkness.gameObject, MonsterDarkness.alpha, o, 0.5f)
                       .setOnUpdate(val => MonsterDarkness.alpha = val);
                AudioManager.Instance.SetBGMVolume((float)PlayerControls.Instance.MonsterDistance - 1 / 5);
                prevDistance = PlayerControls.Instance.MonsterDistance;
            }
        }

        if (floorIndex!=2)
        {
            if (PlayerControls.Instance.MonsterDistance > 0)
            {
                if (isMonsterSpawned)
                {
                    SpawnMonster(false);
                    isMonsterSpawned = false;
                }

                if (monsterApproachCooldown>0)
                {
                    monsterApproachCooldown -= Time.deltaTime;
                }
                else
                {
                    PlayerControls.Instance.MonsterDistance--;
                    monsterApproachCooldown = Random.Range(3,10);
                }
            }
            else if (PlayerControls.Instance.MonsterDistance <= 0)
            {
                if (PlayerControls.Instance.MonsterDistance < 0)
                    PlayerControls.Instance.MonsterDistance = 0;

                if (!isMonsterSpawned)
                {
                    SpawnMonster(true);
                    isMonsterSpawned = true;
                }
            }
        }
    }



    //Function
    public void SwitchFloors(int floor)
    {
        floorIndex = floor;

        LeanTween.value(FloorTransitionBlack.gameObject, 0, 1, 0.5f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val).setOnComplete(() =>
                    {
                        foreach (var f in Floors) f.SetActive(false);
                        Floors[floor].SetActive(true);
                        LeanTween.value(FloorTransitionBlack.gameObject, 1, 0, 0.5f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);
                        if (floor==2)
                            MonsterDarkness.gameObject.SetActive(false);
                        else
                            MonsterDarkness.gameObject.SetActive(true);
                    });
    }

    public void Jumpscare()
    {
        if (isJumpscared) return;
        JumpscarePanel.SetActive(true);
        switch (Random.Range(0,3))
        {
            case 0:
                {
                    AudioManager.Instance.PlayBGM(AudioManager.Instance.s_jumpscare1);
                    break;
                }
            case 1:
                {
                    AudioManager.Instance.PlayBGM(AudioManager.Instance.s_jumpscare2);
                    break;
                }
            case 2:
                {
                    AudioManager.Instance.PlayBGM(AudioManager.Instance.s_jumpscare3);
                    break;
                }
        }
        isJumpscared = true;
        LeanTween.delayedCall(2, () =>
        {
            SceneManager.LoadScene("GameOver");
        });
    }

    public void SpawnMonster(bool spawn)
    {
        if (Monster == null) return;
        if (spawn)
        {
            Monster.agent.isStopped = false;
            Monster.gameObject.SetActive(true);
            Monster.transform.position = MonsterSpawnPoint.position;
        }
        else
        {
            Monster.gameObject.SetActive(false);
        }
    }

    public void AddKey()
    {
        keyCounter++;
    }



    //Messages
    public void StarterMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Find a way out", 1.5f);
    }

    public void MatchingMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Find a polaroid card and drag it here", 1.5f);
    }



    //Cutscenes

    IEnumerator SceneSecondFloorStart()
    {
        PlayerControls.Instance.anim.SetFloat("y", -1);
        PlayerControls.Instance.doPlayerControls = false;
        FloorTransitionBlack.alpha = 1;

        LeanTween.value(gameObject, 1, 0, 6)
            .setOnUpdate(val => FloorTransitionBlack.alpha = val);

        yield return new WaitForSeconds(1);
        PlayerControls.Instance.anim.Play("WalkingUp");
        yield return null;
        float animationLength = PlayerControls.Instance.anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength);
        PlayerControls.Instance.anim.Play("Idle");
        Dialogue[] msg = new Dialogue[]
        {
            new("...a room?", null),
            new("Where am I?", null)
        };
        UIManager.Instance.LoadDialogue(msg);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);
        SideScreenMessage.Instance.DisplayMessage("Objective", "Find a way out", 1.5f);
        PlayerControls.Instance.doPlayerControls = true;
    }



    public void PlayKeyAssemble()
    {
        StartCoroutine(SceneKeyAssemble());
    }

    IEnumerator SceneKeyAssemble()
    {
        if (keyCounter >= 5) yield break;

        if (keyCounter<4)
        {
            Debug.Log("KEY INCOMPLETE CUTSCENE");
        }
        else
        {
            Debug.Log("KEY COMPLETE CUTSCENE");
            yield return null;
            //todo key assemble cutscene
            keyCounter++;
        }
    }



    public void PlaySecondFloorEnd()
    {
        StartCoroutine(SceneSecondFloorEnd());
    }

    IEnumerator SceneSecondFloorEnd()
    {
        LeanTween.value(FloorTransitionBlack.gameObject, 0, 1, 0.5f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);

        yield return new WaitForSeconds(0.1f);
        PlayerControls.Instance.doPlayerControls = false;
        PlayerControls.Instance.doPlayerAnimations = false;

        yield return new WaitForSeconds(1f);
        Camera.main.transform.SetParent(TransPoints[0], false);
        Player.transform.position = TransPoints[1].position;
        yield return new WaitForSeconds(1f);
        LeanTween.value(FloorTransitionBlack.gameObject, 1, 0, 2f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);



        yield return new WaitForSeconds(3f);
        Camera.main.transform.SetParent(Player.transform, false);

        PlayerControls.Instance.doPlayerControls = true;
        PlayerControls.Instance.doPlayerAnimations = true;
    }
}
