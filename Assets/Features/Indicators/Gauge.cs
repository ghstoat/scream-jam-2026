using UnityEngine;

public class Gauge : MonoBehaviour
{
    [SerializeField] GameObject needle;

    [SerializeField] float maxAngle;

    private float targetAngle;
    private float angle;
    [SerializeField] float needleSpeed = 200;

    public void SetTargetAngleFromRatio(float ratio)
    {
        float angleRatio = Mathf.Clamp01(ratio);
        targetAngle = angleRatio * maxAngle;


    }

    private void Update()
    {
        angle = Mathf.MoveTowards(angle, targetAngle, needleSpeed * Time.deltaTime);

        needle.transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
    }

}
