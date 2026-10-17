using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableManager : MonoBehaviour
{
    public static InteractableManager Instance;

    private Camera targetCamera;
    [SerializeField] private LayerMask hoverLayers;
    [SerializeField] private float maxDragDistance = 3f;

    private Interactable hoveredInteractable;
    private Interactable draggedInteractable;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"Multiple InteractableManagers in scene: {name}", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;

        
        targetCamera = Camera.main;
    }

    private void Update()
    {
        if (PauseManager.Instance.paused) return;

        if (draggedInteractable != null && (!IsOnHoverLayer(draggedInteractable) || IsCursorTooFar(draggedInteractable)))
        {
            draggedInteractable.OnDragExit();

            draggedInteractable = null;
            SetHovered(null);
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = targetCamera.ScreenPointToRay(mousePosition);

        Interactable nextInteractable = null;

        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, hoverLayers);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            Interactable candidate = hit.collider.GetComponentInParent<Interactable>();

            if (candidate != null && IsOpaquePixel(hit))
            {
                nextInteractable = candidate;
                break;
            }
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && hoveredInteractable != null)
        {
            draggedInteractable = hoveredInteractable;
            draggedInteractable.OnDragStart();
        }

        Interactable hoverTarget = draggedInteractable != null ? draggedInteractable : nextInteractable;

        if (hoverTarget != hoveredInteractable)
        {
            SetHovered(hoverTarget);
        }

        if (draggedInteractable != null && Mouse.current.leftButton.isPressed)
        {
            draggedInteractable.OnDrag(mousePosition);
        }

        if (draggedInteractable != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            draggedInteractable.OnDragExit();

            draggedInteractable = null;

            SetHovered(nextInteractable);
        }
    }

    private bool IsOpaquePixel(RaycastHit hit)
    {
        Renderer hitRenderer = hit.collider.GetComponent<Renderer>();

        if (hitRenderer == null) hitRenderer = hit.collider.GetComponentInParent<Renderer>();

        if (hitRenderer == null) return true;

        Texture texture = hitRenderer.sharedMaterial.mainTexture;

        if (texture is not Texture2D texture2D) return true;

        try
        {
            return texture2D.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y).a > 0f;
        }
        catch (UnityException)
        {
            Debug.LogWarning( $"Texture '{texture2D.name}' must have Read/Write enabled.", texture2D);
            return true;
        }
    }

    private void SetHovered(Interactable next)
    {
        if (hoveredInteractable != null)
        {
            hoveredInteractable.OnPointerExit();
        }

        hoveredInteractable = next;

        if (hoveredInteractable != null)
        {
            hoveredInteractable.OnPointerEnter();
        }
    }
    private bool IsOnHoverLayer(Interactable interactable)
    {
        if (interactable == null) return false;

        int layer = interactable.gameObject.layer;

        return (hoverLayers.value & (1 << layer)) != 0;
    }

    private bool IsCursorTooFar(Interactable interactable)
    {
        Collider col = interactable.GetComponentInChildren<Collider>();

        if (col == null) return false;

        Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        Vector3 closestPoint = col.bounds.ClosestPoint(ray.origin + ray.direction * 10f);

        float distance = Vector3.Distance(
            closestPoint,
            ray.origin + ray.direction * 10f
        );

        return distance > maxDragDistance;
    }
}