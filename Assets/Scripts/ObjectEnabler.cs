using UnityEngine;

public class ObjectEnabler : MonoBehaviour
{
    [SerializeField] GameObject obj;


    public void EnableObject()
    {
        obj.SetActive(true);
    }
}
