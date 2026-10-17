
using UnityEngine;
using System.Collections;

public enum LightStates
{
    Off,
    On,
    Blinking
}

public class LightBulb : MonoBehaviour
{
    public LightStates state { get; private set; } = LightStates.On;

    [Header("Light References")]
    [SerializeField] private Light pointLight;
    [SerializeField] private Renderer bulbRenderer;

    [Header("Blink Settings")]
    [SerializeField] private float blinkInterval = 0.5f;

    [Header("Bulb Appearance")]
    [SerializeField] Texture2D bulbOnTex;
    [SerializeField] Texture2D bulbOffTex;

    [SerializeField] private Color defaultColor = Color.orange;
    [SerializeField] private Color emissionColor = Color.yellow;

    [SerializeField] private float emissionIntensity = 2f;

    private Material bulbMat;
    private Coroutine blinkCoroutine;

    private static readonly int BaseMapID = Shader.PropertyToID("_BaseMap");
    private static readonly int EmissionColorID = Shader.PropertyToID("_Emission");

    private void Awake()
    {
        bulbMat = bulbRenderer.material;
    }

    private void Start()
    {
        SetState(LightStates.Off);
    }

    public void SetColor(Color color)
    {
        emissionColor = color;
        if (state != LightStates.Off)
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
        }
    }

    private void ApplyLight(bool isOn)
    {
        pointLight.enabled = isOn;

        Texture2D tex = isOn? bulbOnTex : bulbOffTex;
        Color emission = isOn? emissionColor * emissionIntensity : Color.black;
            
        pointLight.color = emissionColor;
        bulbMat.SetColor(EmissionColorID, emission);
        bulbMat.SetTexture(BaseMapID, tex);
    }

    private IEnumerator BlinkRoutine()
    {
        bool isOn = false;

        while (true)
        {
            isOn = !isOn;
            ApplyLight(isOn);

            yield return new WaitForSeconds(blinkInterval);
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
