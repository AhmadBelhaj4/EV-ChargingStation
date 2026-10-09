using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weston.SampleWelding
{
    public class MouseDraw : MonoBehaviour
    {
        public Transform cam;
        public LayerMask terrainMask;
        Camera camCmp;
        public float rayRadius = 0.1f;
        public float radius = 0.02f;
        public float hardness = 1f;
        public float speed = -28;
        bool hasHit;
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
                Paintable p = hit.transform.GetComponent<Paintable>();
                // Writable p = hit.transform.GetComponent<Writable>();

                Debug.DrawLine(cam.position, hitPoint, Color.green);

                if (Input.GetMouseButton(0))
                {
                    PaintManager.instance.paint(p, hitPoint, radius, hardness, strength, speed, paintColor);
                    // WritingManager.Instance.paint(p, hitPoint, radius, hardness, strength, speed, paintColor);

                }

            }
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

