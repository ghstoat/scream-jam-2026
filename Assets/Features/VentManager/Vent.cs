using System.Collections;
using UnityEngine;

public class Vent : MonoBehaviour
{
    [SerializeField] FillBar topShutter;
    [SerializeField] FillBar botShutter;

    [SerializeField] LightBulb closedLight; 

    [SerializeField] LightBulb openingLight;
    [SerializeField] LightBulb closingLight;

    [SerializeField] FillBar powerBar;
    [SerializeField] Gauge toxicityGauge;

    [SerializeField] AudioSource ambianceSource;

    [SerializeField] AudioClip openVentClip;
    [SerializeField] AudioClip closedVentClip;

    [SerializeField] PlayerDialogue ventNoPower;

    [SerializeField] Interactable lever;
    public Interactable Lever => lever;

    private bool closedLastFrame;


    [SerializeField] AudioSource openVentGhostSource;
    [SerializeField] AudioSource closedVentGhostSource;

    private Coroutine ghostInVentRoutine;

    private void Start()
    {
        closedLastFrame = !VentManager.Instance.Closed;
    }

    private void Update()
    {
        UpdateIndicators();
        UpdateAudio();
    }

    private void UpdateAudio()
    {
        bool closed = VentManager.Instance.Closed;
        if (closedLastFrame == closed) return;

        ambianceSource.Stop();

        if (closed) ambianceSource.clip = closedVentClip;
        else ambianceSource.clip = openVentClip;

        ambianceSource.Play();


        closedLastFrame = closed;
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
        toxicityGauge.SetTargetAngleFromRatio(1 - VentManager.Instance.ToxicityRatio);
    }

    public void TryToOpen()
    {
        if (VentManager.Instance.PowerEmpty) PlayerDialogueManager.Instance.ForceDialogue(ventNoPower);

        VentManager.Instance.TryToOpen();
    }

    public void TryToClose()
    {
        if (VentManager.Instance.PowerEmpty) PlayerDialogueManager.Instance.ForceDialogue(ventNoPower);

        VentManager.Instance.TryToClose();

    }
    
    public void SetGhostInVent(bool yes)
    {
        if (yes)
        {
            if (ghostInVentRoutine != null) return;
            ghostInVentRoutine = StartCoroutine(GhostInVentRoutine());
        }

        else
        {
            if (ghostInVentRoutine != null) StopCoroutine(ghostInVentRoutine);
            ghostInVentRoutine = null;

            openVentGhostSource.volume = 0;
            closedVentGhostSource.volume = 0;
        }
    }

    private IEnumerator GhostInVentRoutine()
    {


        while (true)
        {
            bool closed = VentManager.Instance.Closed;

            if (closed)
            {
                openVentGhostSource.volume = 0;
                closedVentGhostSource.volume = 1;
            }
            else
            {
                openVentGhostSource.volume = 1;
                closedVentGhostSource.volume = 0;
            }



            yield return null;
        }
    }
}
