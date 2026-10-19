using UnityEngine;

public class Sway : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    public Axis swayAxis = Axis.Z;
    public float swayAngle = 5f;
    public float swaySpeed = .5f;

    private void Update()
    {
        float angle = Mathf.Sin(Time.time * swaySpeed) * swayAngle;

        Vector3 rotation = Vector3.zero;

        switch (swayAxis)
        {
            case Axis.X:
                rotation.x = angle;
                break;
            case Axis.Y:
                rotation.y = angle;
                break;
            case Axis.Z:
                rotation.z = angle;
                break;
        }

        transform.localRotation = Quaternion.Euler(rotation);
    }
}