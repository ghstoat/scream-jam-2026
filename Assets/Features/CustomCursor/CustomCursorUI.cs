
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomCursorUI : MonoBehaviour
{
    [SerializeField] private RectTransform cursorRect;
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    private void Awake()
    {
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        Cursor.visible = false;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        cursorRect.position = mousePosition - hotspot;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        Cursor.visible = false;
    }
}
