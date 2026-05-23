using System.Runtime.Serialization;
using UnityEngine;

public class MapInfluencer : MonoBehaviour
{
    [SerializeField] GameObject[] BranchA;
    [SerializeField] GameObject[] BranchB;

    public void SetBranch()
    {
        foreach (GameObject obj in BranchA) obj.SetActive(GameManager.Instance.FoundAllSecretItemsInSecondFloor);
        foreach (GameObject obj in BranchB) obj.SetActive(!GameManager.Instance.FoundAllSecretItemsInSecondFloor);
    }
}
