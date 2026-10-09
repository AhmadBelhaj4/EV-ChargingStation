using UnityEngine;

public class M011AnimatorDriver : MonoBehaviour
{
    public Animator animator;

    [Header("Smoothing")]
    public float dampTime = 0.1f;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (animator == null) return;

        float horizontal = Input.GetAxis("Horizontal"); // A / D
        float vertical = Input.GetAxis("Vertical");     // W / S

        // Envoie les paramètres attendus par ton Blend Tree
        animator.SetFloat("Turn", horizontal, dampTime, Time.deltaTime);
        animator.SetFloat("Forward", vertical, dampTime, Time.deltaTime);
    }
}