using UnityEngine;

public class CarDriveButtons : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 80f;

    [Header("Smooth Settings")]
    public float acceleration = 3f;
    public float deceleration = 5f;

    private float moveInput = 0f;
    private float turnInput = 0f;

    private float currentMove = 0f;
    private float currentTurn = 0f;

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        // Smooth acceleration
        currentMove = Mathf.Lerp(currentMove, moveInput, Time.deltaTime * acceleration);
        currentTurn = Mathf.Lerp(currentTurn, turnInput, Time.deltaTime * acceleration);

        // Move forward/back
        transform.Translate(Vector3.forward * currentMove * moveSpeed * Time.deltaTime);

        // Rotate left/right
        transform.Rotate(Vector3.up * currentTurn * turnSpeed * Time.deltaTime);
    }

    // =========================
    // 🎮 BUTTON CONTROLS
    // =========================

    public void ForwardDown()
    {
        moveInput = 1f;
    }

    public void BackDown()
    {
        moveInput = -1f;
    }

    public void LeftDown()
    {
        turnInput = -1f;
    }

    public void RightDown()
    {
        turnInput = 1f;
    }

    public void StopMove()
    {
        moveInput = 0f;
    }

    public void StopTurn()
    {
        turnInput = 0f;
    }

    public void StopAll()
    {
        moveInput = 0f;
        turnInput = 0f;
    }
}