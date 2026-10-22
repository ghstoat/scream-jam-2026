using UnityEngine;

public class FlashMachine : MonoBehaviour
{
    [SerializeField] Interactable pumpLever;
    public Interactable PumpLever => pumpLever;

    [SerializeField] FillBar chargeBar;

    [SerializeField] LightBulb chargedLight;

    [SerializeField] FillBar powerBar;
    [SerializeField] LightBulb powerLight;

    [SerializeField] AudioSource flashSource;
    [SerializeField] AudioSource flashChargedSource;

    [SerializeField] PlayerDialogue flashNoPower;
    [SerializeField] PlayerDialogue flashNoCharge;

    private void Start()
    {
        FlashManager.Instance.onFlash.AddListener(OnFlash);
        FlashManager.Instance.onCharged.AddListener(OnCharged);

    }

    private void OnDestroy()
    {
        FlashManager.Instance.onCharged.RemoveListener(OnCharged);

    }

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
        if (FlashManager.Instance.PowerEmpty) PlayerDialogueManager.Instance.ForceDialogue(flashNoPower);


        FlashManager.Instance.Pump();
    }

    public void Flash()
    {
        if (!FlashManager.Instance.Charged) PlayerDialogueManager.Instance.ForceDialogue(flashNoCharge);

        FlashManager.Instance.UseFlash();
    }

    private void OnFlash()
    {
        flashSource.Play();
    }
    private void OnCharged()
    {
        flashChargedSource.Play();
    }
    
}
