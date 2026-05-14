using UnityEngine;

public class ObjectEnabler : MonoBehaviour
{
    [SerializeField] GameObject obj;
    [SerializeField] GameObject objToDisable;


    public void EnableObject()
    {
        obj.SetActive(true);
        if (objToDisable != null ) objToDisable.SetActive(false);
    }
}
