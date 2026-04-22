using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject Fog;
    [SerializeField] GameObject JumpscarePanel;
    [SerializeField] GameObject[] Floors;
    [SerializeField] CanvasGroup BlackBG;

    void Start()
    {
        if (Fog!=null) Fog.SetActive(true);
        AudioManager.Instance.PlayBGM(AudioManager.Instance.m_lullaby);
    }



    public static GameManager Instance;
    private void OnEnable()
    {
        Instance = this;
    }



    public void StarterMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Find a way out", 1.5f);
    }



    public void MatchingMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Find a polaroid card and drag it here", 1.5f);
    }




    public void SwitchFloors(int floor)
    {
        PlayerControls.Instance.MonsterDistance = 0; //Temporary

        LeanTween.value(BlackBG.gameObject, 0, 1, 0.5f)
                    .setOnUpdate(val => BlackBG.alpha = val).setOnComplete(() =>
                    {
                        foreach (var f in Floors) f.SetActive(false);
                        Floors[floor].SetActive(true);
                        LeanTween.value(BlackBG.gameObject, 1, 0, 0.5f)
                    .setOnUpdate(val => BlackBG.alpha = val);
                    });
    }

    public void Jumpscare()
    {
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
        LeanTween.delayedCall(2, () =>
        {
            SceneManager.LoadScene("GameOver");
        });
    }



    //Debug
    public void TestSideMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Talk to blue guy", 1.5f);
    }
}
