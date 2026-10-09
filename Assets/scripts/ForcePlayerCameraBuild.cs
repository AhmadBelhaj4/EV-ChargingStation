using UnityEngine;

public class ForcePlayerCameraBuild : MonoBehaviour
{
    [Header("Assign your player camera here")]
    public Camera playerCamera;

    [Header("Optional: assign the player start position")]
    public Transform spawnPoint;

    void Awake()
    {
        // Move player to spawn position if assigned
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        // Disable all other cameras
        Camera[] allCameras = FindObjectsOfType<Camera>();

        foreach (Camera cam in allCameras)
        {
            cam.enabled = false;
            cam.gameObject.tag = "Untagged";
        }

        // Enable only player camera
        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            playerCamera.gameObject.tag = "MainCamera";

            AudioListener listener = playerCamera.GetComponent<AudioListener>();
            if (listener == null)
            {
                playerCamera.gameObject.AddComponent<AudioListener>();
            }
        }
        else
        {
            Debug.LogError("ForcePlayerCameraBuild: Player Camera is not assigned!");
        }

        // Lock cursor for movement
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("ForcePlayerCameraBuild applied. Player camera forced.");
    }
}