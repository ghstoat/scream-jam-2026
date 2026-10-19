using UnityEngine;

public class OxygenMeter : MonoBehaviour
{
    [SerializeField] FillBar powerBar;
    [SerializeField] Gauge oxygenGauge;

    [SerializeField] LightBulb generatingOxygenLight;
    [SerializeField] LightBulb highOxygenLight;
    [SerializeField] LightBulb lowOxygenLight;


    private void Start()
    {
        InitLight();
    }

    private void Update()
    {
        UpdateIndicators();
    }

    private void InitLight()
    {
        lowOxygenLight.SetColor(Color.red);

    }

    private void UpdateIndicators()
    {
        powerBar.SetTargetScaleYFromRatio(OxygenManager.Instance.PowerRatio);
        oxygenGauge.SetTargetAngleFromRatio(OxygenManager.Instance.OxygenRatio);

        if (OxygenManager.Instance.GeneratingOxygen) generatingOxygenLight.SetState(LightStates.On);
        else generatingOxygenLight.SetState(LightStates.Off);

        if (OxygenManager.Instance.OxygenRatio > 0.38)
        {
            highOxygenLight.SetState(LightStates.On);
            lowOxygenLight.SetState(LightStates.Off);
        }
        else if (OxygenManager.Instance.OxygenRatio > 0.25)
        {
            highOxygenLight.SetState(LightStates.Off);
            lowOxygenLight.SetState(LightStates.On);
        }
        else if (OxygenManager.Instance.OxygenRatio > 0)
        {
            highOxygenLight.SetState(LightStates.Off);
            lowOxygenLight.SetState(LightStates.Blinking);
        }
        else
        {
            highOxygenLight.SetState(LightStates.Off);
            lowOxygenLight.SetState(LightStates.Strobing);
        }
    }
}
