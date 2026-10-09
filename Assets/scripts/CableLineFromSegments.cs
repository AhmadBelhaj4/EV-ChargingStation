using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CableLineFromSegments : MonoBehaviour
{
    public Transform startPoint;
    public Transform[] cablePoints;
    public Transform endPoint;

    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (startPoint == null || endPoint == null) return;

        int count = 2;
        if (cablePoints != null) count += cablePoints.Length;

        lr.positionCount = count;

        int index = 0;
        lr.SetPosition(index++, startPoint.position);

        if (cablePoints != null)
        {
            for (int i = 0; i < cablePoints.Length; i++)
            {
                if (cablePoints[i] != null)
                    lr.SetPosition(index++, cablePoints[i].position);
            }
        }

        lr.SetPosition(index, endPoint.position);
    }
}