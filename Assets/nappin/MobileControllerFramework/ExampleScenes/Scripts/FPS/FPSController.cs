using UnityEngine;
using System.Collections;


namespace MobileControllerFramework
{
    [RequireComponent(typeof(CharacterController))]
    public class FPSController : MonoBehaviour
    {
        [Header("Controller Settings")]
        [Min(0)] public float moveSpeed = 8f;
        [Min(0)] public float smoothMovement = 0.025f;
        [Range(0, 1)] public float deceleration = 0.9f; // Higher = stops faster (0 = slides forever, 1 = instant stop)
        [Min(0)] public float rotationSpeed = 10f; // Speed at which character rotates to face movement direction
        public float normalHeight = 2f;
        public float crouchHeight = 1f;
        public float throwForce = 10f;

        [Header("References")]
        public Analog analog;
        public Swap swap;
        public GameObject type_1;
        public GameObject type_2;
        public GameObject type_3;
        public Animator gunAnim;
        public Animator characterAnim;

        public Rigidbody whiteboardRigidbody;
        public Collider whiteboardCollider;
        public Collider whiteboardTrigger;

        private CharacterController characterController;
        private Transform cameraTransform;
        private Vector3 moveVelocity;
        private bool isCrouched = false;
        private bool isPunching = false;
        private bool punchedWhiteboard = false;

        #region Setup

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            cameraTransform = Camera.main?.transform;

            if (cameraTransform == null)
                Debug.LogError("FPSController: No Main Camera found in the scene.");
        }

        #endregion


        #region Movement

        private void Update()
        {
            HandleMovement();
        }


        private void HandleMovement()
        {
            if (analog == null || cameraTransform == null) return;

            Vector2 input = analog.GetStickPosition();
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
            
            // If no input, apply deceleration to stop faster
            float lerpSpeed = input.magnitude > 0.1f ? smoothMovement : deceleration;
            moveVelocity.x = Mathf.Lerp(moveVelocity.x, targetVelocity.x, lerpSpeed);
            moveVelocity.z = Mathf.Lerp(moveVelocity.z, targetVelocity.z, lerpSpeed);

            // Apply gravity
            if (characterController.isGrounded && moveVelocity.y < 0) moveVelocity.y = -1f;
            moveVelocity.y += Physics.gravity.y * Time.deltaTime;

            characterController.Move(moveVelocity * Time.deltaTime);
        }

        #endregion


        #region Gun / Object Handling

        public void GunSwap()
        {
            if (swap == null) return;

            SwapContent currentItem = swap.GetCurrentItem();
            if (currentItem == null) return;

            type_1.SetActive((int)currentItem.contentType == 0);
            type_2.SetActive((int)currentItem.contentType == 1);
            type_3.SetActive((int)currentItem.contentType == 2);

            if (gunAnim != null)
            {
                gunAnim.Play("Idle");
                gunAnim.Play("Swap");
            }
        }


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

        private IEnumerator PunchWhiteboard()
        {
            whiteboardTrigger.gameObject.SetActive(false); // disable trigger to avoid multiple punches
            yield return new WaitForSeconds(0.15f);
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
    }
}