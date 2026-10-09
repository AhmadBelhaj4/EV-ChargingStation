using UnityEngine;

public class CarDoorTimeline : MonoBehaviour
{
    [Header("Rotation")]
    public float closedAngle = 0f;
    public float openAngle = 70f;

    [Header("Timeline")]
    public float openStartTime = 2f;   // when opening begins
    public float closeStartTime = 5f;  // when closing begins
    public float duration = 1.5f;      // time to open/close

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        float angle;

        // Phase 1: Closed
        if (timer < openStartTime)
        {
            angle = closedAngle;
        }
        // Phase 2: Opening
        else if (timer < openStartTime + duration)
        {
            float t = (timer - openStartTime) / duration;
            t = Mathf.SmoothStep(0f, 1f, t); // smooth easing
            angle = Mathf.Lerp(closedAngle, openAngle, t);
        }
        // Phase 3: Open (waiting before closing)
        else if (timer < closeStartTime)
        {
            angle = openAngle;
        }
        // Phase 4: Closing
        else if (timer < closeStartTime + duration)
        {
            float t = (timer - closeStartTime) / duration;
            t = Mathf.SmoothStep(0f, 1f, t);
            angle = Mathf.Lerp(openAngle, closedAngle, t);
        }
        // Phase 5: Fully closed again
        else
        {
            angle = closedAngle;
        }

        // Apply rotation (Y axis)
        Vector3 rot = transform.localEulerAngles;
        rot.y = angle;
        transform.localEulerAngles = rot;
    }
}
