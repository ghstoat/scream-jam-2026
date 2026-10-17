
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public enum CamControl
{
    Soft,
    Hard,
    Manual
}


public class PlayerController : MonoBehaviour
{
    [Header("Settings")]

    [SerializeField] private CamControl camControl;

    [SerializeField] private float sensitivity;
    [SerializeField] private float softCamDeadZone;

    [SerializeField] private float softCamSenMult;
    [SerializeField] private float hardCamSenMult;
    [SerializeField] private float manualCamSenMult;

    [Header("Cursor Zoom")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float zoomFOV;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float zoomFocusDistance;

    private float normalFOV;

    [Header("Horizontal Constraints")]
    [SerializeField] private float minHorizontalAngle;
    [SerializeField] private float maxHorizontalAngle;

    [Header("Vertical Constraints")]
    [SerializeField] private float minVerticalAngle;
    [SerializeField] private float maxVerticalAngle;

    private float horizontalRotation;
    private float verticalRotation;

    private const Key freeCursorControl = Key.LeftAlt;

    private void Start()
    {
        Vector3 rotation = transform.localEulerAngles;

        horizontalRotation = rotation.y;
        if (horizontalRotation > 180f) horizontalRotation -= 360f;

        verticalRotation = rotation.x;
        if (verticalRotation > 180f) verticalRotation -= 360f;

        normalFOV = playerCamera.fieldOfView;
    }

    private void Update()
    {
        switch (camControl)
        {
            case CamControl.Soft:
                HandleSoftCam(); break;
            case CamControl.Hard:
                HandleHardCam(); break;
            case CamControl.Manual:
                HandleManualCam(); break;
        }
        HandleZoom();
    }
    private void HandleZoom()
    {
        float targetFOV = Mouse.current.rightButton.isPressed ? zoomFOV : normalFOV;

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView,targetFOV,zoomSpeed * Time.deltaTime);
    }

    private void HandleSoftCam()
    {
        if (Keyboard.current != null && Keyboard.current[freeCursorControl].isPressed)
        {
            SetCursorState(CursorLockMode.None);
            return;
        }

        SetCursorState(CursorLockMode.Confined);

        float softCamSen = sensitivity * softCamSenMult;


        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 cursorDirection = (mousePosition - screenCenter) / screenCenter;


        if (mousePosition.x < 0f || mousePosition.x > Screen.width || mousePosition.y < 0f || mousePosition.y > Screen.height) return; //stop moving if out of screen bounds
       

        if (Mathf.Abs(cursorDirection.x) < softCamDeadZone) cursorDirection.x = 0f;

        if (Mathf.Abs(cursorDirection.y) < softCamDeadZone) cursorDirection.y = 0f;

        horizontalRotation += cursorDirection.x * softCamSen * Time.deltaTime;
        horizontalRotation = Mathf.Clamp(horizontalRotation, minHorizontalAngle, maxHorizontalAngle);

        verticalRotation -= cursorDirection.y * softCamSen * Time.deltaTime;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }


    private void HandleHardCam()
    {

        if (Keyboard.current != null && Keyboard.current[freeCursorControl].isPressed)
        {
            SetCursorState(CursorLockMode.None);
            return;
        }

        SetCursorState(CursorLockMode.Locked);

        float hardCamSen = sensitivity * hardCamSenMult;

        float mouseX = Mouse.current.delta.ReadValue().x * hardCamSen * Time.deltaTime;
        float mouseY = Mouse.current.delta.ReadValue().y * hardCamSen * Time.deltaTime;

        horizontalRotation += mouseX;
        horizontalRotation = Mathf.Clamp(horizontalRotation, minHorizontalAngle, maxHorizontalAngle);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation,0f);
    }


    private void HandleManualCam()
    {
        SetCursorState(CursorLockMode.None);

        float manualCamSen = sensitivity * manualCamSenMult;

        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) horizontalInput -= 1f;

            if (Keyboard.current.dKey.isPressed) horizontalInput += 1f;

            if (Keyboard.current.sKey.isPressed) verticalInput += 1f;

            if (Keyboard.current.wKey.isPressed) verticalInput -= 1f;
        }

        horizontalRotation += horizontalInput * manualCamSen * Time.deltaTime;
        verticalRotation += verticalInput * manualCamSen * Time.deltaTime;

        horizontalRotation = Mathf.Clamp(horizontalRotation, minHorizontalAngle, maxHorizontalAngle);

        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }

    private void SetCursorState(CursorLockMode lockMode)
    {
        if (Cursor.lockState != lockMode) Cursor.lockState = lockMode;
    }
}
