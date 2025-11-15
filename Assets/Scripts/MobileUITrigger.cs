using UnityEngine;
using MobileControllerFramework;

/// <summary>
/// Controls MobileControllerFramework Button UI state when player enters/exits trigger.
/// Enables buttons on enter and disables on exit (or inverts with hideInstead option).
/// </summary>
[RequireComponent(typeof(Collider))]
public class MobileUITrigger : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button[] buttons;
    
    [Header("Settings")]
    [Tooltip("Tag to check for player")]
    [SerializeField] private string playerTag = "Player";
    
    [Tooltip("When true, hides buttons on enter and shows on exit (opposite behavior)")]
    [SerializeField] private bool hideInstead = false;
    
    private void OnValidate()
    {
        // Ensure the collider is set to trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[MobileUITrigger] Collider on '{gameObject.name}' was not set as trigger. Fixed automatically.");
        }
    }
    
    private void Awake()
    {
        if (buttons == null || buttons.Length == 0)
        {
            Debug.LogWarning($"[MobileUITrigger] No buttons assigned to '{gameObject.name}'!");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (buttons == null || buttons.Length == 0) return;
            
            // Enable or disable based on hideInstead setting
            if (hideInstead)
            {
                // Hide buttons on enter
                foreach (var button in buttons)
                {
                    if (button != null)
                    {
                        button.gameObject.SetActive(false);
                    }
                }
                Debug.Log($"[MobileUITrigger] Player entered '{gameObject.name}' - Hiding buttons");
            }
            else
            {
                // Show buttons on enter
                foreach (var button in buttons)
                {
                    if (button != null)
                    {
                        button.gameObject.SetActive(true);
                    }
                }
                Debug.Log($"[MobileUITrigger] Player entered '{gameObject.name}' - Showing buttons");
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (buttons == null || buttons.Length == 0) return;
            
            // Disable or enable based on hideInstead setting (opposite of enter)
            if (hideInstead)
            {
                // Show buttons on exit
                foreach (var button in buttons)
                {
                    if (button != null)
                    {
                        button.gameObject.SetActive(true);
                    }
                }
                Debug.Log($"[MobileUITrigger] Player exited '{gameObject.name}' - Showing buttons");
            }
            else
            {
                // Hide buttons on exit
                foreach (var button in buttons)
                {
                    if (button != null)
                    {
                        button.gameObject.SetActive(false);
                    }
                }
                Debug.Log($"[MobileUITrigger] Player exited '{gameObject.name}' - Hiding buttons");
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw the trigger volume
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            // Color based on hideInstead setting
            Gizmos.color = hideInstead ? new Color(1, 0, 0, 0.3f) : new Color(0, 1, 0, 0.3f);
            
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
        // Draw lines to assigned buttons
        if (buttons != null)
        {
            Gizmos.color = hideInstead ? Color.red : Color.green;
            foreach (var button in buttons)
            {
                if (button != null && button.gameObject != gameObject)
                {
                    Gizmos.DrawLine(transform.position, button.transform.position);
                }
            }
        }
    }
}
