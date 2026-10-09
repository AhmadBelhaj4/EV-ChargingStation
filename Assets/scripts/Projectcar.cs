using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Projectcar : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float verticalSpeed = 5f;
    public bool flyMode = true;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2.0f;
    public Transform cameraRoot;
    public float pitchMin = -90f;
    public float pitchMax = 90f;

    [Header("Cursor Lock")]
    public bool lockCursorOnStart = true;
    public KeyCode unlockKey = KeyCode.Escape;

    public bool uiOpen = false;

    [Header("Control State")]
    public bool allowMovement = true;   // <- nouveau
    public bool allowLook = true;       // <- nouveau

    private CharacterController controller;
    private float pitch;
    private bool cursorLocked;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraRoot == null && Camera.main != null)
            cameraRoot = Camera.main.transform;

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

        if (uiOpen)
            return;

        if (controller == null || cameraRoot == null)
            return;

        if (!gameObject.activeInHierarchy)
            return;

        // ===== LOOK =====
        if (cursorLocked && allowLook)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

            transform.Rotate(Vector3.up * mouseX);
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        // ===== MOVEMENT =====
        float moveX = 0f;
        float moveZ = 0f;

        if (allowMovement)
        {
            moveX = Input.GetAxisRaw("Horizontal");
            moveZ = Input.GetAxisRaw("Vertical");
        }

        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized * moveSpeed;

        float upDown = 0f;
        if (flyMode && allowMovement)
        {
            if (Input.GetKey(KeyCode.Q)) upDown += 1f;
            if (Input.GetKey(KeyCode.E)) upDown -= 1f;
        }

        Vector3 vertical = Vector3.up * (upDown * verticalSpeed);

        if (controller.enabled && gameObject.activeInHierarchy)
            controller.Move((move + vertical) * Time.deltaTime);
    }

    void ApplyCursorState()
    {
        Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !cursorLocked;
    }

    public void OpenUI()
    {
        uiOpen = true;
        cursorLocked = false;
        ApplyCursorState();
    }

    public void CloseUI()
    {
        uiOpen = false;
        cursorLocked = true;
        ApplyCursorState();
    }
}