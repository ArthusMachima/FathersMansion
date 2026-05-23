using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class FakeMonsterBehavior : MonoBehaviour
{
    public NavMeshAgent agent;
    [SerializeField] Transform target;
    [SerializeField] UnityEvent onJumpscare;
    [SerializeField] bool doJumpscare=true;


    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) return;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    

    private void Update()
    {
        if (agent == null) return; 
        agent.isStopped = GameManager.Instance.gamePaused || 
            PlayerControls.Instance.isPlayerHiddenInCloset || 
            UIManager.Instance.pendingDialogue.Count>0; 
        if (target != null) agent.SetDestination(target.position);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FakeJumpscare();
        }
    }

    public void FakeJumpscare()
    {
        if (doJumpscare)
        {
            PlayerControls.Instance.StopPlayer();
            PlayerControls.Instance.doPlayerControls = false;
            PlayerControls.Instance.doPlayerAnimations = false;

            if (GameManager.Instance.isJumpscared) return;
            GameManager.Instance.JumpscarePanel.SetActive(true);
            switch (Random.Range(0, 3))
            {
                case 0:
                    {
                        AudioManager.Instance.PlayBGM(AudioManager.Instance.s_jumpscare1);
                        break;
                    }
                case 1:
                    {
                        AudioManager.Instance.PlayBGM(AudioManager.Instance.s_jumpscare2);
                        break;
                    }
                case 2:
                    {
                        AudioManager.Instance.PlayBGM(AudioManager.Instance.s_jumpscare3);
                        break;
                    }
            }
            GameManager.Instance.isJumpscared = true;
            LeanTween.delayedCall(Random.Range(1f, 2f), () =>
            {
                AudioManager.Instance.StopBGM();
                GameManager.Instance.JumpscarePanel.SetActive(false);
                GameManager.Instance.isJumpscared = false;
                PlayerControls.Instance.doPlayerControls = true;
                PlayerControls.Instance.doPlayerAnimations = true;
                onJumpscare?.Invoke();
                gameObject.SetActive(false);
            });
        }

        if (doJumpscare) return;
        onJumpscare?.Invoke();
        gameObject.SetActive(false);
    }
}