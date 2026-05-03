using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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


    void Start()
    {
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



    //Messages
    public void TestSideMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Talk to blue guy", 1.5f);
    }

    public void StarterMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Find a way out", 1.5f);
    }

    public void MatchingMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Find a polaroid card and drag it here", 1.5f);
    }

}
