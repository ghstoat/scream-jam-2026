using UnityEngine;
using UnityEngine.InputSystem;

public class Dial : Rotatable
{
    [SerializeField] private float minAngle = -135f;
    [SerializeField] private float maxAngle = 135f;

    public float Value { get; private set; }

    private float currentAngle;
    private Quaternion initialRotation;

    protected override void Init()
    {
        base.Init();

        initialRotation = rotationTarget.rotation;
        currentAngle = 0f;
        UpdateValue();
    }

    public override void OnDrag(Vector2 mousePosition)
    {
        Vector2 direction = GetMouseDirection(Mouse.current.position.ReadValue());
        rotationDelta = Vector2.SignedAngle(previousDirection, direction);

        float nextAngle = Mathf.Clamp(currentAngle + rotationDelta, minAngle, maxAngle);
        rotationDelta = nextAngle - currentAngle;
        currentAngle = nextAngle;

        rotationTarget.RotateAround(axisReference.position, axisReference.forward, rotationDelta);

        previousDirection = direction;
        UpdateValue();
    }

    private void UpdateValue()
    {
        Value = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);
    }
}