using UnityEngine;
using UnityEngine.Playables;
using MobileControllerFramework;
using Unity.Cinemachine;

/// <summary>
/// Triggers a cutscene when player enters. Disables FPS controls and Cinemachine follow,
/// plays a timeline, then re-enables everything when the timeline finishes.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CutsceneTrigger : MonoBehaviour
{
    [Header("Timeline Reference")]
    [SerializeField] private PlayableDirector playableDirector;
    
    [Header("FPS Controller")]
    [SerializeField] private FPSController fpsController;
    [Tooltip("Analog control UI to hide during cutscene")]
    [SerializeField] private Analog analogControl;
    
    [Header("Cinemachine Camera")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [Tooltip("The CinemachineFollow component to disable during cutscene")]
    [SerializeField] private CinemachineFollow cinemachineFollow;
    
    [Header("Settings")]
    [Tooltip("Tag to check for player")]
    [SerializeField] private string playerTag = "Player";
    
    [Tooltip("Auto-find PlayableDirector on this GameObject if not assigned")]
    [SerializeField] private bool autoFindDirector = true;
    
    [Tooltip("Auto-find FPSController on player when triggered")]
    [SerializeField] private bool autoFindFPSController = true;
    
    [Tooltip("Auto-find CinemachineCamera in scene if not assigned")]
    [SerializeField] private bool autoFindCinemachine = true;
    
    [Tooltip("Allow cutscene to play more than once")]
    [SerializeField] private bool canPlayMoreThanOnce = false;
    
    private bool hasPlayed = false;
    private bool controlsWereEnabled = false;
    private bool followWasEnabled = false;
    private bool analogWasActive = false;
    
    private void OnValidate()
    {
        // Ensure the collider is set to trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[CutsceneTrigger] Collider on '{gameObject.name}' was not set as trigger. Fixed automatically.");
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
        
        // Auto-find CinemachineCamera if enabled and not assigned
        if (autoFindCinemachine && cinemachineCamera == null)
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            if (cinemachineCamera != null && cinemachineFollow == null)
            {
                cinemachineFollow = cinemachineCamera.GetComponent<CinemachineFollow>();
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
            Debug.LogError($"[CutsceneTrigger] No PlayableDirector assigned to '{gameObject.name}'!");
        }
        
        // Try to find CinemachineCamera if not assigned
        if (autoFindCinemachine && cinemachineCamera == null)
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            if (cinemachineCamera != null && cinemachineFollow == null)
            {
                cinemachineFollow = cinemachineCamera.GetComponent<CinemachineFollow>();
            }
        }
        
        // Subscribe to timeline stopped event
        if (playableDirector != null)
        {
            playableDirector.stopped += OnTimelineStopped;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from timeline stopped event
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnTimelineStopped;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // Check if we can play
            if (hasPlayed && !canPlayMoreThanOnce)
            {
                Debug.Log($"[CutsceneTrigger] Cutscene on '{gameObject.name}' was already played and canPlayMoreThanOnce is false");
                return;
            }
            
            if (playableDirector == null)
            {
                Debug.LogError($"[CutsceneTrigger] No PlayableDirector assigned to '{gameObject.name}'!");
                return;
            }
            
            // Auto-find FPSController if needed
            if (autoFindFPSController && fpsController == null)
            {
                fpsController = other.GetComponent<FPSController>();
            }
            
            // Start cutscene
            StartCutscene();
        }
    }
    
    private void StartCutscene()
    {
        Debug.Log($"[CutsceneTrigger] Starting cutscene on '{gameObject.name}'");
        
        // 1. Disable FPS controls
        if (fpsController != null)
        {
            controlsWereEnabled = fpsController.controlsEnabled;
            fpsController.controlsEnabled = false;
            Debug.Log($"[CutsceneTrigger] Disabled FPS controls (controlsEnabled = false)");
            
            // Hide analog UI
            if (analogControl == null && fpsController.analog != null)
            {
                analogControl = fpsController.analog;
            }
            
            if (analogControl != null)
            {
                // Reset analog stick position and state before hiding
                if (analogControl.StickTarget != null)
                {
                    analogControl.StickTarget.anchoredPosition = analogControl.StickTarget.anchoredPosition; // Force position update
                }
                analogWasActive = analogControl.gameObject.activeSelf;
                analogControl.gameObject.SetActive(false);
                Debug.Log($"[CutsceneTrigger] Hidden Analog UI");
            }
        }
        
        // 2. Disable Cinemachine follow behavior
        if (cinemachineFollow != null)
        {
            followWasEnabled = cinemachineFollow.enabled;
            cinemachineFollow.enabled = false;
            Debug.Log($"[CutsceneTrigger] Disabled CinemachineFollow component");
        }
        
        // 3. Play the timeline
        playableDirector.Play();
        hasPlayed = true;
    }
    
    private void OnTimelineStopped(PlayableDirector director)
    {
        // Only respond to our timeline
        if (director != playableDirector) return;
        
        Debug.Log($"[CutsceneTrigger] Timeline finished on '{gameObject.name}' - Re-enabling controls");
        
        // 4. Re-enable FPS controls
        if (fpsController != null)
        {
            fpsController.controlsEnabled = controlsWereEnabled;
            Debug.Log($"[CutsceneTrigger] Re-enabled FPS controls (controlsEnabled = {controlsWereEnabled})");
            
            // Show analog UI
            if (analogControl != null)
            {
                analogControl.gameObject.SetActive(analogWasActive);
                Debug.Log($"[CutsceneTrigger] Restored Analog UI visibility");
            }
        }
        
        // 5. Re-enable Cinemachine follow behavior
        if (cinemachineFollow != null)
        {
            cinemachineFollow.enabled = followWasEnabled;
            Debug.Log($"[CutsceneTrigger] Re-enabled CinemachineFollow component");
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw the trigger volume
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            bool isPlaying = playableDirector != null && playableDirector.state == PlayState.Playing;
            Gizmos.color = isPlaying ? new Color(1, 0, 1, 0.3f) : new Color(0, 0.5f, 1, 0.3f);
            
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
        // Draw lines to referenced components
        Gizmos.color = Color.cyan;
        
        if (playableDirector != null && playableDirector.gameObject != gameObject)
        {
            Gizmos.DrawLine(transform.position, playableDirector.transform.position);
        }
        
        if (fpsController != null)
        {
            Gizmos.DrawLine(transform.position, fpsController.transform.position);
        }
        
        if (cinemachineCamera != null)
        {
            Gizmos.DrawLine(transform.position, cinemachineCamera.transform.position);
        }
    }
}
