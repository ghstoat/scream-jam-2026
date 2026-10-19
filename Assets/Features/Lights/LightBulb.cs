
using UnityEngine;
using System.Collections;

public enum LightStates
{
    Off,
    On,
    Blinking,
    Strobing
}

public class LightBulb : MonoBehaviour
{
    public LightStates state { get; private set; } = LightStates.On;

    [Header("Light References")]
    [SerializeField] private Light pointLight;
    [SerializeField] private Renderer bulbRenderer;

    [Header("Blink Settings")]
    [SerializeField] private float blinkInterval = 0.5f;
    [SerializeField] private float strobeInterval = 0.1f;

    [Header("Bulb Appearance")]
    [SerializeField] Texture2D bulbOnTex;
    [SerializeField] Texture2D bulbOffTex;

    [SerializeField] private Color defaultColor = Color.orange;
    private Color emissionColor;

    [SerializeField] private float emissionIntensity = 2f;

    private Material bulbMat;
    private Coroutine blinkCoroutine;

    private static readonly int BaseMapID = Shader.PropertyToID("_BaseMap");
    private static readonly int EmissionColorID = Shader.PropertyToID("_Emission");

    private void Awake()
    {
        bulbMat = bulbRenderer.material;
        emissionColor = defaultColor;   
    }

    private void Start()
    {
        SetState(LightStates.Off);
    }

    public void SetColor(Color color)
    {
        emissionColor = color;
        if (state == LightStates.On)
        {
            Color emission = emissionColor * emissionIntensity;
            bulbMat.SetColor(EmissionColorID, emission);
            pointLight.color = color;
        }

    }

    public void SetState(LightStates newState)
    {
        if (state == newState && blinkCoroutine != null) return;

        StopBlinking();

        state = newState;

        switch (state)
        {
            case LightStates.Off:
                ApplyLight(false);
                break;

            case LightStates.On:
                ApplyLight(true);
                break;

            case LightStates.Blinking:
                blinkCoroutine = StartCoroutine(BlinkRoutine());
                break;

            case LightStates.Strobing:
                blinkCoroutine = StartCoroutine(BlinkRoutine());
                break;
        }
    }

    private void ApplyLight(bool isOn)
    {
        pointLight.enabled = isOn;

        Texture2D tex = isOn? bulbOnTex : bulbOffTex;
        Color emission = isOn? emissionColor * emissionIntensity : Color.black;
            
        pointLight.color = emissionColor;

        bulbMat.SetColor(EmissionColorID, Color.black);
        
        bulbMat.SetTexture(BaseMapID, tex);

    }

    private IEnumerator BlinkRoutine()
    {
        bool isOn = false;

        while (true)
        {
            isOn = !isOn;
            ApplyLight(isOn);

            float interval = blinkInterval;
            if (state == LightStates.Strobing) interval = strobeInterval;
            

            yield return new WaitForSeconds(interval);
        }
    }

    private void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }

    private void OnDisable()
    {
        StopBlinking();
    }
}
