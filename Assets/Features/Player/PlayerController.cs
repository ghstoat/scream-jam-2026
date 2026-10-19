
using System.Collections;
using System.Threading;
using UnityEditor;
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
    [SerializeField] private GameObject camParent;

    private bool locked = false;

    [Header("Settings")]

    public CamControl camControl { get; private set; } = CamControl.Soft;

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

    [Header("UI Control")]
    [SerializeField] GameObject wasdLookControl;
    [SerializeField] GameObject altFreeControl;

    [Header("Horizontal Constraints")]
    [SerializeField] private float minHorizontalAngle;
    [SerializeField] private float maxHorizontalAngle;

    [Header("Vertical Constraints")]
    [SerializeField] private float minVerticalAngle;
    [SerializeField] private float maxVerticalAngle;

    private float horizontalRotation;
    private float verticalRotation;

    private const Key freeCursorControl = Key.LeftAlt;
    public const string CAMCONTROL = "CamControl";

    public static PlayerController Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"!!! Multiple {this.name} found in scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;


        int savedCamControl = PlayerPrefs.GetInt(CAMCONTROL, 0);
        camControl = (CamControl)savedCamControl;
    }


    private void Start()
    {
        Vector3 rotation = camParent.transform.localEulerAngles;

        horizontalRotation = rotation.y;
        if (horizontalRotation > 180f) horizontalRotation -= 360f;

        verticalRotation = rotation.x;
        if (verticalRotation > 180f) verticalRotation -= 360f;

        normalFOV = playerCamera.fieldOfView;

        locked = false;
    }

    private void Update()
    {
        if (PauseManager.Instance.paused) return;
        if (NoteManager.Instance.open) return;
        if (TutorialManager.Instance.ShowingTutorial) return;
        if (locked) return;

        UpdateControlUI();

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

    public void Lock(bool locked)
    {
        this.locked = locked;
    }

    public void SetControlType(CamControl control)
    {
        camControl = control;
        PlayerPrefs.SetInt(CAMCONTROL, (int)camControl);
        PlayerPrefs.Save();
    }

    private void UpdateControlUI()
    {
        if (camControl == CamControl.Soft || camControl == CamControl.Hard)
        {
            altFreeControl.SetActive(true);
            wasdLookControl.SetActive(false);
        }
        else if (camControl == CamControl.Manual)
        {
            altFreeControl.SetActive(false);
            wasdLookControl.SetActive(true);
        }
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

        camParent.transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
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

        camParent.transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation,0f);
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

        camParent.transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }

    private void SetCursorState(CursorLockMode lockMode)
    {
        if (Cursor.lockState != lockMode) Cursor.lockState = lockMode;
    }

    public void LookAtTransform(Transform target, float duration = 0f, float pause = 0f)
    {
        if (target == null) return;
        Lock(true);
        NoteManager.Instance.CloseNote();

        Transform camTransform = camParent.transform;

        // Direction from the camera to the target in world space.
        Vector3 direction = target.position - camTransform.position;

        if (direction.sqrMagnitude < 0.0001f) return;

        // Convert direction into the camera parent's local space.
        Transform parent = camTransform.parent;

        Vector3 localDirection = parent != null
            ? parent.InverseTransformDirection(direction)
            : direction;

        // Calculate the required local camera rotation.
        Quaternion targetRotation = Quaternion.LookRotation(localDirection);
        Vector3 angles = targetRotation.eulerAngles;

        float targetHorizontal = angles.y;
        float targetVertical = angles.x;

        if (targetHorizontal > 180f) targetHorizontal -= 360f;
        if (targetVertical > 180f) targetVertical -= 360f;

        // Temporarily ignore camera angle limits for the jumpscare.
        if (duration <= 0f)
        {
            horizontalRotation = targetHorizontal;
            verticalRotation = targetVertical;

            camTransform.localRotation = Quaternion.Euler(
                verticalRotation, horizontalRotation, 0f);

            return;
        }

        StopCoroutineIfRotating();
        rotationCoroutine = StartCoroutine(
            RotateToTarget(targetHorizontal, targetVertical, duration, pause));
    }

    private Coroutine rotationCoroutine;

    private void StopCoroutineIfRotating()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }
    }

    private IEnumerator RotateToTarget(
        float targetHorizontal,
        float targetVertical,
        float duration,
        float pause)
    {
        float startHorizontal = horizontalRotation;
        float startVertical = verticalRotation;
        float elapsed = 0f;


        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Smooth acceleration and deceleration.
            t = t * t * (3f - 2f * t);

            horizontalRotation = Mathf.LerpAngle(
                startHorizontal, targetHorizontal, t);

            verticalRotation = Mathf.Lerp(
                startVertical, targetVertical, t);

            camParent.transform.localRotation = Quaternion.Euler(
                verticalRotation, horizontalRotation, 0f);

            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, zoomSpeed * Time.deltaTime);

            yield return null;
        }

        horizontalRotation = targetHorizontal;
        verticalRotation = targetVertical;

        camParent.transform.localRotation = Quaternion.Euler(
            verticalRotation, horizontalRotation, 0f);

        yield return new WaitForSeconds(pause); 

        Lock(false);

        rotationCoroutine = null;
    }
}
