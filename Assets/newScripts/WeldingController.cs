using UnityEngine;

public class WeldingController : MonoBehaviour
{

    private Material matInstance;
    [Header("References")]
    [SerializeField] private WeldingSparks weldingSparks;
    [SerializeField] private Camera mainCamera;

    [Header("Weld Shader Property (optional)")]
    [SerializeField] private MeshRenderer weldPlane;
    [SerializeField] private string weldProgressProperty = "_WeldProgress"; // your shader graph property name

    [Range(0f, 1f)]
    private float weldProgress = 0f;
    private bool isWelding = false;
    [SerializeField] private float weldSpeed = 0.3f;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        // This creates a unique instance so SetFloat actually works
    var mat = weldPlane.material; // Unity auto-instances on first access
    mat.SetFloat("_WeldProgress", 0f);
    matInstance = weldPlane.material;
    }

    
    
    
   void Update()
{
    if (Input.GetMouseButton(0))
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.name == "hole1")
            {
                if (Input.GetMouseButtonDown(0) || !isWelding)
                {
                    weldingSparks.EmitAt(hit.point, hit.normal);
                    isWelding = true;
                }

                weldProgress = Mathf.Clamp01(weldProgress + weldSpeed * Time.deltaTime);
                matInstance.SetFloat(weldProgressProperty, weldProgress);

                if (Time.frameCount % 8 == 0)
                    weldingSparks.EmitAt(hit.point, hit.normal);
            }
        }
    }

    if (Input.GetMouseButtonUp(0))
    {
        isWelding = false;
    }
}
    
    
    
    
    
    
    
    
    
    
    /*void Update()
    {
        // Start welding on mouse hold
        if (Input.GetMouseButton(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == weldPlane.gameObject)
                {
                    // Trigger sparks on first click
                    if (Input.GetMouseButtonDown(0) || !isWelding)
                    {
                        weldingSparks.EmitAt(hit.point, hit.normal);
                        isWelding = true;
                    }

                    // Advance weld progress on the shader
                    weldProgress = Mathf.Clamp01(weldProgress + weldSpeed * Time.deltaTime);
                    weldPlane.material.SetFloat(weldProgressProperty, weldProgress);

                    // Keep emitting sparks while holding
                    if (Time.frameCount % 8 == 0) // every ~8 frames
                        weldingSparks.EmitAt(hit.point, hit.normal);
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isWelding = false;
        }
    }*/

    // Call this to reset the weld
    public void ResetWeld()
    {
        weldProgress = 0f;
        weldPlane.material.SetFloat(weldProgressProperty, 0f);
    }
}