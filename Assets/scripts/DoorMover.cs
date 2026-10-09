using UnityEngine;

public class DoorMover : MonoBehaviour
{
    [Header("Positions")]
    public float closedY = 0f;   // Y when door is closed
    public float openY = 3f;     // Y when door is open

    [Header("Timing")]
    public float moveDuration = 2f; // time to fully open/close

    [Header("Control")]
    public bool isOpen = false;     // target state (open/close)
    public bool isMoving = false;   // when movement begins

    private float timer = 0f;
    private float startY;
    private float targetY;

    void Update()
    {
        if (!isMoving) return;

        timer += Time.deltaTime;

        float t = timer / moveDuration;

        // Smooth easing (deceleration)
        float smoothT = 1f - Mathf.Pow(1f - t, 2f);

        float newY = Mathf.Lerp(startY, targetY, smoothT);

        Vector3 pos = transform.position;
        pos.y = newY;
        transform.position = pos;

        // Stop when finished
        if (timer >= moveDuration)
        {
            isMoving = false;
        }
    }

    // 🔓 Call this to OPEN
    public void OpenDoor()
    {
        startY = transform.position.y;
        targetY = openY;

        timer = 0f;
        isOpen = true;
        isMoving = true;
    }

    // 🔒 Call this to CLOSE
    public void CloseDoor()
    {
        startY = transform.position.y;
        targetY = closedY;

        timer = 0f;
        isOpen = false;
        isMoving = true;
    }
}
