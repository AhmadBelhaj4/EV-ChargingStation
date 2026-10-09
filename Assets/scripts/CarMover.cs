using UnityEngine;

public class CarMover : MonoBehaviour
{
    public float moveSpeed = 1f;     // starting speed
    public float moveDuration = 5f;   // how long it moves before stopping

    private float timer = 0f;

    void Update()
    {
        if (timer >= moveDuration) return;

        timer += Time.deltaTime;

        // Smooth deceleration
        float currentSpeed = Mathf.Lerp(moveSpeed, 0f, timer / moveDuration);

        // Move along Z axis
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }
}
