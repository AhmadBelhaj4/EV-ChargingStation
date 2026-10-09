using UnityEngine;
using UnityEngine.EventSystems;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;

    [Header("References")]
    public Transform playerBody;

    private float xRotation = 0f;
    private bool cursorLocked = true;

    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        // Escape unlocks cursor for UI
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
            return;
        }

        // Left click locks cursor again ONLY if not clicking UI
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            LockCursor();
        }

        // Do not rotate camera when cursor is unlocked
        if (!cursorLocked)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    public void LockCursor()
    {
        cursorLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        cursorLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}