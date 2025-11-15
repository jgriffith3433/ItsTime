using UnityEngine;

public class MovementAnimator : MonoBehaviour
{
    public CharacterController controller;
    public Animator animator;

    [Header("Movement Settings")]
    public float maxWalkSpeed = 2f;
    public float maxRunSpeed = 6f;
    public float smoothTime = 0.1f; // Controls how fast the animation transitions

    private Vector3 lastPosition;
    private float currentAnimSpeed = 0f;
    private float velocity = 0f; // Used by SmoothDamp

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // Compute delta movement per second
        Vector3 delta = transform.position - lastPosition;
        float speed = delta.magnitude / Time.deltaTime;

        // Convert speed → normalized (0 to 0.5) for your blend tree thresholds
        float normalizedSpeed;

        if (speed <= maxWalkSpeed)
        {
            // Idle → Walk (0 → 0.25)
            normalizedSpeed = Mathf.Lerp(0f, 0.25f, speed / maxWalkSpeed);
        }
        else
        {
            // Walk → Run (0.25 → 0.5)
            float runPct = (speed - maxWalkSpeed) / (maxRunSpeed - maxWalkSpeed);
            normalizedSpeed = Mathf.Lerp(0.25f, 0.5f, runPct);
        }

        // Clamp to avoid overshooting
        normalizedSpeed = Mathf.Clamp(normalizedSpeed, 0f, 0.5f);

        // Smooth the animation speed for fluid transitions
        currentAnimSpeed = Mathf.SmoothDamp(currentAnimSpeed, normalizedSpeed, ref velocity, smoothTime);

        // Feed smoothed value into Blend Tree
        animator.SetFloat("Speed", currentAnimSpeed);

        lastPosition = transform.position;
    }
}
