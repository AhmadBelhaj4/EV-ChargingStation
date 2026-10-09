using UnityEngine;

public class PlayerControllercooloing : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float mouseSensitivity = 2f;

    private float xRotation = 0f;
    private CharacterController cc;
    private Transform cam;
    private bool canMove = false; // GameManager controls this

    void Start()
    {
        cc = GetComponent<CharacterController>();
        cam = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!canMove) return;

        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // WASD movement
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;
        cc.Move(move * moveSpeed * Time.deltaTime);
        // Simple gravity
        cc.Move(Vector3.down * 9.8f * Time.deltaTime);
    }

    public void SetMovement(bool enabled)
    {
        canMove = enabled;
        Cursor.lockState = enabled
            ? CursorLockMode.Locked
            : CursorLockMode.None;
    }
}