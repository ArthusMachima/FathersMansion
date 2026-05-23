using System;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public AudioClip currentBGM;
    [SerializeField] CanvasGroup PlayerWalkingScene;
    [SerializeField] GameObject GameOverUI;
    [SerializeField] Transform Map;
    [SerializeField] RendererGroupAlpha[] RoomAlpha;
    [SerializeField] RendererGroupAlpha[] starterRooms;
    [SerializeField] Transform[] FloorSpawnPoints;
    public bool gamePaused;
    [SerializeField] GameObject ToggleColorblind;
    [SerializeField] CanvasGroup PausePanel;
    public bool colorblindMode;
    [SerializeField] GameObject[] Floors;
    [SerializeField] int floorIndex;
    public GameObject JumpscarePanel;
    public bool isJumpscared;
    [SerializeField] bool isMonsterSpawned;
    [SerializeField] CanvasGroup FloorTransitionBlack;
    [SerializeField] CanvasGroup MonsterDarkness;
    [SerializeField] GameObject GameUI;
    public MonsterBehavior Monster;
    [SerializeField] Transform MonsterSpawnPoint;
    [SerializeField] float monsterApproachCooldown=10;
    [SerializeField] int prevDistance;
    [SerializeField] bool doCutscene=true;
    [SerializeField] bool doMonsterSpawn=true;

    [Header("2nd Floor")]
    [SerializeField] ItemClass LongKey;
    [SerializeField] int keyCounter=0;

    [Header("Cutscene Elements")]
    public bool cutsceneMode;
    public GameObject Player;
    public Transform[] TransPoints;
    [SerializeField] Sprite[] CutsceneImages;
    [SerializeField] GameObject[] CutsceneObjects;

    [Header("True Ending Conditions")]
    public ItemClass[] SecondFloorSecretItems;
    public bool FoundAllSecretItemsInSecondFloor;

    [Header("Basement")]
    [SerializeField] Transform crossroadReturnPoint;
    [SerializeField] ShaderEffect_BleedingColors ScreenEffectsHazing;
    [SerializeField] ShaderEffect_Tint ScreenEffectsHue;
    [SerializeField] ShaderEffect_CorruptedVram ScreenEffectsWipe;

    [Header("Tutorial")]
    [SerializeField] bool informedOnMonster;



    void Start()
    {
        RoomAlpha = Map.GetComponentsInChildren<RendererGroupAlpha>(true);
        if (GameUI != null) GameUI.SetActive(true);
        StartGame();
    }

    void StartGame()
    {
        if (PlayerPrefs.GetInt("PlayCount", 0) == 0)
        {
            if (doCutscene) StartCoroutine(SceneSecondFloorStart());
        }
        else LoadLocation();
        currentBGM = AudioManager.Instance.m_lullaby;
        AudioManager.Instance.PlayBGM(currentBGM);
    }

    public void LoadLocation()
    {
        int index = PlayerPrefs.GetInt("savedFloor", 2);
        SwitchFloors(index, false);
        foreach (var room in RoomAlpha) room.alpha = 0;
        starterRooms[index].alpha = 1;
        Player.transform.position = FloorSpawnPoints[index].position;
        FloorTransitionBlack.alpha = 0;
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
                AudioManager.Instance.SetBGMVolume(PlayerControls.Instance.MonsterDistance - 1f / 5f, 0.5f);
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
                    if (doMonsterSpawn && UIManager.Instance.pendingDialogue.Count==0) monsterApproachCooldown -= Time.deltaTime;
                }
                else
                {
                    PlayerControls.Instance.MonsterDistance--;
                    monsterApproachCooldown = Random.Range(3,10);
                }
            }
            if (PlayerControls.Instance.MonsterDistance == 2)
            {
                if (!informedOnMonster && PlayerPrefs.GetInt("PlayCount", 0) == 0)
                {
                    StartCoroutine(InformAboutMonster());
                    informedOnMonster = true;
                }
            }
            else if (PlayerControls.Instance.MonsterDistance <= 0)
            {
                if (PlayerControls.Instance.MonsterDistance < 0)
                    PlayerControls.Instance.MonsterDistance = 0;

                if (!isMonsterSpawned && UIManager.Instance.pendingDialogue.Count==0)
                {
                    SpawnMonster(true);
                    isMonsterSpawned = true;
                }
            }
        }


        if (Input.GetKeyDown(KeyCode.Escape)) PauseGame();

    }


    //Function
    public void doReturnCrossroad()
    {
        StartCoroutine(ReturnCrossroad());
    }

    IEnumerator ReturnCrossroad()
    {
        //black fade out
        PlayerControls.Instance.StopPlayer();
        PlayerControls.Instance.doPlayerControls = false;
        PlayerControls.Instance.doPlayerAnimations = false;
        FloorTransitionBlack.gameObject.GetComponent<Image>().color = new(0, 0, 0);
        LeanTween.value(FloorTransitionBlack.gameObject, 0, 1, 0.2f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);
        yield return new WaitForSeconds(0.3f);

        //moving player
        Player.transform.position = crossroadReturnPoint.position;

        //black fade in
        LeanTween.value(FloorTransitionBlack.gameObject, 1, 0, 0.5f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);
        yield return new WaitForSeconds(0.2f);

        PlayerControls.Instance.doPlayerControls = true;
        PlayerControls.Instance.doPlayerAnimations = true;
    }

    public void DoBasementScreenEffects(bool activate)
    {
        LeanTween.cancel(Camera.main.gameObject);
        if (activate) AudioManager.Instance.CrossFadeBGM(AudioManager.Instance.m_SHE, 5f);
        else AudioManager.Instance.CrossFadeBGM(currentBGM, 5f);

        // --- Screen Hue ---
        if (activate) ScreenEffectsHue.enabled = true;

        LeanTween.value(Camera.main.gameObject, ScreenEffectsHue.y, activate ? 0.30f : 1f, 5f)
            .setOnUpdate((float val) => ScreenEffectsHue.y = val);

        LeanTween.value(Camera.main.gameObject, ScreenEffectsHue.u, activate ? -3f : 1f, 5f)
            .setOnUpdate((float val) => ScreenEffectsHue.u = val);

        LeanTween.value(Camera.main.gameObject, ScreenEffectsHue.v, activate ? 5f : 1f, 5f)
            .setOnUpdate((float val) => ScreenEffectsHue.v = val)
            .setOnComplete(() =>
            {
                if (!activate) ScreenEffectsHue.enabled = false;
            });

        // --- Screen Hazing ---
        if (activate) ScreenEffectsHazing.enabled = true;

        float hazingStart = ScreenEffectsHazing.intensity;   // always read live value
        float hazingTarget = activate ? -5f : 0f;

        LeanTween.value(Camera.main.gameObject, hazingStart, hazingTarget, 1f)
            .setEaseInOutQuad()
            .setOnUpdate((float val) => ScreenEffectsHazing.intensity = val)
            .setOnComplete(() =>
            {
                if (!activate)
                {
                    ScreenEffectsHazing.enabled = false;
                }
                else
                {
                    LeanTween.value(Camera.main.gameObject, -5f, 5f, 1f)
                        .setEaseInOutQuad()
                        .setLoopPingPong()
                        .setRepeat(-1)
                        .setOnUpdate((float val) => ScreenEffectsHazing.intensity = val);
                }
            });

        // --- Camera Rotation ---
        float rotStart = Camera.main.transform.eulerAngles.z;
        float rotTarget = activate ? -5f : 0f;

        LeanTween.value(Camera.main.gameObject, rotStart, rotTarget, 8f)
            .setEaseInOutQuad()
            .setOnUpdate((float val) =>
            {
                Vector3 euler = Camera.main.transform.eulerAngles;
                euler.z = val;
                Camera.main.transform.eulerAngles = euler;
            })
            .setOnComplete(() =>
            {
                if (activate)
                {
                    LeanTween.value(Camera.main.gameObject, -5f, 5f, 8f)
                        .setEaseInOutQuad()
                        .setLoopPingPong()
                        .setRepeat(-1)
                        .setOnUpdate((float val) =>
                        {
                            Vector3 euler = Camera.main.transform.eulerAngles;
                            euler.z = val;
                            Camera.main.transform.eulerAngles = euler;
                        });
                }
            });

        // --- Camera Background Color ---
        Color startColor = Camera.main.backgroundColor;
        Color endColor = activate ? Color.HSVToRGB(1f, 0.3f, 0.3f) : Color.black;

        LeanTween.value(Camera.main.gameObject, 0f, 1f, 10f)
            .setEaseInOutQuad()
            .setOnUpdate((float t) =>
            {
                Camera.main.backgroundColor = Color.Lerp(startColor, endColor, t);
            })
            .setOnComplete(() =>
            {
                if (activate)
                {
                    LeanTween.value(Camera.main.gameObject, 0f, 1f, 10f)
                        .setEaseInOutQuad()
                        .setLoopClamp()
                        .setRepeat(-1)
                        .setOnUpdate((float val) =>
                        {
                            Camera.main.backgroundColor = Color.HSVToRGB(val, 0.3f, 0.3f);
                        });
                }
            });
    }

    public void DoBasementScreenEffects(bool activate, bool smooth)
    {
        if (smooth)
        {
            DoBasementScreenEffects(activate);
            return;
        }

        LeanTween.cancel(Camera.main.gameObject);

        if (activate) AudioManager.Instance.CrossFadeBGM(AudioManager.Instance.m_SHE, 0f);
        else AudioManager.Instance.CrossFadeBGM(currentBGM, 0f);

        ScreenEffectsHue.enabled = activate;
        ScreenEffectsHue.y = activate ? 0.30f : 1f;
        ScreenEffectsHue.u = activate ? -3f : 1f;
        ScreenEffectsHue.v = activate ? 5f : 1f;

        ScreenEffectsHazing.enabled = activate;
        if (activate)
        {
            LeanTween.value(Camera.main.gameObject, -5f, 5f, 1f)
                .setEaseInOutQuad()
                .setLoopPingPong()
                .setRepeat(-1)
                .setOnUpdate((float val) => ScreenEffectsHazing.intensity = val);
        }
        else
        {
            ScreenEffectsHazing.intensity = 0f;
        }

        Vector3 euler = Camera.main.transform.eulerAngles;
        if (activate)
        {
            LeanTween.value(Camera.main.gameObject, -5f, 5f, 8f)
                .setEaseInOutQuad()
                .setLoopPingPong()
                .setRepeat(-1)
                .setOnUpdate((float val) =>
                {
                    Vector3 e = Camera.main.transform.eulerAngles;
                    e.z = val;
                    Camera.main.transform.eulerAngles = e;
                });
        }
        else
        {
            euler.z = 0f;
            Camera.main.transform.eulerAngles = euler;
        }

        if (activate)
        {
            LeanTween.value(Camera.main.gameObject, 0f, 1f, 10f)
                .setEaseInOutQuad()
                .setLoopClamp()
                .setRepeat(-1)
                .setOnUpdate((float val) =>
                {
                    Camera.main.backgroundColor = Color.HSVToRGB(val, 0.3f, 0.3f);
                });
        }
        else
        {
            Camera.main.backgroundColor = Color.black;
        }
    }

    public void InstaStopBasementEffects()
    {
        DoBasementScreenEffects(false, false);
    }

    public void ShouldMonsterSpawn(bool spawn)
    {
        doMonsterSpawn = spawn;
        if (!spawn)
        {
            PlayerControls.Instance.MonsterDistance = 5;
        }
    }

    public void PauseGame()
    {
        gamePaused = !gamePaused;
        PausePanel.alpha = gamePaused?1:0;
        PausePanel.interactable = gamePaused;
        PausePanel.blocksRaycasts = gamePaused;
        if (UIManager.Instance.pendingDialogue.Count==0)
        {
            PlayerControls.Instance.doPlayerControls = !gamePaused;
            PlayerControls.Instance.doPlayerAnimations = !gamePaused;
        }
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

                        if (floor==0) AudioManager.Instance.FadeStopBGM(0.5f);
                        else
                        {
                            if (!AudioManager.Instance.IsBGMPlaying())
                                AudioManager.Instance.PlayBGM(currentBGM);
                        }

                        if (floor == 2)
                            MonsterDarkness.gameObject.SetActive(false);
                        else
                            MonsterDarkness.gameObject.SetActive(true);
                        LeanTween.value(FloorTransitionBlack.gameObject, 1, 0, 0.5f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);
                        
                    });

        PlayerPrefs.SetInt("savedFloor", floorIndex);
    }

    public void SwitchFloors(int floor, bool doTransition)
    {
        if (doTransition)
        {
            floorIndex = floor;
            LeanTween.value(FloorTransitionBlack.gameObject, 0, 1, 0.5f)
                        .setOnUpdate(val => FloorTransitionBlack.alpha = val).setOnComplete(() =>
                        {
                            foreach (var f in Floors) f.SetActive(false);
                            Floors[floor].SetActive(true);
                            LeanTween.value(FloorTransitionBlack.gameObject, 1, 0, 0.5f)
                        .setOnUpdate(val => FloorTransitionBlack.alpha = val);
                            if (floor == 2)
                                MonsterDarkness.gameObject.SetActive(false);
                            else
                                MonsterDarkness.gameObject.SetActive(true);
                        });
            PlayerPrefs.SetInt("savedFloor", floorIndex);
        }
        else
        {
            floorIndex = floor;
            foreach (var f in Floors) f.SetActive(false);
            Floors[floor].SetActive(true);
            if (floor == 2)
                MonsterDarkness.gameObject.SetActive(false);
            else
                MonsterDarkness.gameObject.SetActive(true);
        }
    }

    public void Jumpscare()
    {
        PlayerControls.Instance.StopPlayer();
        PlayerControls.Instance.doPlayerControls = false;
        PlayerControls.Instance.doPlayerAnimations = false;

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
        LeanTween.delayedCall(Random.Range(1f,2f), () =>
        {
            AudioManager.Instance.StopBGM();
            Monster.transform.position = Vector3.zero;
            Monster.gameObject.SetActive(false);
            JumpscarePanel.SetActive(false);
            GameOver();
            isJumpscared = false;
        });
    } //TODO bug fix player can still move while jumpscare and gameover

    public void JumpscareEnd()
    {
        PlayerControls.Instance.StopPlayer();
        PlayerControls.Instance.doPlayerControls = false;
        PlayerControls.Instance.doPlayerAnimations = false;

        if (isJumpscared) return;
        JumpscarePanel.SetActive(true);
        switch (Random.Range(0, 3))
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
        LeanTween.delayedCall(Random.Range(1f, 2f), () =>
        {
            Application.Quit();
        });
    }

    void GameOver()
    {
        GameOverUI.SetActive(true);
        GameOverUI.GetComponent<MainMenuBehavior>().ShowMainMenu();

        FloorTransitionBlack.alpha=1;
        doMonsterSpawn=false;
        PlayerControls.Instance.doPlayerAnimations = false;
        PlayerControls.Instance.doPlayerControls = false;

        cutsceneMode = true;
        PlayerControls.Instance.StopPlayer();
        UIManager.Instance.ShowPuzzlePanel();
        UIManager.Instance.ForceStopDialogue();
        HideClosetBehavior.Instance.ShowClosetPanel(false);
        PlayerControls.Instance.PuzzleMode(false);
        PlayerControls.Instance.CloseInventory();
    }

    public void RespawnPlayer()
    {
        DoBasementScreenEffects(false, false);
        GameOverUI.SetActive(false);
        cutsceneMode = false;
        doMonsterSpawn = true;
        PlayerControls.Instance.MonsterDistance = 5;
        PlayerControls.Instance.doPlayerAnimations = true;
        PlayerControls.Instance.doPlayerControls = true;
        StartGame();
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


    //Tutorials
    public IEnumerator InformAboutMonster()
    {
        yield return null;
        Debug.Log("INFORMED MONSTER");
        doMonsterSpawn = true;
    }

    public IEnumerator InformAboutControls()
    {
        yield return null;
        Debug.Log("INFORMED CONTROLS");
        PlayerControls.Instance.doPlayerControls = true;
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
        StartCoroutine(InformAboutControls());
    }

    public void PlaySecondFloorEnd()
    {
        PlayerControls.Instance.StaminaPanel.SetActive(false);
        StartCoroutine(SceneSecondFloorEnd());
    }

    IEnumerator SceneSecondFloorEnd()
    {
        cutsceneMode = true;
        PlayerControls.Instance.StopPlayer();

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
            new("From it, I see the figure staring right back at me, Just as confused as I was.", CutsceneImages[1]),
            new("I moved - she followed, she could mimic every single thing I'd try to do.", CutsceneImages[1]),
            new("This is undoubtedly me, whom I've never met before.", CutsceneImages[1])
        };
        UIManager.Instance.LoadDialogue(msg);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);
        msg=null;
        yield return new WaitForSeconds(1);

        //Item check
        if (FoundAllSecretItemsInSecondFloor = SecondFloorSecretItems.All(secret =>
             InventoryManager.Instance.items.Any(slot => slot.PeekItem() == secret)))
        {
            foreach (var slot in InventoryManager.Instance.items)
                if (SecondFloorSecretItems.Contains(slot.PeekItem())) slot.TakeItem();

            foreach (MysteryItemClass item in SecondFloorSecretItems.Cast<MysteryItemClass>())
            { item.isRealized = true; InventoryManager.Instance.TransferItem(item, true); }
        }

        //Branched Dialogue
        if (FoundAllSecretItemsInSecondFloor)
        {
            msg = new Dialogue[]
            {
            new("I noticed the weight my pockets were carrying as I stood there for a while.", null),
            new("Almost forgot I had these items. I wonder why I can feel a connection to them?", CutsceneImages[2]),
            new("The next thing I know...", null),
            new("I wore the hat and posed in front of the mirror carrying a camera.", CutsceneImages[3]),
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
            UIManager.Instance.ScreenText.gameObject.SetActive(true);
            AudioManager.Instance.PlayBGM(AudioManager.Instance.m_2ndFloorEnd);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.s_OfficeDoor);
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
            UIManager.Instance.ScreenText.gameObject.SetActive(false);

            //White fade out
            FloorTransitionBlack.GetComponent<Image>().color = new(1, 1, 1);
            AudioManager.Instance.FadeStopBGM(1);
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
        AudioManager.Instance.PlayBGM(AudioManager.Instance.m_lullaby);
        cutsceneMode = false;
    }

    public void PlayMainExitFound()
    {
        StartCoroutine(SceneMainExitFound());
    }

    IEnumerator SceneMainExitFound()
    {
        //initial setup
        cutsceneMode = true;
        PlayerControls.Instance.doPlayerControls = false;
        PlayerControls.Instance.doPlayerAnimations = false;

        //dialogue
        Dialogue[] msg =
        {
            new("!!", null),
            new("This must be the main exit", null),
        };
        UIManager.Instance.LoadDialogue(msg);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //walks to door
        Player.LeanMove(TransPoints[3].position, 0.3f).setOnComplete(() =>
        {
            PlayerControls.Instance.anim.SetBool("isMoving", false);
        });
        PlayerControls.Instance.anim.SetFloat("x", 0);
        PlayerControls.Instance.anim.SetFloat("y", 1);
        PlayerControls.Instance.anim.SetBool("isMoving", true);
        yield return new WaitForSeconds(0.3f);

        //door sound effect
        AudioManager.Instance.PlaySFX(AudioManager.Instance.s_DoorLocked);
        yield return new WaitForSeconds(0.5f);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.s_DoorLocked);
        yield return new WaitForSeconds(0.5f);

        //dialogue
        msg = new Dialogue[]
        {
            new("Dammit! it's locked", null),
            new("It’s one of those elongated padlocks again.", CutsceneImages[7]),
            new("The keys must be somewhere…", null),
        };
        UIManager.Instance.LoadDialogue(msg);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //exit setup
        cutsceneMode = false;
        PlayerControls.Instance.doPlayerControls = true;
        PlayerControls.Instance.doPlayerAnimations = true;
    }

    public void PlayMonsterEncounter()
    {
        StartCoroutine(SceneMonsterEncounter());
    }

    IEnumerator SceneMonsterEncounter()
    {
        //initial setup
        cutsceneMode = true;
        PlayerControls.Instance.doPlayerControls = false;
        PlayerControls.Instance.doPlayerAnimations = false;
        PlayerControls.Instance.StopPlayer();

        //darken
        PlayerControls.Instance.MonsterDistance = 2;
        yield return new WaitForSeconds(0.5f);

        //dialogue
        Dialogue[] msg =
        {
            new("Wait… why is everything going dark?", null),
            new("Did someone mess with the power here?", null),
            new("Or maybe… I'm not alone in here.", null),
            new("Ughh this gives me the creeps I might need to hide in a closet or something.", null),
        };
        UIManager.Instance.LoadDialogue(msg);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //exit setup
        cutsceneMode = false;
        PlayerControls.Instance.doPlayerControls = true;
        PlayerControls.Instance.doPlayerAnimations = true;
        doMonsterSpawn = true;
    }

    public void EndingCondition()
    {
        if (FoundAllSecretItemsInSecondFloor)
            CutsceneObjects[6].SetActive(true);
        else
            PlayNormalEscape();
    }

    public void FinalJumpscare()
    {
        PlayerControls.Instance.StopPlayer();
        PlayerControls.Instance.doPlayerControls = false;
        PlayerControls.Instance.doPlayerAnimations = false;

        if (isJumpscared) return;
        switch (Random.Range(0, 3))
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
        float time = Random.Range(1f, 2f);
        ScreenEffectsWipe.enabled = true;
        LeanTween.value(ScreenEffectsWipe.gameObject, 0, 40, time)
                    .setOnUpdate(val => ScreenEffectsWipe.shift = val).setOnComplete(() =>
                    {
                        ScreenEffectsWipe.enabled = false;
                        AudioManager.Instance.StopBGM();
                        PlayRealEscape();
                        isJumpscared = false;
                    });
    }

    public void PlayNormalEscape()
    {
        StartCoroutine(SceneNormalEscape());
    }

    IEnumerator SceneNormalEscape()
    {
        //initial setup
        cutsceneMode = true;
        PlayerControls.Instance.doPlayerControls = false;
        PlayerControls.Instance.doPlayerAnimations = false;

        yield return null;

        //fade out game
        LeanTween.value(FloorTransitionBlack.gameObject, 0, 1, 0.5f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);
        AudioManager.Instance.FadeStopBGM(0.5f);
        yield return new WaitForSeconds(0.5f);
        
        //dialogue
        Dialogue[] msg =
        {
            new("Finally, I managed to escape the place.", null),
        };
        UIManager.Instance.LoadDialogue(msg);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //fade in player walking
        LeanTween.value(PlayerWalkingScene.gameObject, 0, 1, 0.5f)
                    .setOnUpdate(val => PlayerWalkingScene.alpha = val);
        yield return new WaitForSeconds(0.5f);

        //play music
        AudioManager.Instance.PlayBGM(AudioManager.Instance.m_1stFloorEnd);

        //dialogue
        msg = new Dialogue[]
            {
                new("And as I'm walking, I think about the things I've seen back there.", null),
                new("The fact that I woke up on the second floor of a mansion.", null),
                new("The fact that I somehow had close ties to it, seeing my own face in the portrait and all.", null),
                new("The fact that something was chasing me, I don't know what that is.", null),
                new("And the fact that I seem to not recognize myself.", null),
                new("Though I may be curious about finding all the answers.", null),
                new("I decided to just... ditched all of them, go back home to rest up from dealing all of that.", null),
                new("But... which is the way to home? Where do I even live?", null),
                new("And from that moment I realized that I kept on walking, without seeing a thing, all in pitch black.", null),
            };
        UIManager.Instance.LoadDialogue(msg);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //fade music and charcter sprite
        LeanTween.value(PlayerWalkingScene.gameObject, 1, 0, 1)
                    .setOnUpdate(val => PlayerWalkingScene.alpha = val);
        AudioManager.Instance.SetBGMVolume(0, 1);

        //dialogue
        msg = new Dialogue[]
            {
                new("Walking endlessly and aimlessly.", null),
                new("It's like I haven't escaped from a delusion.", null),
            };
        UIManager.Instance.LoadDialogue(msg);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //go to menu
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayRealEscape()
    {
        StartCoroutine(SceneRealEscape());
    }

    IEnumerator SceneRealEscape()
    {
        //initial setup
        cutsceneMode = true;
        PlayerControls.Instance.doPlayerControls = false;
        PlayerControls.Instance.doPlayerAnimations = false;
        yield return null;

        //fade out game
        FloorTransitionBlack.alpha = 1;
        AudioManager.Instance.StopBGM();
        yield return new WaitForSeconds(1f);

        //dialogue
        Dialogue[] msg =
        {
            new("I woke up.", null),
        };
        UIManager.Instance.LoadDialogue(msg);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //play music
        AudioManager.Instance.PlayBGM(AudioManager.Instance.m_BasementEnd);

        //dialogue
        msg = new Dialogue[]
            {
                new("...", CutsceneImages[4]),
                new("...ohh", null),
                new("...", CutsceneImages[5]),
                new("I see now... I almost forgot.", null),
                new("...no wonder I forgot all about it.", CutsceneImages[6]),
                new("about who I am and all.", CutsceneImages[6]),
                new("It's the last thing I'd want to remember, honestly.", null),
            };
        UIManager.Instance.LoadDialogue(msg);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //fade music and charcter sprite
        AudioManager.Instance.SetBGMVolume(0, 2);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("MainMenu");
    }

    public void Play1stFloorFinalPainting()
    {
        StartCoroutine(Scene1stFloorFinalPainting());
    }

    IEnumerator Scene1stFloorFinalPainting()
    {

        //initial setup
        cutsceneMode = true;
        PlayerControls.Instance.doPlayerControls = false;
        PlayerControls.Instance.doPlayerAnimations = false;

        Dialogue[] msg =
            {
                new("Something came down the table.", null),
                new("Huh, it's a painting of a telephon-", CutsceneImages[8], true), //putting cutscene images does not proceed to next convo
            };
        UIManager.Instance.LoadDialogue(msg, UIManager.Instance.dialogueText);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);
        Debug.Log("after");

        //sudden flash back dialogue
        UIManager.Instance.ScreenText.text = "";
        FloorTransitionBlack.GetComponent<Image>().color = new(1, 1, 1);
        FloorTransitionBlack.alpha = 1;
        UIManager.Instance.ScreenText.gameObject.SetActive(true);
        msg = new Dialogue[]
            {
            new("\"Melania\"", CutsceneImages[8]),
            };
        UIManager.Instance.LoadDialogue(msg, UIManager.Instance.ScreenText);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //dialogue
        UIManager.Instance.dialogueText.text = "";
        FloorTransitionBlack.alpha = 0;
        msg = new Dialogue[]
            {
            new("!?", CutsceneImages[8]),
            };
        UIManager.Instance.LoadDialogue(msg, UIManager.Instance.dialogueText);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);






        yield break;

        //initial setup
        cutsceneMode = true;
        PlayerControls.Instance.doPlayerControls = false;
        PlayerControls.Instance.doPlayerAnimations = false;
        /*
        Dialogue[] msg =
            {
                new("Something came down the table.", null),
                new("Huh, it's a painting of a telephon-", CutsceneImages[8], true), //putting cutscene images does not proceed to next convo
            };
        UIManager.Instance.LoadDialogue(msg, UIManager.Instance.dialogueText);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);
        Debug.Log("after");
        */
        //sudden flash back dialogue
        UIManager.Instance.ScreenText.text = "";
        FloorTransitionBlack.GetComponent<Image>().color = new(1, 1, 1);
        FloorTransitionBlack.alpha = 1;
        UIManager.Instance.ScreenText.gameObject.SetActive(true);
        msg = new Dialogue[]
            {
            new("\"Melania\"", CutsceneImages[8]),
            };
        UIManager.Instance.LoadDialogue(msg, UIManager.Instance.ScreenText);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //dialogue
        UIManager.Instance.dialogueText.text = "";
        FloorTransitionBlack.alpha = 0;
        msg = new Dialogue[]
            {
            new("!?", CutsceneImages[8]),
            };
        UIManager.Instance.LoadDialogue(msg, UIManager.Instance.dialogueText);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //sudden flash back dialogue
        UIManager.Instance.ScreenText.text = "";
        FloorTransitionBlack.alpha = 1;
        msg = new Dialogue[]
            {
            new("\"I want you to take over the mansion for me\"", CutsceneImages[8]),
            new("That's what he said on the telephone.", CutsceneImages[8]),
            };
        UIManager.Instance.LoadDialogue(msg, UIManager.Instance.ScreenText);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //dialogue
        UIManager.Instance.dialogueText.text = "";
        FloorTransitionBlack.alpha = 0;
        msg = new Dialogue[]
            {
            new("Huh...? What was-", CutsceneImages[8], true),
            };
        UIManager.Instance.LoadDialogue(msg, UIManager.Instance.dialogueText);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //sudden flash back dialogue
        UIManager.Instance.ScreenText.text = "";
        FloorTransitionBlack.alpha = 1;
        msg = new Dialogue[]
            {
            new("I don't think you need to.", CutsceneImages[8]),
            new("Father, I'm already doing well with my job.", CutsceneImages[8]),
            };
        UIManager.Instance.LoadDialogue(msg, UIManager.Instance.ScreenText);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //dialogue
        UIManager.Instance.dialogueText.text = "";
        FloorTransitionBlack.alpha = 0;
        msg = new Dialogue[]
            {
            new("...", CutsceneImages[8], true),
            new("It's gonna take a long while for me to process what I just remembered.", null),
            };
        UIManager.Instance.LoadDialogue(msg, UIManager.Instance.dialogueText);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //end setup
        cutsceneMode = false;
        PlayerControls.Instance.doPlayerControls = true;
        PlayerControls.Instance.doPlayerAnimations = true;
        FloorTransitionBlack.GetComponent<Image>().color = new(0, 0, 0);
        UIManager.Instance.ScreenText.gameObject.SetActive(false);
    } // CRITICAL BUG

    public void PlaySpecialRoom()
    {
        StartCoroutine(SceneSpecialRoom());
    }

    IEnumerator SceneSpecialRoom()
    {
        //initial setup
        cutsceneMode = true;
        PlayerControls.Instance.doPlayerControls = false;
        PlayerControls.Instance.doPlayerAnimations = false;
        PlayerControls.Instance.StopPlayer();
        AudioManager.Instance.FadeStopBGM(0.5f);

        //black fade out
        FloorTransitionBlack.gameObject.GetComponent<Image>().color = new(0, 0, 0);
        LeanTween.value(FloorTransitionBlack.gameObject, 0, 1, 0.5f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);
        yield return new WaitForSeconds(1f);

        //moving player
        Player.transform.position = TransPoints[5].position;

        //black fade in
        FloorTransitionBlack.gameObject.GetComponent<Image>().color = new(0, 0, 0);
        LeanTween.value(FloorTransitionBlack.gameObject, 1, 0, 0.5f)
                    .setOnUpdate(val => FloorTransitionBlack.alpha = val);
        yield return new WaitForSeconds(0.5f);

        //dialogue
        Dialogue[] msg =
            {
            new("What a messy room.", null),
            new("What's even the cause of all this?", null),
            };
        UIManager.Instance.LoadDialogue(msg, UIManager.Instance.dialogueText);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //player move
        Player.LeanMove(TransPoints[6].position, 2f).setOnComplete(() =>
        {
            PlayerControls.Instance.anim.SetBool("isMoving", false);
        });
        PlayerControls.Instance.anim.SetFloat("x", 0);
        PlayerControls.Instance.anim.SetFloat("y", 1);
        PlayerControls.Instance.anim.SetBool("isMoving", true);
        yield return new WaitForSeconds(3f);

        //dialogue
        msg = new Dialogue[]
            {
            new("...is that me?", null),
            new("How did I end up lying there?", null),
            };
        UIManager.Instance.LoadDialogue(msg, UIManager.Instance.dialogueText);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //cutscene pics sequence
        CutsceneObjects[2].SetActive(true);
        yield return new WaitForSeconds(0.2f);
        CutsceneObjects[2].SetActive(false);
        CutsceneObjects[3].SetActive(true);
        yield return new WaitForSeconds(0.2f);
        CutsceneObjects[3].SetActive(false);
        CutsceneObjects[4].SetActive(true);
        yield return new WaitForSeconds(0.2f);
        CutsceneObjects[4].SetActive(false);
        CutsceneObjects[0].SetActive(false);
        CutsceneObjects[1].SetActive(true);
        yield return new WaitForSeconds(0.5f);

        //dialogue
        msg = new Dialogue[]
            {
            new("no...", null),
            new("Ow, my head hurts.", null),
            };
        UIManager.Instance.LoadDialogue(msg, UIManager.Instance.dialogueText);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);
        
        //insanity filter
        DoBasementScreenEffects(true);

        //player move
        Player.LeanMove((TransPoints[5].position+TransPoints[6].position)/2, 2f).setOnComplete(() =>
        {
            PlayerControls.Instance.anim.SetBool("isMoving", false);
        });
        PlayerControls.Instance.anim.SetFloat("x", 0);
        PlayerControls.Instance.anim.SetFloat("y", 1);
        PlayerControls.Instance.anim.SetBool("isMoving", true);
        yield return new WaitForSeconds(2f);

        //dialogue
        msg = new Dialogue[]
            {
            new("Oh no, I'm going insane again.", null),
            };
        UIManager.Instance.LoadDialogue(msg, UIManager.Instance.dialogueText);
        yield return new WaitUntil(() => UIManager.Instance.pendingDialogue.Count == 0);

        //move cam to bed
        CutsceneObjects[5].SetActive(true);
        Camera.main.gameObject.LeanMove((Vector2)CutsceneObjects[5].transform.position, 1f);
        yield return new WaitForSeconds(2);

        //move it back to player
        Camera.main.gameObject.LeanMove((Vector2)Player.transform.position, 0.5f);
        yield return new WaitForSeconds(0.5f);

        //end setup
        CutsceneObjects[5].GetComponent<NavMeshAgent>().enabled = true;
        Destroy(CutsceneObjects[5], 2f);
        cutsceneMode = false;
        PlayerControls.Instance.doPlayerControls = true;
        PlayerControls.Instance.doPlayerAnimations = true;
        FloorTransitionBlack.GetComponent<Image>().color = new(0, 0, 0);
        UIManager.Instance.ScreenText.gameObject.SetActive(false);
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("PlayCount");
        PlayerPrefs.DeleteKey("savedFloor");
    }
}
