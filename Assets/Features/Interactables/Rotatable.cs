using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class Gear
{
    public Transform target;
    public Transform axis;
    public float rotationMult;
}


public class Rotatable : Interactable
{
    [SerializeField] protected Transform rotationTarget;
    [SerializeField] protected Transform axisReference;

    [SerializeField]  protected Gear[] gears;


    protected Camera targetCamera;
    protected Plane dragPlane;
    protected Vector2 previousDirection;

    protected float rotationDelta;
    public float RotationDelta => rotationDelta;
    protected override void Init()
    {
        base.Init();
        targetCamera = Camera.main;

        if (rotationTarget == null) rotationTarget = transform;
    }

    public override void OnDragStart()
    {
        base.OnDragStart();

        dragPlane = new Plane(targetCamera.transform.forward, axisReference.position);
        previousDirection = GetMouseDirection(Mouse.current.position.ReadValue());
    }

    public override void OnDrag(Vector2 mousePosition)
    {
        base.OnDrag(mousePosition);

        Vector2 direction = GetMouseDirection(Mouse.current.position.ReadValue());
        float angle = Vector2.SignedAngle(previousDirection, direction);

        rotationDelta = Vector2.SignedAngle(previousDirection, direction);

        rotationTarget.RotateAround(axisReference.position, axisReference.forward, rotationDelta);

        if (gears != null)
        {
            foreach (Gear gear in gears)
            {
                if (gear == null || gear.target == null || gear.axis == null) continue;

                gear.target.RotateAround( gear.axis.position, gear.axis.forward, rotationDelta * gear.rotationMult);
            }
        }

        previousDirection = direction;
    }

    public override void OnDragExit()
    {
        base.OnDragExit();
        rotationDelta = 0;
    }

    protected Vector2 GetMouseDirection(Vector2 screenPosition)
    {
        Ray ray = targetCamera.ScreenPointToRay(screenPosition);
        if (!dragPlane.Raycast(ray, out float distance)) return previousDirection;

        Vector3 hitPoint = ray.GetPoint(distance);
        Vector3 offset = hitPoint - axisReference.position;

        return new Vector2(Vector3.Dot(offset, axisReference.right), Vector3.Dot(offset, axisReference.up)).normalized;
    }
}