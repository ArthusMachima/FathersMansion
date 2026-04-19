using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject Fog;

    void Start()
    {
        Fog.SetActive(true);
        AudioManager.Instance.PlayBGM(AudioManager.Instance.m_lullaby);
    }



    public void StarterMessage()
    {

        SideScreenMessage.Instance.DisplayMessage("Objective", "Find a way out", 1.5f);
    }


    public void MatchingMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Find a polaroid card and drag it here", 1.5f);
    }








    //Debug
    public void TestSideMessage()
    {
        SideScreenMessage.Instance.DisplayMessage("Objective", "Talk to blue guy", 1.5f);
    }

    public void SampleVoid()
    {
        Debug.Log("WORKING!!!!");
    }

    public void SampleVoidYellow()
    {
        Debug.LogWarning("WORKING!!!!");
    }

    public void SampleVoiRed()
    {
        Debug.LogError("WORKING!!!!");
    }
}
