using UnityEngine;

namespace Weston.SampleWelding
{
    public class TriangleDrawer : MonoBehaviour
    {
        public Transform cam;
        public LayerMask terrainMask;
        Camera camCmp;
        public float rayRadius = 0.1f;
        public float radius = 0.02f;
        public float hardness = 1f;
        public float speed = -28;
        bool hasHit;
        Vector3 lastHitPoint;
        Vector3 hitPoint;
        public float strength = 1;
        public Color paintColor = Color.black;
        // Start is called before the first frame update
        void Start()
        {
            camCmp = cam.GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update()
        {
            hasHit = false;
            RaycastHit hit;
            Ray ray = camCmp.ScreenPointToRay(Input.mousePosition);
            if (Physics.SphereCast(ray.origin, rayRadius, ray.direction, out hit, 1000, terrainMask))
            {

                hasHit = true;
                hitPoint = hit.point;
                // Paintable p = hit.transform.GetComponent<Paintable>();
                Writable writable = hit.transform.GetComponent<Writable>();

                Debug.DrawLine(cam.position, hitPoint, Color.green);

                if (Input.GetMouseButton(0))
                {
                    if(lastHitPoint==null){
                        lastHitPoint = hitPoint;
                    }
                    else{
                        // Vector3 direction = (hitPoint-lastHitPoint).normalized;
                        // Vector3 writableForward = writable.transform.forward;
                        
                        
                        // //生成三角形
                        // Vector3 left = Vector3.Cross(direction, hit.normal).normalized;
                        // PaintTwoTriangle(writable,lastHitPoint,hitPoint,left);
                        // lastHitPoint = hitPoint;
                        Vector3 p1 =  new Vector3(-0.11f, -0.17f, 0.00f);
                        Vector3 p2 =  new Vector3(0.11f, 0.17f, 0.00f);
                        Vector3 p3 =  new Vector3(-1.50f, 0.70f, 0.00f);
                        WritingManager.Instance.paint(writable, p1,p2,p3);
                    }
                    // WritingManager.Instance.paint(writable, hitPoint, radius, hardness, strength, speed, paintColor);

                }

            }
        }

        void PaintTwoTriangle(Writable writable,Vector3 lastHitPoint,Vector3 hitPoint,Vector3 perpendicular){
            Vector3 p1 = lastHitPoint + perpendicular* radius;
            Vector3 p2 = lastHitPoint - perpendicular* radius;
            Vector3 p3 = hitPoint + perpendicular* radius;
            Vector3 p4 = hitPoint - perpendicular* radius;

            Debug.Log(p1+":"+p2+":"+p3);

            WritingManager.Instance.paint(writable, p1,p2,p3);
            WritingManager.Instance.paint(writable, p3,p4,p2);
            
        }

        void OnDrawGizmos()
        {
            if (hasHit)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(hitPoint, 0.25f);
            }
        }
    }
}
