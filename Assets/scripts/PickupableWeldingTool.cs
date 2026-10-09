using UnityEngine;

/// <summary>
/// Attach to the welding tool sitting in the world.
/// Shows a pickup prompt when player is near.
/// Return logic is handled by PlayerWelder (this GO is disabled when picked up).
/// </summary>
public class PickupableWeldingTool : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private Transform _player;
    private PlayerWelder _playerWelder;
    private bool _isPickedUp = false;

    // Original transform — used to place the tool back
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Transform _originalParent;

    void Start()
    {
        // Save the tool's starting transform
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
        _originalParent = transform.parent;

        // Find the player by tag
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            _player = playerGO.transform;
            _playerWelder = playerGO.GetComponentInChildren<PlayerWelder>();
            if (_playerWelder == null)
                _playerWelder = playerGO.GetComponent<PlayerWelder>();
        }
        else
        {
            Debug.LogError("No GameObject tagged 'Player' found!");
        }
    }

    void Update()
    {
        if (_player == null || _isPickedUp) return;

        float distance = Vector3.Distance(_originalPosition, _player.position);
        if (distance > pickupRange) return;

        if (Input.GetKeyDown(interactKey))
            Pickup();
    }

    void Pickup()
    {
        _isPickedUp = true;

        if (_playerWelder != null)
            _playerWelder.EquipTool(this); // pass self so PlayerWelder can call back
        else
            Debug.LogError("No PlayerWelder found on player!");

        QuestManager.Instance?.UpdateQuest("pickup_tool");

        gameObject.SetActive(false);
        Debug.Log("Welding tool picked up!");
    }

    /// <summary>
    /// Called by PlayerWelder when the player presses E to return the tool.
    /// </summary>
    public void ReturnFromPlayer()
    {
        _isPickedUp = false;

        if (_playerWelder != null)
            _playerWelder.UnequipTool();

        // Re-show the tool at its original position
        transform.SetParent(_originalParent);
        transform.position = _originalPosition;
        transform.rotation = _originalRotation;
        gameObject.SetActive(true);

        Debug.Log("Welding tool returned to original position!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }

    void OnGUI()
    {
        if (_player == null || _isPickedUp) return;

        float distance = Vector3.Distance(_originalPosition, _player.position);
        if (distance > pickupRange) return;

        string key = PromptUI.KeyName(interactKey);
        PromptUI.Draw(key, "Pick Up Welding Tool");
    }
}