using UnityEngine;
using System.Collections;
using UnityEngine.Playables;


namespace MobileControllerFramework
{
    [RequireComponent(typeof(CharacterController))]
    public class FPSController : MonoBehaviour
    {
        [SerializeField] private PlayableDirector playableDirector;
        public PlayableDirector introTimeline;

        [Header("Controller Settings")]
        [Tooltip("Enable or disable character movement and input processing")]
        public bool controlsEnabled = true;
        [Min(0)] public float moveSpeed = 8f;
        [Min(0)] private float constantForwardSpeed = 0.15f; // Constant forward movement speed
        [Min(0)] public float smoothMovement = 0.025f;
        [Range(0, 1)] public float deceleration = 0.9f; // Higher = stops faster (0 = slides forever, 1 = instant stop)
        [Min(0)] public float rotationSpeed = 5f; // Speed at which character rotates to face movement direction
        public float normalHeight = 2f;
        public float crouchHeight = 1f;
        public float throwForce = 10f;

        [Header("Character Mesh")]
        [Tooltip("The character mesh transform to rotate for aiming")]
        [SerializeField] private Transform characterMesh;
        [Tooltip("Speed at which character mesh rotates to simulate aiming")]
        [Min(0)] public float meshRotationSpeed = 5f;

        [Header("Raycast Target")]
        [Tooltip("GameObject to position at raycast hit point")]
        [SerializeField] private GameObject bulletHits;
        [Tooltip("Perform raycast every N frames (1 = every frame, 2 = every other frame, etc.)")]
        [Min(1)] private int raycastFrameInterval = 15;
        [Tooltip("Maximum raycast distance")]
        [Min(1)] private float raycastMaxDistance = 25f;

        [Header("References")]
        public Analog analog;
        public Analog fireAnalog;
        public MobileControllerFramework.Button punchButton;
        public Swap swap;
        public GameObject type_1;
        public GameObject type_2;
        public GameObject type_3;
        public Animator gunAnim;
        public Animator characterAnim;
        public GameObject muzzleFire;
        public GameObject bulletShells;
        public AudioSource gunAudio;

        public Rigidbody whiteboardRigidbody;
        public Collider whiteboardCollider;
        public Collider whiteboardTrigger;

        private CharacterController characterController;
        private Transform cameraTransform;
        private Vector3 moveVelocity;
        private bool isCrouched = false;
        private bool isPunching = false;
        private bool punchedWhiteboard = false;
        private bool wasControlsEnabled = true;
        private bool isFiring = false;
        private int frameCounter = 0;
        private int raycastLayerMask;

        #region Setup

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            cameraTransform = Camera.main?.transform;

            if (cameraTransform == null)
                Debug.LogError("FPSController: No Main Camera found in the scene.");

            if (Application.isMobilePlatform)
            {
                analog.gameObject.SetActive(true);
                fireAnalog.gameObject.SetActive(true);
            }
            else
            {
                analog.gameObject.SetActive(false);
                fireAnalog.gameObject.SetActive(false);
            }

            // Create layer mask that ignores Player layer
            raycastLayerMask = ~LayerMask.GetMask("Player");
        }

        #endregion


        #region Movement

        private void Update()
        {
            // Reset velocity when controls are disabled
            if (!controlsEnabled && wasControlsEnabled)
            {
                moveVelocity = Vector3.zero;
            }
            wasControlsEnabled = controlsEnabled;

            // if mobile
            if (Application.isMobilePlatform)
            {
                HandleMobileMovement();
            }
            else
            {
                HandlePCMovement();
            }
            
            if (gunAnim != null)
            {
                gunAnim.SetBool("Fire", isFiring);
            }

            if (muzzleFire != null)
            {
                muzzleFire.SetActive(isFiring);
            }
            if (bulletShells != null)
            {
                bulletShells.SetActive(isFiring);
            }
            if (bulletHits != null)
            {
                bulletHits.SetActive(isFiring);
            }
            if (gunAudio != null)
            {
                if (isFiring)
                {
                    if (!gunAudio.isPlaying)
                    {
                        gunAudio.Play();
                    }
                }
                else
                {
                    if (gunAudio.isPlaying)
                    {
                        gunAudio.Stop();
                    }
                }
            }
        }

        private void LateUpdate()
        {
            RotateCharacterMeshTowardsCameraAim();
            if (isFiring)
            {
                UpdateBulletHits();
            }
        }

