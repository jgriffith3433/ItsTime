using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Plays a Timeline when player enters trigger, pauses on exit, and resumes on re-entry.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TimelineTrigger : MonoBehaviour
{
    [Header("Timeline Reference")]
    [SerializeField] private PlayableDirector playableDirector;
    
    [Header("Settings")]
    [Tooltip("Tag to check for player")]
    [SerializeField] private string playerTag = "Player";
    
    [Tooltip("Auto-find PlayableDirector on this GameObject if not assigned")]
    [SerializeField] private bool autoFindDirector = true;
    
    [Tooltip("Allow timeline to resume from paused position when re-entering trigger")]
    [SerializeField] private bool canResume = true;
    
    [Tooltip("Allow timeline to play again after being stopped/paused")]
    [SerializeField] private bool canPlayMoreThanOnce = true;
    
    [Tooltip("Pause timeline when player exits trigger")]
    [SerializeField] private bool pauseOnExit = true;
    
    private bool wasPlaying = false;
    
    private void OnValidate()
    {
        // Ensure the collider is set to trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[TimelineTrigger] Collider on '{gameObject.name}' was not set as trigger. Fixed automatically.");
        }
        
        // Auto-find PlayableDirector if enabled and not assigned
        if (autoFindDirector && playableDirector == null)
        {
            playableDirector = GetComponent<PlayableDirector>();
            if (playableDirector == null)
            {
                playableDirector = GetComponentInChildren<PlayableDirector>();
            }
        }
    }
    
    private void Awake()
    {
        // Try to find PlayableDirector if not assigned
        if (playableDirector == null && autoFindDirector)
        {
            playableDirector = GetComponent<PlayableDirector>();
            if (playableDirector == null)
            {
                playableDirector = GetComponentInChildren<PlayableDirector>();
            }
        }
        
        if (playableDirector == null)
        {
            Debug.LogError($"[TimelineTrigger] No PlayableDirector assigned to '{gameObject.name}'!");
        }
    }

    // handle external scripts disabling the trigger
    private void OnDisable()
    {
        if (playableDirector == null) return;
        
        // Pause timeline if trigger is disabled while playing
        if (playableDirector.state == PlayState.Playing)
        {
            playableDirector.Pause();
            wasPlaying = true;
            Debug.Log($"[TimelineTrigger] Trigger on '{gameObject.name}' disabled - Pausing Timeline");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (playableDirector == null) return;
            
            // Check if timeline was previously playing and can't play more than once
            if (wasPlaying && !canPlayMoreThanOnce)
            {
                Debug.Log($"[TimelineTrigger] Timeline on '{gameObject.name}' was already played and canPlayMoreThanOnce is false");
                return;
            }
            
            // Check if timeline was paused and hasn't ended
            if (canResume && wasPlaying && playableDirector.time < playableDirector.duration)
            {
                // Resume timeline
                playableDirector.Play();
                Debug.Log($"[TimelineTrigger] Player entered '{gameObject.name}' - Resuming Timeline");
            }
            else if (playableDirector.state != PlayState.Playing)
            {
                // Start timeline from beginning
                playableDirector.Play();
                wasPlaying = true;
                Debug.Log($"[TimelineTrigger] Player entered '{gameObject.name}' - Starting Timeline");
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (playableDirector == null) return;
            
            // Only pause if pauseOnExit is enabled and timeline is currently playing
            if (pauseOnExit && playableDirector.state == PlayState.Playing)
            {
                playableDirector.Pause();
                wasPlaying = true;
                Debug.Log($"[TimelineTrigger] Player exited '{gameObject.name}' - Pausing Timeline");
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw the trigger volume
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            bool isPlaying = playableDirector != null && playableDirector.state == PlayState.Playing;
            Gizmos.color = isPlaying ? new Color(0, 1, 0, 0.3f) : new Color(1, 0.5f, 0, 0.3f);
            
            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius * transform.lossyScale.x);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw line to PlayableDirector if assigned
        if (playableDirector != null && playableDirector.gameObject != gameObject)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, playableDirector.transform.position);
        }
    }
}
