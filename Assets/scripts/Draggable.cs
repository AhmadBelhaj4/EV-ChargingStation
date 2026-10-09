using UnityEngine;

public class Draggable : MonoBehaviour
{
    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color heldColor   = new Color(1f, 1f, 0.5f, 1f);

    [Header("Grid")]
    // How high above the table surface this pipe sits when dragged
    // (accounts for the pipe's own height/pivot offset)
    public float heightAboveTable = 0.25f;

    Rigidbody rb;
    Renderer rend;
    MaterialPropertyBlock propBlock;

    void Awake()
    {
        rb        = GetComponent<Rigidbody>();
        rend      = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    public void OnPickUp()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity    = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        SetColor(heldColor);
    }

    public void OnRelease()
    {
        if (rb != null)
            rb.isKinematic = false;

        SetColor(normalColor);
    }

    // Uses a MaterialPropertyBlock so pipes can share a material
    // and still have individual colour tints
    void SetColor(Color color)
    {
        if (rend == null) return;
        rend.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", color);
        rend.SetPropertyBlock(propBlock);
    }
}