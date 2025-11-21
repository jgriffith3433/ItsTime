using UnityEngine;

[ExecuteAlways]
public class PositionLerp : MonoBehaviour
{
    [SerializeField] private GameObject lerpObject;
    [SerializeField] private GameObject startObject;
    [SerializeField] private GameObject endObject;
    [SerializeField] [Range(0f, 1f)] private float t = 0f;

    private Vector3 startPosition;
    private Vector3 endPosition;

    private float elapsedTime = 0f;
    private bool isLerping = false;

    private void Start()
    {
        UpdatePositions();
    }

    private void Update()
    {
        UpdatePositions();
        lerpObject.transform.position = Vector3.Lerp(startPosition, endPosition, t);
    }

    private void OnValidate()
    {
        UpdatePositions();
        lerpObject.transform.position = Vector3.Lerp(startPosition, endPosition, t);
    }

    private void UpdatePositions()
    {
        if (startObject != null)
        {
            startPosition = startObject.transform.position;
        }

        if (endObject != null)
        {
            endPosition = endObject.transform.position;
        }
    }
}
