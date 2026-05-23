using UnityEngine;

public class ObjectEnabler : MonoBehaviour
{
    [SerializeField] GameObject obj;
    [SerializeField] GameObject objToDisable;
    [SerializeField] GameObject[] groupToEnable;
    [SerializeField] GameObject[] groupToDisable;
    [SerializeField] bool makeItTransparent;
    SpriteRenderer render;

    private void Start()
    {
        render = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (render != null && makeItTransparent) render.color = new(0, 0, 0, 0);
    }

    public void EnableObject()
    {
        obj.SetActive(true);
        if (objToDisable != null ) objToDisable.SetActive(false);
        if (groupToEnable.Length>0) foreach (GameObject group in groupToEnable) group.SetActive(true);
        if(groupToDisable.Length>0) foreach (GameObject group in groupToDisable) group.SetActive(false);
    }
}
