using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject Fog;

    void Start()
    {
        Fog.SetActive(true);
    }
}