        private void UpdateBulletHits()
        {
            if (bulletHits == null || cameraTransform == null) return;

            // Only perform raycast every N frames
            frameCounter++;
            if (frameCounter >= raycastFrameInterval)
            {
                frameCounter = 0;

                Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, raycastMaxDistance, raycastLayerMask))
                {
                    // Position target at hit point
                    bulletHits.transform.position = hit.point;
                    
                    // Rotate target to look at camera
                    Vector3 directionToCamera = cameraTransform.position - hit.point;
                    if (directionToCamera.magnitude > 0.01f)
                    {
                        bulletHits.transform.rotation = Quaternion.LookRotation(directionToCamera);
                    }

                    // Check if hit object has "Bug" tag
                    if (hit.collider.CompareTag("Bug"))
                    {
                        Bug bug = hit.collider.GetComponent<Bug>();
                        if (bug != null)
                        {
                            bug.Explode();
                        }
                    }
                }
                else
                {
                    // If no hit, position at max distance
                    bulletHits.transform.position = cameraTransform.position + cameraTransform.forward * raycastMaxDistance;
                    bulletHits.transform.rotation = Quaternion.LookRotation(cameraTransform.position - bulletHits.transform.position);
                }
            }
        }

        private void RotateCharacterMeshTowardsCameraAim()
        {
            if (characterMesh == null || cameraTransform == null) return;

            // Get the camera's look direction
            Vector3 cameraForward = cameraTransform.forward;
            
            // Clamp vertical rotation - only allow upward rotation (positive Y), not downward
            if (cameraForward.y < 0)
            {
                cameraForward.y = 0;
            }
            
            if (cameraForward.magnitude > 0.01f)
            {
                cameraForward.Normalize();
                
                // Calculate target rotation for the character mesh
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                
                // Smoothly rotate the character mesh
                characterMesh.rotation = Quaternion.Slerp(characterMesh.rotation, targetRotation, meshRotationSpeed * Time.deltaTime);
            }
        }


        private void HandleMobileMovement()
        {
            if (!controlsEnabled || analog == null || fireAnalog == null || cameraTransform == null) return;

            Vector2 input = analog.GetStickPosition();
            
            // Apply threshold of 0.8f to analog input
            if (input.magnitude < 0.8f)
            {
                input = Vector2.zero;
            }
            
            // Prevent backward movement (negative Y input)
            if (input.y < 0)
            {
                input.y = 0;
            }
            
            Vector3 inputDir = cameraTransform.forward * input.y + cameraTransform.right * input.x;
            inputDir.y = 0; // prevent vertical movement from camera tilt
            inputDir.Normalize();

            // Rotate character to face movement direction
            if (inputDir.magnitude > 0.1f) // Only rotate if there's significant input
            {
                Quaternion targetRotation = Quaternion.LookRotation(inputDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Apply movement
            Vector3 targetVelocity = inputDir * moveSpeed;
            
            // Add constant forward movement
            Vector3 constantForward = transform.forward * constantForwardSpeed;
            targetVelocity += constantForward;
            
            // If no input, apply deceleration to stop faster
            float lerpSpeed = input.magnitude > 0.1f ? smoothMovement : deceleration;
            moveVelocity.x = Mathf.Lerp(moveVelocity.x, targetVelocity.x, lerpSpeed);
            moveVelocity.z = Mathf.Lerp(moveVelocity.z, targetVelocity.z, lerpSpeed);

            // Apply gravity
            if (characterController.isGrounded && moveVelocity.y < 0) moveVelocity.y = -1f;
            moveVelocity.y += Physics.gravity.y * Time.deltaTime;

            characterController.Move(moveVelocity * Time.deltaTime);

            // Firing based on fireAnalog input
            if (fireAnalog.IsDragging())
            {
                isFiring = true;
            }
            else
            {
                isFiring = false;
            }
        }

        private void HandlePCMovement()
        {
            if (!controlsEnabled || cameraTransform == null) return;

            float inputX = Input.GetAxis("Horizontal");
            float inputZ = Input.GetAxis("Vertical");
            
            // Prevent backward movement (S key, negative vertical input)
            if (inputZ < 0)
            {
                inputZ = 0;
            }
            
            Vector3 inputDir = cameraTransform.forward * inputZ + cameraTransform.right * inputX;
            inputDir.y = 0; // prevent vertical movement from camera tilt
            inputDir.Normalize();

            // Rotate character to face movement direction
            if (inputDir.magnitude > 0.1f) // Only rotate if there's significant input
            {
                Quaternion targetRotation = Quaternion.LookRotation(inputDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Apply movement
            Vector3 targetVelocity = inputDir * moveSpeed;

            // Add constant forward movement
            Vector3 constantForward = transform.forward * constantForwardSpeed;
            targetVelocity += constantForward;

            // If no input, apply deceleration to stop faster
            float lerpSpeed = (Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputZ) > 0.1f) ? smoothMovement : deceleration;
            moveVelocity.x = Mathf.Lerp(moveVelocity.x, targetVelocity.x, lerpSpeed);
            moveVelocity.z = Mathf.Lerp(moveVelocity.z, targetVelocity.z, lerpSpeed);

            // Apply gravity
            if (characterController.isGrounded && moveVelocity.y < 0) moveVelocity.y = -1f;
            moveVelocity.y += Physics.gravity.y * Time.deltaTime;

            characterController.Move(moveVelocity * Time.deltaTime);


            // firing is left click
            if (Input.GetMouseButton(0))
            {
                isFiring = true;
            }
            else
            {
                isFiring = false;
            }
        }

        #endregion


        #region Gun / Object Handling


        public void ToggleHeight()
        {
            isCrouched = !isCrouched;
            characterController.height = isCrouched ? crouchHeight : normalHeight;
        }

        public void PunchForward()
        {
            if (characterAnim != null)
            {
                if (!isPunching)
                {
                    isPunching = true;
                    StartCoroutine(PunchingReset());
                }
                if (!punchedWhiteboard)
                {
                    // check if facing whiteboard to apply force
                    if (whiteboardRigidbody != null)
                    {
                        Vector3 toWhiteboard = whiteboardRigidbody.transform.position - transform.position;
                        toWhiteboard.y = 0;
                        toWhiteboard.Normalize();

                        float dot = Vector3.Dot(transform.forward, toWhiteboard);
                        if (dot > 0.5f) // roughly facing the whiteboard
                        {
                            characterAnim.Play("PunchForward");
                            punchedWhiteboard = true;
                            StartCoroutine(PunchWhiteboard());
                        }
                    }
                }
            }
        }

        private IEnumerator PunchingReset()
        {
            yield return new WaitForSeconds(0.5f);
            isPunching = false;
        }

        public void StartFiring()
        {
            isFiring = true;
        }

        public void StopFiring()
        {
            isFiring = false;
        }

        private IEnumerator PunchWhiteboard()
        {
            if (introTimeline != null && introTimeline.state == PlayState.Playing)
            {
                introTimeline.Stop();
            }
            playableDirector.Play();
            whiteboardTrigger.gameObject.SetActive(false); // disable trigger to avoid multiple punches
            yield return new WaitForSeconds(0.15f);
            punchButton.gameObject.SetActive(false);
            whiteboardRigidbody.isKinematic = false; // ensure physics is enabled
            Vector3 forceDir = whiteboardRigidbody.transform.up + Vector3.up * 0.1f;
            forceDir.Normalize();
            whiteboardRigidbody.AddForce(forceDir * 8f, ForceMode.Impulse);
            yield return new WaitForSeconds(1f);
            whiteboardCollider.enabled = false;
            whiteboardRigidbody.isKinematic = true; // ensure physics is enabled
        }
        
        public void ThrowObject(GameObject objPrefab)
        {
            if (objPrefab == null || cameraTransform == null) return;

            // Instantiate object slightly in front of the camera
            Vector3 spawnPos = cameraTransform.position + cameraTransform.forward;
            GameObject throwable = Instantiate(objPrefab, spawnPos, Random.rotation);

            Rigidbody rb = throwable.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(cameraTransform.forward * throwForce, ForceMode.Impulse);
            }
        }

        #endregion


        #region Player Death

        public void Die()
        {
            Debug.Log("Player has died!");
            
            // Disable controls
            controlsEnabled = false;
            
            // Stop movement
            moveVelocity = Vector3.zero;
            
            // Play death animation if available
            if (characterAnim != null)
            {
                characterAnim.SetTrigger("Die");
            }
            
            // You can add additional death logic here:
            // - Respawn after delay
            // - Show game over screen
            // - Reload scene
            // - etc.
        }

        #endregion
    }
}