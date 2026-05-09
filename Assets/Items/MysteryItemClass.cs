using UnityEngine;


[CreateAssetMenu(fileName = "New Mystery Item", menuName = "Items/Mystery Item")]
public class MysteryItemClass : ItemClass
{
    [Header("Mystery Item Class")]
    public bool isRealized;
    public string realName;
    public string realDescription;
}
