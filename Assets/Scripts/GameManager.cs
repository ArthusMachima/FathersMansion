using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject Fog;

    void Start()
    {
        Fog.SetActive(true);
    }



    public void StarterMessage()
    {

        SideScreenMessage.Instance.DisplayMessage("Objective", "Find a way out", 1.5f);
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
