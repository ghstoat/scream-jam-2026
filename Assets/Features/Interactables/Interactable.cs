using NaughtyAttributes;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private Renderer objectRenderer;

    private static readonly int GlowID = Shader.PropertyToID("_Glow");
    private bool isGlowing;

    public bool isHovering { private set; get; }
    public bool isDragging { private set; get; }

    public bool isInteractable { private set; get; }
    private const string INTERACTABLE = "Interactable";
    private const string BACKGROUND = "Background";

    private void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        SetInteractable(true);

    }

    public virtual void OnPointerEnter()
    {
        isHovering = true;
        SetGlow(true);

    }

    public virtual void OnPointerExit()
    {
        isHovering = false;
        SetGlow(false);
    }

    public virtual void OnDragStart()
    {
        isDragging = true;
    }

    public virtual void OnDragExit()
    {
        isDragging = false;

    }
    public virtual void OnDrag(Vector2 mousePosition)
    {
    }

    public virtual void SetInteractable(bool interactable)
    {
        isInteractable = interactable;


        if (interactable)
        {
            int targetLayer = LayerMask.NameToLayer(INTERACTABLE);
            gameObject.layer = targetLayer;
        }

        else
        {
            int targetLayer = LayerMask.NameToLayer(BACKGROUND);
            gameObject.layer = targetLayer;
        }
    }

    protected void SetGlow(bool enabled)
    {
        if (isGlowing == enabled || objectRenderer == null) return;

        isGlowing = enabled;
        objectRenderer.material.SetFloat(GlowID, enabled ? 1f : 0f);
    }

    [Button]
    private void AssignRenderer()
    {
        objectRenderer = GetComponent<Renderer>();
    }
}
