using UnityEngine;

public class FlashMachine : MonoBehaviour
{

    [SerializeField] FillBar chargeBar;

    [SerializeField] LightBulb chargedLight;

    [SerializeField] FillBar powerBar;
    [SerializeField] LightBulb powerLight;

    private void Update()
    {
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        chargeBar.SetTargetScaleYFromRatio(FlashManager.Instance.ChargeRatio);

        if (FlashManager.Instance.Charged) chargedLight.SetState(LightStates.On);
        else
        {
            chargedLight.SetState(LightStates.Off);

        }

        powerBar.SetTargetScaleYFromRatio(FlashManager.Instance.PowerRatio);
        if (FlashManager.Instance.PowerEmpty) powerLight.SetState(LightStates.Off);
        else powerLight.SetState(LightStates.On);
    }

    public void Pump()
    {
        FlashManager.Instance.Pump();
    }

    public void Flash()
    {
        FlashManager.Instance.UseFlash();
    }
}
