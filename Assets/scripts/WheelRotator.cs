using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    public float rotationSpeed = 200f;
    public float spinDuration = 3f;

    private float timer = 0f;

    void Update()
    {
        if (timer >= spinDuration) return;

        timer += Time.deltaTime;

        // gradually slows to 0 as timer approaches spinDuration
        float currentSpeed = Mathf.Lerp(rotationSpeed, 0f, timer / spinDuration);
        transform.Rotate(Vector3.right, currentSpeed * Time.deltaTime);
    }
}