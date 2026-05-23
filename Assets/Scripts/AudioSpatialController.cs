using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpatialAudioController : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Assign the player transform here. If left empty, it will search for the 'Player' tag.")]
    public Transform playerTransform;

    [Header("Audio Bounds")]
    [Tooltip("The maximum distance at which the player can hear this sound.")]
    public float maxHearingDistance = 20f;

    [Tooltip("The distance at which the sound starts to fade out. Must be less than Max Hearing Distance.")]
    public float fadeStartDistance = 5f;

    [Header("Volume Constraints")]
    [Range(0f, 1f)] public float maxVolume = 1f;
    [Range(0f, 1f)] public float minVolume = 0f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Auto-configure AudioSource for custom 2D/3D scripting
        audioSource.spatialBlend = 0f; // Keep at 2D to manually control panStereo
        audioSource.loop = true;

        // Fallback search for player
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    void Update()
    {
        if (playerTransform == null || audioSource == null) return;

        UpdateVolume();
        UpdatePanning();
    }

    private void UpdateVolume()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance >= maxHearingDistance)
        {
            audioSource.volume = minVolume;
            return;
        }

        if (distance <= fadeStartDistance)
        {
            audioSource.volume = maxVolume;
            return;
        }

        // Smoothly interpolate volume between the fade start and max hearing distance
        float t = (distance - fadeStartDistance) / (maxHearingDistance - fadeStartDistance);
        audioSource.volume = Mathf.Lerp(maxVolume, minVolume, t);
    }

    private void UpdatePanning()
    {
        // Vector pointing from player to this audio source
        Vector3 directionToSource = transform.position - playerTransform.position;

        // Normalize based on your game plane (assumes X/Z movement. Switch to X/Y for pure 2D side-scrollers)
        directionToSource.Normalize();

        // Calculate dot product relative to the player's right side
        // Returns 1 if perfectly to the right, -1 if perfectly to the left
        float panValue = Vector3.Dot(playerTransform.right, directionToSource);

        // Clamp values to prevent audio glitches
        audioSource.panStereo = Mathf.Clamp(panValue, -1f, 1f);
    }

    // Visualizes the audio ranges inside the Unity Editor Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxHearingDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fadeStartDistance);
    }
}