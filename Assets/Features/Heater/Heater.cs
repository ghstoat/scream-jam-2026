using UnityEngine;

public class Heater : MonoBehaviour
{
    [SerializeField] float minTemp;
    [SerializeField] float maxTemp;

    [SerializeField] FillBar extTempBar;
    [SerializeField] FillBar heaterTempBar;
    [SerializeField] FillBar intTempBar;

    [SerializeField] LightBulb overheatingLight;
    [SerializeField] LightBulb livableLight;
    [SerializeField] LightBulb freezingLight;

    [SerializeField] Renderer coil;
    [SerializeField] float maxCoilIntensity = 10;

    [SerializeField] FillBar powerBar;
    [SerializeField] LightBulb powerLight;

    private void Start()
    {
        overheatingLight.SetColor(Color.red);
        freezingLight.SetColor(Color.red);

    }

    private void Update()
    {
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        extTempBar.SetTargetScaleYFromRatio(GetRatioFromTemp(TemperatureManager.Instance.extTemp));
        heaterTempBar.SetTargetScaleYFromRatio(GetRatioFromTemp(TemperatureManager.Instance.heaterTemp));
        intTempBar.SetTargetScaleYFromRatio(GetRatioFromTemp(TemperatureManager.Instance.intTemp));

        if (TemperatureManager.Instance.Overheating) overheatingLight.SetState(LightStates.Blinking);
        else overheatingLight.SetState(LightStates.Off);

        if (TemperatureManager.Instance.Livable) livableLight.SetState(LightStates.On);
        else livableLight.SetState(LightStates.Off);

        if (TemperatureManager.Instance.Freezing) freezingLight.SetState(LightStates.Blinking);
        else freezingLight.SetState(LightStates.Off);

        powerBar.SetTargetScaleYFromRatio(TemperatureManager.Instance.PowerRatio);
        if (TemperatureManager.Instance.PowerEmpty) powerLight.SetState(LightStates.Off);
        else powerLight.SetState(LightStates.On);


        float intensity = maxCoilIntensity * TemperatureManager.Instance.PowerRatio;
        coil.material.SetColor("_Color", Color.white * intensity);
    }

    private float GetRatioFromTemp(float temp)
    {
        return Mathf.Clamp01((temp - minTemp) / (maxTemp - minTemp));
    }
}
