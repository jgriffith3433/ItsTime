using UnityEngine;
using MobileControllerFramework;

public class CameraOrbitSpaceShip : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The game object to orbit around")]
    public Transform target;

    [Header("Touch Input")]
    [Tooltip("Reference to the TouchDrag component for input")]
    public TouchDrag touchDrag;

    [Header("Orbit Settings")]
    [Tooltip("Distance from the target")]
    public float distance = 5f;
    
    [Tooltip("Sensitivity for horizontal rotation")]
    public float horizontalSensitivity = 0.5f;
    
    [Tooltip("Sensitivity for vertical rotation")]
    public float verticalSensitivity = 0.5f;
    
    [Tooltip("Minimum vertical angle (looking down)")]
    public float minVerticalAngle = -80f;
    
    [Tooltip("Maximum vertical angle (looking up)")]
    public float maxVerticalAngle = 80f;

    [Header("Smoothing")]
    [Tooltip("Enable smooth rotation")]
    public bool enableSmoothing = true;
    
    [Tooltip("Rotation smoothing factor")]
    public float smoothSpeed = 10f;

    [Header("Spaceship Control")]
    [Tooltip("Reference to the spaceship controller to drive based on camera movement")]
    public VattalusSpaceshipController spaceshipController;
    
    [Tooltip("Enable driving spaceship inputs based on camera orbit")]
    public bool driveSpaceshipInputs = true;
    
    [Tooltip("Sensitivity for pitch input (vertical camera movement)")]
    private float pitchInputSensitivity = 10.0f;
    
    [Tooltip("Sensitivity for yaw input (horizontal camera movement)")]
    private float yawInputSensitivity = 10.0f;

    // Current rotation angles
    private float currentHorizontalAngle = 0f;
    private float currentVerticalAngle = 0f;

    // Target rotation angles (for smoothing)
    private float targetHorizontalAngle = 0f;
    private float targetVerticalAngle = 0f;

    // Previous touch position for delta calculation
    private Vector2 previousTouchPosition = Vector2.zero;

    // Previous angles for spaceship input calculation
    private float previousHorizontalAngle = 0f;
    private float previousVerticalAngle = 0f;


    private void Start()
    {
        // Initialize angles based on current camera position
        if (target != null)
        {
            Vector3 direction = transform.position - target.position;
            currentHorizontalAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            currentVerticalAngle = Mathf.Asin(direction.y / direction.magnitude) * Mathf.Rad2Deg;
            
            targetHorizontalAngle = currentHorizontalAngle;
            targetVerticalAngle = currentVerticalAngle;
            previousHorizontalAngle = currentHorizontalAngle;
            previousVerticalAngle = currentVerticalAngle;
        }

        // Enable external input mode on the spaceship controller
        if (spaceshipController != null && driveSpaceshipInputs)
        {
            spaceshipController.useExternalInput = true;
        }
    }


    private void LateUpdate()
    {
        if (target == null || touchDrag == null)
            return;

        // Get touch drag movement
        Vector2 touchMovement = touchDrag.GetTouchDragMovement();

        // Only process input when touch is active
        if (touchDrag.GetTouchDragState() == TouchDrag.TouchDragState.touch)
        {
            // Calculate delta from previous frame
            Vector2 delta = touchMovement - previousTouchPosition;
            
            // Update target angles based on drag delta
            targetHorizontalAngle += delta.x * horizontalSensitivity;
            targetVerticalAngle -= delta.y * verticalSensitivity;
            
            // Clamp vertical angle
            targetVerticalAngle = Mathf.Clamp(targetVerticalAngle, minVerticalAngle, maxVerticalAngle);
        }

        // Store current position for next frame
        previousTouchPosition = touchMovement;

        // Apply smoothing or direct assignment
        if (enableSmoothing)
        {
            currentHorizontalAngle = Mathf.Lerp(currentHorizontalAngle, targetHorizontalAngle, Time.deltaTime * smoothSpeed);
            currentVerticalAngle = Mathf.Lerp(currentVerticalAngle, targetVerticalAngle, Time.deltaTime * smoothSpeed);
        }
        else
        {
            currentHorizontalAngle = targetHorizontalAngle;
            currentVerticalAngle = targetVerticalAngle;
        }

        // Calculate the new camera position
        Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0f);
        Vector3 offset = rotation * Vector3.back * distance;
        transform.position = target.position + offset;

        // Make camera look at the target
        transform.LookAt(target);

        // Apply ship's roll to the camera
        if (spaceshipController != null)
        {
            // Get the ship's current roll angle from its transform
            float shipRoll = target.eulerAngles.z;
            
            // Apply the roll to the camera while maintaining look-at
            Vector3 currentEuler = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(currentEuler.x, currentEuler.y, shipRoll);
        }

        // Drive spaceship inputs based on camera movement (only when actively dragging)
        if (driveSpaceshipInputs && spaceshipController != null)
        {
            if (touchDrag.GetTouchDragState() == TouchDrag.TouchDragState.touch)
            {
                // Calculate camera movement deltas
                float deltaHorizontal = currentHorizontalAngle - previousHorizontalAngle;
                float deltaVertical = currentVerticalAngle - previousVerticalAngle;

                // Normalize deltas to -1 to 1 range for input
                float yawInput = Mathf.Clamp(deltaHorizontal * yawInputSensitivity * 0.1f, -1f, 1f);
                float pitchInput = Mathf.Clamp(deltaVertical * pitchInputSensitivity * 0.1f, -1f, 1f);

                // Set the spaceship inputs
                spaceshipController.SetYawInput(yawInput);
                spaceshipController.SetPitchInput(pitchInput);
            }
            else
            {
                // Reset inputs when not touching
                spaceshipController.SetYawInput(0f);
                spaceshipController.SetPitchInput(0f);
            }

            // Store current values for next frame
            previousHorizontalAngle = currentHorizontalAngle;
            previousVerticalAngle = currentVerticalAngle;
        }
    }


    private void OnValidate()
    {
        // Ensure distance is positive
        if (distance < 0.1f)
            distance = 0.1f;

        // Ensure min/max angles are valid
        if (minVerticalAngle > maxVerticalAngle)
            minVerticalAngle = maxVerticalAngle;
    }


    #region Public Methods

    /// <summary>
    /// Set a new target to orbit around
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        
        if (target != null)
        {
            Vector3 direction = transform.position - target.position;
            distance = direction.magnitude;
        }
    }


    /// <summary>
    /// Set the orbit distance
    /// </summary>
    public void SetDistance(float newDistance)
    {
        distance = Mathf.Max(0.1f, newDistance);
    }


    /// <summary>
    /// Reset the camera to a specific angle
    /// </summary>
    public void ResetToAngle(float horizontal, float vertical)
    {
        currentHorizontalAngle = horizontal;
        currentVerticalAngle = Mathf.Clamp(vertical, minVerticalAngle, maxVerticalAngle);
        targetHorizontalAngle = currentHorizontalAngle;
        targetVerticalAngle = currentVerticalAngle;
    }

    #endregion
}
