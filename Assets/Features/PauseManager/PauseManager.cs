using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{

    public bool paused { get; private set; } = false;
    private const Key pauseControl = Key.Tab;

    [SerializeField] GameObject pauseScreen;

    [SerializeField] TextMeshProUGUI controlTypeButton;

    public static PauseManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"!!! Multiple {this.name} found in scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }

    private void Start()
    {
        UpdateControlTypeButtonText(PlayerController.Instance.camControl.ToString());
    }

    private void Update()
    {
        if (Keyboard.current[pauseControl].wasPressedThisFrame && !TutorialManager.Instance.ShowingTutorial)
        {
            TogglePause();
            return;
        }
    }

    public void TogglePause()
    {
        paused = !paused;   

        TimeManager.Instance.PauseTime(paused);
        pauseScreen.SetActive(paused);

        if (paused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }
    }

    public void UpdateControlTypeButtonText(string text)
    {
        controlTypeButton.text = $"CAMERA TYPE : {text}";
    }

    public void CycleCamControl()
    {
        CamControl currentControl = PlayerController.Instance.camControl;
        
        switch (currentControl)
        {
            case CamControl.Soft:
                currentControl = CamControl.Hard; break;
            case CamControl.Hard:
                currentControl = CamControl.Manual; break;
            case CamControl.Manual:
                currentControl = CamControl.Soft; break;
        }

        PlayerController.Instance.SetControlType(currentControl);
        UpdateControlTypeButtonText(currentControl.ToString());
    }

    public void ToggleSettings()
    {
        Debug.LogWarning("No settings functionality");
    }

    public void Quit()
    {
        Debug.LogWarning("No quit functionality");
    }
}
