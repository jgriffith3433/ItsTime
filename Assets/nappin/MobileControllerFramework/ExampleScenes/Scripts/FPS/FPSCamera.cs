using System.Collections;
using UnityEngine;


namespace MobileControllerFramework
{
    [RequireComponent(typeof(Transform))]
    public class FPSCamera : MonoBehaviour
    {
        public float shakeIntensity = 0.1f;
        public float shakeDuration = 0.5f;

        [Header("Input")]
        public Analog fireAnalog;
        
        [Header("Camera Settings")]
        [Tooltip("Minimum range for horizontal camera movement (left)")]
        public float xMinRange = -0.5f;
        [Tooltip("Maximum range for horizontal camera movement (right)")]
        public float xMaxRange = 0.5f;
        [Tooltip("Minimum range for vertical camera movement (down)")]
        public float yMinRange = -0.5f;
        [Tooltip("Maximum range for vertical camera movement (up)")]
        public float yMaxRange = 0.5f;
        [Tooltip("Speed at which camera lerps to target position")]
        [Range(0.01f, 1f)]
        public float lerpSpeed = 0.1f;
        [Tooltip("Deadzone threshold - input must exceed this to move camera")]
        [Range(0f, 1f)]
        private float deadzone = 0.1f;

        private Vector3 targetLocalPosition;

        private void Start()
        {
            targetLocalPosition = Vector3.zero;
            transform.localPosition = Vector3.zero;
        }

        private void Update()
        {
            if (Application.isMobilePlatform)
            {
                HandleMobileCamera();
            }
            else
            {
                HandleDesktopCamera();
            }

            // Lerp to target position
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, lerpSpeed);
        }

        private void HandleMobileCamera()
        {
            if (fireAnalog == null) return;

            Vector2 stickPosition = fireAnalog.GetStickPosition();

            // Apply deadzone - input must exceed threshold to move camera
            if (stickPosition.magnitude < deadzone)
            {
                targetLocalPosition = Vector3.zero;
            }
            else
            {
                // Map stick position to camera offset using min/max ranges
                float xOffset = stickPosition.x < 0 ? stickPosition.x * Mathf.Abs(xMinRange) : stickPosition.x * xMaxRange;
                float yOffset = stickPosition.y < 0 ? stickPosition.y * Mathf.Abs(yMinRange) : stickPosition.y * yMaxRange;
                targetLocalPosition = new Vector3(xOffset, yOffset, 0);
            }
        }

        private void HandleDesktopCamera()
        {
            // Get mouse position normalized to center of screen (-0.5 to 0.5)
            Vector2 mousePos = new Vector2(
                (Input.mousePosition.x / Screen.width) - 0.5f,
                (Input.mousePosition.y / Screen.height) - 0.5f
            );

            // Scale by 2 to convert from (-0.5 to 0.5) to (-1 to 1)
            Vector2 normalizedMousePos = mousePos * 2f;
            
            // Apply deadzone - mouse must be at least halfway from center
            if (normalizedMousePos.magnitude < deadzone)
            {
                targetLocalPosition = Vector3.zero;
            }
            else
            {
                // Map mouse position to camera offset using min/max ranges
                float xOffset = normalizedMousePos.x < 0 ? normalizedMousePos.x * Mathf.Abs(xMinRange) : normalizedMousePos.x * xMaxRange;
                float yOffset = normalizedMousePos.y < 0 ? normalizedMousePos.y * Mathf.Abs(yMinRange) : normalizedMousePos.y * yMaxRange;
                targetLocalPosition = new Vector3(xOffset, yOffset, 0);
            }
        }

        public void ShakeCamera()
        {
            StartCoroutine(Shake());
        }


        private IEnumerator Shake()
        {
            Vector3 originalPosition = transform.localPosition;
            float elapsedTime = 0f;

            while (elapsedTime < shakeDuration)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-shakeIntensity, shakeIntensity),
                    Random.Range(-shakeIntensity, shakeIntensity),
                    0f
                );

                transform.localPosition = originalPosition + offset;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalPosition;
        }
    }
}