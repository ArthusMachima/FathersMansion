using System;
using UnityEngine;

public class DialogueObject : MonoBehaviour, IInteractable
{
    [SerializeField] Dialogue[] dialogue;
    [SerializeField] bool isCutscene;
    [SerializeField] bool isRepeatable;
    SpriteRenderer sprite;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null && isCutscene)
        {
            sprite.color = new(0,0,0,0);
        }
    }

    public void Interact()
    {
        UIManager.Instance.LoadDialogue(dialogue);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCutscene)
        {
            UIManager.Instance.LoadDialogue(dialogue);
            if (!isRepeatable) Destroy(gameObject, 0.1f);
        }
    }
}

[Serializable]
public class Dialogue
{
    public string sentence;
    public Sprite cutsceneImage;
}
