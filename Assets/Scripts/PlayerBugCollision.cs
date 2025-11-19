using UnityEngine;
using MobileControllerFramework;

[RequireComponent(typeof(Collider))]
public class PlayerBugCollision : MonoBehaviour
{
    private FPSController fpsController;
    public PlayableDirectorSequencer directorSequencer;

    private void Awake()
    {
        fpsController = GetComponent<FPSController>();
        
        if (fpsController == null)
        {
            Debug.LogError("FPSController not found on the same GameObject!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bug"))
        {
            Debug.Log("Player collided with Bug!");
            
            // Call KillPlayer on the Bug script
            Bug bugScript = other.GetComponent<Bug>();
            if (bugScript != null)
            {
                bugScript.KillPlayer();
            }
            
            // Call Die function on the player
            if (fpsController != null)
            {
                fpsController.Die();
            }
            directorSequencer.PlayDeathSequence();
        }
    }
}
