using System;
using UnityEngine;
using UnityEngine.Events;

public class DialogueObject : MonoBehaviour, IInteractable
{
    [SerializeField] Dialogue[] dialogue;
    [SerializeField] bool isInteractable = true;
    [SerializeField] bool isRepeatable;
    [SerializeField] bool onlyShownOnce;
    [SerializeField] bool isCutscene;
    bool alreadyShown;
    SpriteRenderer render;

    private void Start()
    {
        render = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (render != null && isCutscene) render.color = new(0, 0, 0, 0);
    }

    public void Interact()
    {
        if (isInteractable) LoadDialogue();
    }

    public void LoadDialogue()
    {
        if (alreadyShown) return;
        UIManager.Instance.LoadDialogue(dialogue);
        if (onlyShownOnce) alreadyShown = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCutscene)
        {
            if (!isInteractable) return;
            UIManager.Instance.LoadDialogue(dialogue);
            if (!isRepeatable) Destroy(gameObject, 0.1f);
        }
    }
}

[Serializable]
public class Dialogue
{
    public Dialogue(string msg, Sprite img)
    {
        sentence = msg;
        cutsceneImage = img;
    }

    public string sentence;
    public Sprite cutsceneImage;
    public AudioClip sound;
    public UnityEvent methodCall;
    public bool disableTextSpecificDelays;
}
