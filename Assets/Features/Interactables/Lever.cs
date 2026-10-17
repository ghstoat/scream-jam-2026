using UnityEngine;
using UnityEngine.InputSystem;

public class Lever : Interactable
{
    [Header("Pull Path")]
    [SerializeField] private Transform pullStart;
    [SerializeField] private Transform pullEnd;

    protected Camera targetCamera;
    protected Plane dragPlane;
    protected Vector2 virtualMousePosition;

    protected float currentPull;
    protected float dragStartPull;
    protected float dragStartPathPosition;
    protected bool dragInitialized;

    public float CurrentPull => currentPull;

    protected override void Init()
    {
        base.Init();
        targetCamera = Camera.main;
        transform.position = pullStart.position;
    }

    public override void OnDragStart()
    {
        base.OnDragStart();

        if (!isInteractable || Mouse.current == null) return;

        dragPlane = new Plane(targetCamera.transform.forward, pullStart.position);
        virtualMousePosition = Mouse.current.position.ReadValue();

        if (!TryGetPathPosition(virtualMousePosition, out dragStartPathPosition)) return;

        dragStartPull = currentPull;
        dragInitialized = true;
    }

    public override void OnDrag(Vector2 mousePosition)
    {
        base.OnDrag(mousePosition);

        if (!dragInitialized || !isInteractable) return;

        virtualMousePosition += Mouse.current.delta.ReadValue();

        if (!TryGetPathPosition(virtualMousePosition, out float pathPosition)) return;

        currentPull = Mathf.Clamp01(dragStartPull + pathPosition - dragStartPathPosition);
        UpdateLeverPosition();
    }

    protected bool TryGetPathPosition(Vector2 screenPosition, out float pathPosition)
    {
        Ray ray = targetCamera.ScreenPointToRay(screenPosition);

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 path = pullEnd.position - pullStart.position;

            if (path.sqrMagnitude < 0.000001f)
            {
                pathPosition = 0f;
                return false;
            }

            pathPosition = Vector3.Dot(hitPoint - pullStart.position, path) / path.sqrMagnitude;
            return true;
        }

        pathPosition = 0f;
        return false;
    }

    protected void UpdateLeverPosition()
    {
        transform.position = Vector3.Lerp(pullStart.position, pullEnd.position, currentPull);
    }

    public override void OnDragExit()
    {
        base.OnDragExit();
        dragInitialized = false;
    }
}
