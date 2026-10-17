using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FlashManager : PoweredSystem
{
    [Header("Charge decays at one per second")]
    [SerializeField] float chargePerPower = 1;
    private float storedCharge = 0f;

    [SerializeField] float chargeRequiredToFlash = 250f;
    public float ChargeRatio => storedCharge / chargeRequiredToFlash;
    public bool Charged => storedCharge >= chargeRequiredToFlash;


    [SerializeField] int numOfFlashes;
    [SerializeField] float timeBetweenFlashes;
    public UnityEvent onFlash;

    private Coroutine flashRoutine;

    public static FlashManager Instance { get; private set; }
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

    private void LateUpdate()
    {
        if (!Charged && flashRoutine == null)
        {
            storedCharge -= Time.deltaTime;
            storedCharge = Mathf.Clamp(storedCharge, 0, chargeRequiredToFlash);

        }
    }

    public void Pump()
    {
        storedCharge += (float)currentPower * chargePerPower;
        storedCharge = Mathf.Clamp(storedCharge, 0, chargeRequiredToFlash);
    }

    public void UseFlash()
    {
        if (Charged  && flashRoutine == null) flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float chargePerFlash = chargeRequiredToFlash / numOfFlashes;

        for (int i = 0; i < numOfFlashes; i++)
        {
            onFlash?.Invoke();

            storedCharge = Mathf.Max(0f, storedCharge - chargePerFlash);

            if (i < numOfFlashes - 1) yield return new WaitForSeconds(timeBetweenFlashes);
        }

        flashRoutine = null;
    }

    [Button]
    private void ForceFlash()
    {
        storedCharge = chargeRequiredToFlash;
        UseFlash();
    }
}
