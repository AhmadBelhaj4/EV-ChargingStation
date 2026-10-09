using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        CarEntry,
        PlayerFreeRoam,
        Dialogue,
        Complete
    }

    public GameState currentState;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentState = GameState.CarEntry;
        Debug.Log("GameManager started");
    }
}