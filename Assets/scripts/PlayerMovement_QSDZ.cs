using UnityEngine;
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement_QSDZ : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 50f;
    public float verticalSpeed = 10f;   // Q/E
    public bool flyMode = true;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2.0f;
    public Transform cameraRoot;       // Main Camera transform
    public float pitchMin = -90f;
    public float pitchMax = 90f;

    [Header("Cursor Lock")]
    public bool lockCursorOnStart = true;
    public KeyCode unlockKey = KeyCode.Escape;

    CharacterController controller;
    float pitch;
    bool cursorLocked;
    float verticalVelocity = 0f;

    /// <summary>Set to true from other scripts to freeze camera rotation (e.g. while a UI slider is active).</summary>
    public static bool FreezeLook { get; set; }

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraRoot == null && Camera.main != null)
            cameraRoot = Camera.main.transform;
        pitch = cameraRoot.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        cursorLocked = lockCursorOnStart;
        ApplyCursorState();
    }


    void Update()
    {
        if (Input.GetKeyDown(unlockKey))
        {
            cursorLocked = !cursorLocked;
            ApplyCursorState();
        }

        if (controller == null || cameraRoot == null)
            return;

        // Mouse look — skip when FreezeLook is active, UNLESS right mouse button is held
        bool allowLook = cursorLocked && !FreezeLook;
        bool rightClickLook = FreezeLook && Input.GetMouseButton(1);

        if (allowLook || rightClickLook)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

            transform.Rotate(Vector3.up * mouseX);
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        // Horizontal movement (QSDZ)
        float moveX = Input.GetAxisRaw("Horizontal"); // Q/D
        float moveZ = Input.GetAxisRaw("Vertical");   // Z/S

        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized * moveSpeed;

        // Vertical movement
        if (flyMode)
        {
            verticalVelocity = 0f;
            if (Input.GetKey(KeyCode.Q)) verticalVelocity = verticalSpeed;
            if (Input.GetKey(KeyCode.E)) verticalVelocity = -verticalSpeed;
        }
        else
        {
            // Apply gravity when not flying
            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 vertical = Vector3.up * verticalVelocity;

        // Final movement
        controller.Move((move + vertical) * Time.deltaTime);
    }

    void ApplyCursorState()
    {
        Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !cursorLocked;
    }
}