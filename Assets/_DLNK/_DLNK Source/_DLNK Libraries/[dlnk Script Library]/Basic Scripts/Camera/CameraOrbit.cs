using UnityEngine;
using System.Collections;

public class CameraOrbit : MonoBehaviour
{
    public Transform Target;
    public float Distance = 5.0f;
    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;
    public float yMinLimit = -20.0f;
    public float yMaxLimit = 80.0f;

    [Header("Spaceship Control")]
    [Tooltip("Reference to the spaceship controller to drive based on camera movement")]
    public VattalusSpaceshipController spaceshipController;
    
    [Tooltip("Enable driving spaceship inputs based on camera orbit")]
    public bool driveSpaceshipInputs = true;
    
    [Tooltip("Sensitivity for pitch input (vertical camera movement)")]
    public float pitchInputSensitivity = 1.0f;
    
    [Tooltip("Sensitivity for yaw input (horizontal camera movement)")]
    public float yawInputSensitivity = 1.0f;

    private float x;
    private float y;
    private float previousX;
    private float previousY;

    void Awake()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.x;
        y = angles.y;
        previousX = x;
        previousY = y;

        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().freezeRotation = true;
        }
    }

    void LateUpdate()
    {
        if (Target != null)
        {
            x += (float)(Input.GetAxis("Mouse X") * xSpeed * 0.02f);
            y -= (float)(Input.GetAxis("Mouse Y") * ySpeed * 0.02f);

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 position = rotation * (new Vector3(0.0f, 0.0f, -Distance)) + Target.position;

            transform.rotation = rotation;
            transform.position = position;

            // Drive spaceship inputs based on camera movement
            if (driveSpaceshipInputs && spaceshipController != null)
            {
                // Calculate camera movement deltas
                float deltaX = x - previousX;
                float deltaY = y - previousY;

                // Normalize deltas to -1 to 1 range for input
                float yawInput = Mathf.Clamp(deltaX * yawInputSensitivity * 0.1f, -1f, 1f);
                float pitchInput = Mathf.Clamp(deltaY * pitchInputSensitivity * 0.1f, -1f, 1f);

                // Set the spaceship inputs
                spaceshipController.SetYawInput(yawInput);
                spaceshipController.SetPitchInput(pitchInput);

                // Store current values for next frame
                previousX = x;
                previousY = y;
            }
        }
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }
        if (angle > 360)
        {
            angle -= 360;
        }
        return Mathf.Clamp(angle, min, max);
    }
}
