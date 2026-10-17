
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HangingWire : MonoBehaviour
{
    [Header("Endpoints")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Wire Settings")]
    [SerializeField, Min(0.01f)] private float wireLength = 5f;
    [SerializeField, Min(2)] private int segments = 30;

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
    }

    private void LateUpdate()
    {
        UpdateWire();
    }

    public void SetEndpoints(Transform a, Transform b)
    {
        pointA = a;
        pointB = b;
        UpdateWire();
    }

    private void UpdateWire()
    {
        if (pointA == null || pointB == null)
        {
            line.positionCount = 0;
            return;
        }

        Vector3 start = pointA.position;
        Vector3 end = pointB.position;

        float distance = Vector3.Distance(start, end);

        // A wire cannot span farther than its own length.
        float effectiveLength = Mathf.Max(wireLength, distance);

        // Find sag by binary searching for a curve with
        // approximately the desired arc length.
        float low = 0f;
        float high = effectiveLength;

        for (int iteration = 0; iteration < 24; iteration++)
        {
            float mid = (low + high) * 0.5f;
            float length = CalculateLength(start, end, mid);

            if (length < effectiveLength)
                low = mid;
            else
                high = mid;
        }

        float sag = (low + high) * 0.5f;

        line.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);

            Vector3 position = Vector3.Lerp(start, end, t);
            position.y -= 4f * sag * t * (1f - t);

            line.SetPosition(i, position);
        }
    }

    private float CalculateLength(Vector3 start, Vector3 end, float sag)
    {
        float length = 0f;
        Vector3 previous = start;

        for (int i = 1; i < 100; i++)
        {
            float t = i / 99f;

            Vector3 position = Vector3.Lerp(start, end, t);
            position.y -= 4f * sag * t * (1f - t);

            length += Vector3.Distance(previous, position);
            previous = position;
        }

        return length;
    }
}
