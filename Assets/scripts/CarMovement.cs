using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public float speed = 5f;

    public GameObject startButtonObject;
    public GameObject stopButtonObject;
    public GameObject exitButtonObject;

    private bool isMoving = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        ResetCarButtons();
    }

    void FixedUpdate()
    {
        if (!isMoving || rb == null) return;

        Vector3 newPosition = rb.position - transform.forward * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    public void StartCar()
    {
        if (isMoving) return;

        isMoving = true;

        if (startButtonObject != null)
            startButtonObject.SetActive(false);

        if (stopButtonObject != null)
            stopButtonObject.SetActive(true);

        if (exitButtonObject != null)
            exitButtonObject.SetActive(false);
    }

    public void StopCar()
    {
        isMoving = false;

        if (startButtonObject != null)
            startButtonObject.SetActive(false);

        if (stopButtonObject != null)
            stopButtonObject.SetActive(false);

        if (exitButtonObject != null)
            exitButtonObject.SetActive(true);
    }

    public void ResetCarButtons()
    {
        isMoving = false;

        if (startButtonObject != null)
            startButtonObject.SetActive(true);

        if (stopButtonObject != null)
            stopButtonObject.SetActive(false);

        if (exitButtonObject != null)
            exitButtonObject.SetActive(false);
    }
}