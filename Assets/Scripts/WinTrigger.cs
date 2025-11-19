using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WinTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the PlayableDirectorSequencer")]
    public PlayableDirectorSequencer sequencer;
    
    [Header("Settings")]
    [Tooltip("Tag to check for trigger (leave empty to trigger on any collision)")]
    public string requiredTag = "Player";

    private bool hasTriggered = false;

    private void Awake()
    {
        if (sequencer == null)
        {
            Debug.LogError("PlayableDirectorSequencer reference is not assigned on WinTrigger!");
        }
        
        // Ensure the collider is set to trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("WinTrigger collider is not set as trigger. Setting it now.");
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Prevent multiple triggers
        if (hasTriggered) return;
        
        // Check if tag matches (if specified)
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
        {
            return;
        }
        
        Debug.Log($"Win trigger activated by {other.gameObject.name}!");
        
        if (sequencer != null)
        {
            hasTriggered = true;
            sequencer.PlayWinSequence();
        }
    }

    // Optional: Reset the trigger (useful for testing)
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
