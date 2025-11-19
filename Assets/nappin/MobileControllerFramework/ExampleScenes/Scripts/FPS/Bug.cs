using UnityEngine;

namespace MobileControllerFramework
{
    public class Bug : MonoBehaviour
    {
        [Header("Animation")]
        [Tooltip("The Animator component for the bug")]
        public Animator bugAnimator;
        
        [Tooltip("Name of the kill animation trigger")]
        public string killAnimationTrigger = "Kill";
        
        [Header("Movement Settings")]
        [Tooltip("Speed at which the bug moves forward")]
        [Min(0)] public float moveSpeed = 5f;

        [Header("Explosion Settings")]
        [Tooltip("Explosion prefab to instantiate when bug is destroyed")]
        [SerializeField] private GameObject explosionPrefab;

        [SerializeField] private FPSController fpsController;
        
        [Tooltip("Time in seconds before explosion and bug are destroyed")]
        [Min(0)] private float explosionLifetime = 15f;

        private void Awake()
        {
            if (bugAnimator == null)
            {
                bugAnimator = GetComponent<Animator>();
            }
        }

        private void Update()
        {
            if (fpsController == null)
            {
                return;
            }
            // Move the bug toward the fps controller based on its transform.forward direction
            var previousPosition = transform.position;
            var directionTowardPlayer = (fpsController.transform.position - transform.position);

            if (directionTowardPlayer.magnitude < 5f)
            {
                var directionNormalized = directionTowardPlayer.normalized;
                transform.position += directionNormalized * moveSpeed * Time.deltaTime;
                transform.position = new Vector3(transform.position.x, previousPosition.y, transform.position.z);
                // rotate to face the player
                transform.forward = directionNormalized;
            }
            else
            {
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
            }
        }

        public void Explode()
        {
            if (explosionPrefab != null)
            {
                // Instantiate explosion at bug's position with -90 degrees X rotation
                Quaternion explosionRotation = Quaternion.Euler(-90f, 0f, 0f);
                GameObject explosion = Instantiate(explosionPrefab, transform.position, explosionRotation);
                
                // Destroy explosion after specified lifetime
                Destroy(explosion, explosionLifetime);
            }

            // Destroy the bug after the same lifetime
            Destroy(gameObject);
        }

        public void KillPlayer()
        {
            Debug.Log("Bug is killing the player!");
            
            if (bugAnimator != null)
            {
                bugAnimator.SetTrigger(killAnimationTrigger);
            }
            else
            {
                Debug.LogWarning("Bug Animator not assigned!");
            }
        }
    }
}
