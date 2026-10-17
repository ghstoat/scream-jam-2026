using UnityEngine;

public class Vent : MonoBehaviour
{
    [SerializeField] FillBar topShutter;
    [SerializeField] FillBar botShutter;

    [SerializeField] LightBulb closedLight; 

    [SerializeField] LightBulb openingLight;
    [SerializeField] LightBulb closingLight;

    [SerializeField] FillBar powerBar;
    [SerializeField] LightBulb powerLight;

    private void Update()
    {
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        topShutter.SetTargetScaleYFromRatio(VentManager.Instance.closingProgress);
        botShutter.SetTargetScaleYFromRatio(VentManager.Instance.closingProgress);

        if (VentManager.Instance.Closed) closedLight.SetState(LightStates.On);
        else closedLight.SetState(LightStates.Off); 

        if (!VentManager.Instance.PowerEmpty)
        {
            if (VentManager.Instance.opening)
            {
                openingLight.SetState(LightStates.On);
                closingLight.SetState(LightStates.Off);
            }
            else
            {
                openingLight.SetState(LightStates.Off);
                closingLight.SetState(LightStates.On);
            }
        }
        else
        {
            openingLight.SetState(LightStates.Off);
            closingLight.SetState(LightStates.Off);
        }

        powerBar.SetTargetScaleYFromRatio(VentManager.Instance.PowerRatio);
        if (VentManager.Instance.PowerEmpty) powerLight.SetState(LightStates.Off);
        else powerLight.SetState(LightStates.On);
    }

    public void TryToOpen()
    {
        VentManager.Instance.TryToOpen();
    }

    public void TryToClose()
    {
        VentManager.Instance.TryToClose();

    }

}
