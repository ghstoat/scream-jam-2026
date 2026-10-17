using UnityEngine;

public class FlashMachine : MonoBehaviour
{

    [SerializeField] FillBar chargeBar;

    [SerializeField] LightBulb chargedLight;


    private void Update()
    {
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        chargeBar.SetTargetScaleYFromRatio(FlashManager.Instance.ChargeRatio);

        if (FlashManager.Instance.Charged) chargedLight.SetState(LightStates.On);
        else chargedLight.SetState(LightStates.Off);
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
