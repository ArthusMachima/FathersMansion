using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public bool gamePaused;
    [SerializeField] GameObject ToggleColorblind;
    [SerializeField] CanvasGroup PausePanel;

    public bool colorblindMode;
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
    [SerializeField] ItemClass LongKey;
    [SerializeField] int keyCounter=0;

    [Header("Cutscene Elements")]
    [SerializeField] GameObject Player;
    [SerializeField] Transform[] TransPoints;
    [SerializeField] Sprite[] CutsceneImages;

    [Header("True Ending Conditions")]
    public ItemClass[] SecondFloorSecretItems;
    public bool FoundAllSecretItemsInSecondFloor;


    void Start()
    {
        if (doCutscene) StartCoroutine(SceneSecondFloorStart());
        if (PlayerPrefs.GetInt("PlayCount", 0)==0)
        {
            
        }
        if (GameUI != null) GameUI.SetActive(true);
        AudioManager.Instance.PlayBGM(AudioManager.Instance.m_lullaby);
    }



    public static GameManager Instance;
    private void OnEnable()
    {
        Instance = this;
        if (PlayerPrefs.GetInt("colorblindMode", 0)==1) colorblindMode = true;
        else colorblindMode = false;
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
                AudioManager.Instance.SetBGMVolume((float)PlayerControls.Instance.MonsterDistance - 1 / 5, 0.5f);
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


        if (Input.GetKeyDown(KeyCode.Escape)) PauseGame(); 
    }



    //Function
    public void PauseGame()
    {
        gamePaused = !gamePaused;
        PausePanel.alpha = gamePaused?1:0;
        PausePanel.interactable = gamePaused;
        PausePanel.blocksRaycasts = gamePaused;
        PlayerControls.Instance.doPlayerControls = !gamePaused;
        PlayerControls.Instance.doPlayerAnimations = !gamePaused;
        if (gamePaused)
        {
            PlayerControls.Instance.StopPlayer();
        }

        ToggleColorblind.SetActive(!colorblindMode);
    }

    public void ToggleColorblindMode()
    {
        if (!colorblindMode)
        {
            colorblindMode = true;
            ToggleColorblind.SetActive(false);
        }
        else
        {
            colorblindMode = false;
            ToggleColorblind.SetActive(true);
        }
        PlayerPrefs.SetInt("colorblindMode", colorblindMode ? 1 : 0);
    }

    public void ReturnMainMenu()
    {
        PauseGame();
        LeanTween.value(FloorTransitionBlack.gameObject, 0, 1, 0.5f)
                .setOnUpdate(val => FloorTransitionBlack.alpha = val).setOnComplete(() =>
                {
                    SceneManager.LoadScene("MainMenu");
                });
    }

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
        SideScreenMessage.Instance.DisplayMessage("Objective", $"Get all keys ({keyCounter}/5)", 1.5f);
    }

    public void GetSecondFloorFinalKey()
    {
        foreach (var key in InventoryManager.Instance.items) if (key.PeekItem() is ItemKeyClass) key.TakeItem();
        foreach (var key in InventoryManager.Instance.keyItems) if (key.PeekItem() is ItemKeyClass) key.TakeItem();
        InventoryManager.Instance.TransferItem(LongKey, false);
        AddKey();
    }



    //Messages
    public void StarterMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Find a way out", 1.5f);
    }

    public void SlidingMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Find the puzzle piece and drag it here", 1.5f);
    }

    public void MatchingMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Find the right polaroid card and drag it here", 1.5f);
    }

    public void BookHueMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Find missing book and drag it here", 1.5f);
    }

    public void PaintingMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Find the right missing painting and arrange it correctly", 1.5f);
    }

    public void PinMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Enter the correct pin", 1.5f);
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



    




    public void PlaySecondFloorEnd()
    {
        PlayerControls.Instance.StaminaPanel.SetActive(false);
        StartCoroutine(SceneSecondFloorEnd());
    }

    IEnumerator SceneSecondFloorEnd()
    {
        //Fades out
        AudioManager.Instance.FadeStopBGM(0.5f);
        LeanTween.value(FloorTransitionBlack.gameObject, 0, 1, 0.5f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);
        yield return new WaitForSeconds(0.1f);

        //Disable controls
        PlayerControls.Instance.doPlayerControls = false;
        PlayerControls.Instance.doPlayerAnimations = false;
        yield return new WaitForSeconds(1f);

        //Move objects
        Camera.main.transform.SetParent(TransPoints[0], false);
        Player.transform.position = TransPoints[1].position;
        yield return new WaitForSeconds(1f);

        //Fades in
        LeanTween.value(FloorTransitionBlack.gameObject, 1, 0, 3f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);
        yield return new WaitForSeconds(1f);

        //Player moves to carpet
        Player.LeanMove(TransPoints[0].position, 2f).setOnComplete(() =>
        {
            PlayerControls.Instance.anim.SetBool("isMoving", false);
        });
        PlayerControls.Instance.anim.SetFloat("x", 0);
        PlayerControls.Instance.anim.SetFloat("y", -1);
        PlayerControls.Instance.anim.SetBool("isMoving", true);
        yield return new WaitForSeconds(3f);

        //Player looks around
        PlayerControls.Instance.anim.SetFloat("y", -0.5f);
        PlayerControls.Instance.anim.SetFloat("x", -1);
        yield return new WaitForSeconds(1);
        PlayerControls.Instance.anim.SetFloat("x", 1);
        yield return new WaitForSeconds(1);
        PlayerControls.Instance.anim.SetFloat("x", -1);
        yield return new WaitForSeconds(1);

        //Player goes to the mirror
        Player.LeanMove(TransPoints[2].position, 2f).setOnComplete(() =>
        {
            PlayerControls.Instance.anim.SetBool("isMoving", false);
        });
        PlayerControls.Instance.anim.SetFloat("x", 0);
        PlayerControls.Instance.anim.SetFloat("y", 1);
        PlayerControls.Instance.anim.SetBool("isMoving", true);
        yield return new WaitForSeconds(3f);

        //Dialogue
        Dialogue[] msg =
        {
            new("I began to stare at the mirror, before I could ask myself why.", null),
            new("From it, I see the figure staring right back at me, Just as confused as I was.", null), // cutscene image starts here
            new("I moved - she followed, she could mimic every single thing I'd try to do.", null),
            new("This is undoubtedly me, whom I've never met before.", null)
        };
        UIManager.Instance.LoadDialogue(msg);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);
        msg=null;

        yield return new WaitForSeconds(1);

        //Item check
        FoundAllSecretItemsInSecondFloor = SecondFloorSecretItems.All(secret =>
        InventoryManager.Instance.items.Any(slot => slot.PeekItem() == secret));

        //Branched Dialogue
        if (FoundAllSecretItemsInSecondFloor)
        {
            msg = new Dialogue[]
            {
            new("I noticed the weight my pockets were carrying as I stood there for a while.", null),
            new("Almost forgot I had these items. I wonder why I can feel a connection to them?", null), // image
            new("The next thing I know...", null),
            new("I wore the hat and posed in front of the mirror carrying a camera.", null), // cutscene image
            new("Then, I began to remember something...", null)
            };
            UIManager.Instance.LoadDialogue(msg);
            yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

            //White fade in
            FloorTransitionBlack.GetComponent<Image>().color = new(1, 1, 1);
            LeanTween.value(FloorTransitionBlack.gameObject, 0, 1, 2f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);
            yield return new WaitForSeconds(2f);

            //Dialogue
            msg = new Dialogue[]
            {
            new("It all started with the sound of an open door.", null),
            new("Then footsteps, and that newspaper was placed on my desk.", null),
            new("\"Please help me find my daughter, Miss Melania.\"", null),
            new("She came up to me with tears in her eyes as she said that.", null),
            new("I cursed my heart back then for seeing it only as a means to get by, instead of doing it for the sake of compassion.", null),
            new("I read the newspaper, put on my hat, and brought my camera with me.", null),
            new("I walked through the forest until the mansion came into view.", null),
            new("And then... then...", null),
            };
            UIManager.Instance.LoadDialogue(msg, UIManager.Instance.ScreenText);
            yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

            //White fade out
            FloorTransitionBlack.GetComponent<Image>().color = new(1, 1, 1);
            LeanTween.value(FloorTransitionBlack.gameObject, 1, 0, 1f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);
            yield return new WaitForSeconds(1f);

            //Dialogue
            msg = new Dialogue[]
            {
            new("I don't know what happened after that, but here I am.", null),
            new("I am Melania, a private investigator, paid by someone to investigate a mansion.", null),
            new("Now I know why I'm here.", null),
            };
            UIManager.Instance.LoadDialogue(msg, UIManager.Instance.dialogueText);
            yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        }
        else
        {
            msg = new Dialogue[]
            {
            new("...I just can't stand it", null),
            new("Why can't I even recognize myself?", null)
            };
            UIManager.Instance.LoadDialogue(msg);
            yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);
        }



        //Fades out
        FloorTransitionBlack.GetComponent<Image>().color = new(0, 0, 0);
        LeanTween.value(FloorTransitionBlack.gameObject, 0, 1, 0.5f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);
        yield return new WaitForSeconds(1f);

        //Restore cam
        Camera.main.transform.SetParent(Player.transform, false);

        //Fades in
        LeanTween.value(FloorTransitionBlack.gameObject, 1, 0, 0.5f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);
        yield return new WaitForSeconds(0.5f);

        //Restore controls
        PlayerControls.Instance.doPlayerControls = true;
        PlayerControls.Instance.doPlayerAnimations = true;
    }
}
