using UnityEngine;

/// <summary>
/// Simple script to attach to your player GameObject.
/// Ensures the player has the "Player" tag for collision detection.
/// 
/// Setup:
/// 1. Attach this script to your player GameObject
/// 2. Make sure your player has a Collider (not a trigger)
/// 3. Tag your player as "Player" in the Inspector
/// </summary>
public class PlayerControllerAlt : MonoBehaviour
{
    private void OnEnable()
    {
        // Verify player has correct tag
        if (!gameObject.CompareTag("Player"))
        {
            Debug.LogWarning($"Player GameObject '{gameObject.name}' should be tagged as 'Player' for proper detection.");
        }
    }
}