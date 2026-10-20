using System.Collections;
using UnityEngine;

public class TemperatureManager : PoweredSystem
{
    [SerializeField] float minLivableTemp;
    [SerializeField] float maxLivableTemp;
    public bool Overheating => intTemp > maxLivableTemp;
    public bool Freezing => intTemp < minLivableTemp;
    public bool Livable => intTemp < maxLivableTemp && intTemp > minLivableTemp;

    [SerializeField] float minExtTemp;
    [SerializeField] float maxExtTemp;
    public float extTemp { get; private set; }

    [SerializeField] float maxHeaterTemp;
    public float heaterTemp { get; private set; }

    private float targetTemp;
    public float intTemp { get; private set; }

    [SerializeField] float tempChangeSpeed;

    [SerializeField] float maxLightTemp = 20000;
    [SerializeField] float minLightTemp = 3000;


    [SerializeField] float minTimeBetweenSolarFlares;
    [SerializeField] float maxTimeBetweenSolarFlares;

    private float timeToNextSolarFlare;
    public float TimeToNextSolarFlare => timeToNextSolarFlare;
    public static TemperatureManager Instance { get; private set; }
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


    private void Start()
    {
        StartCoroutine(SolarFlareRoutine() );
        intTemp = extTemp + heaterTemp;
    }

    private void Update()
    {
        HandleHeaterTemp();
        HandleInternalTemp();
    }

    private void HandleInternalTemp()
    {
        targetTemp = extTemp + heaterTemp;

        if (intTemp != targetTemp)
        {
            intTemp = Mathf.MoveTowards(intTemp, targetTemp, tempChangeSpeed * Time.deltaTime);
        }

        float tempRatio = 1 - Mathf.InverseLerp(minLivableTemp, maxLivableTemp, intTemp);

        CockpitManager.Instance.MainLight.colorTemperature = Mathf.Lerp(minLightTemp, maxLightTemp, tempRatio);
    }

    private IEnumerator SolarFlareRoutine()
    {
        while (true)
        {
            extTemp = Random.Range(minExtTemp, maxExtTemp);
            timeToNextSolarFlare = Random.Range(minTimeBetweenSolarFlares, maxTimeBetweenSolarFlares);
            while (timeToNextSolarFlare > 0)
            {
                timeToNextSolarFlare -= Time.deltaTime;
                if (timeToNextSolarFlare < 0)
                {
                    CockpitManager.Instance.ShakeCockpit(.1f, 2.5f);
                }

                yield return null;
            }
        }
    }

    public void HandleHeaterTemp()
    {
        float ratio = (float)currentPower / maxPower;
        heaterTemp = maxHeaterTemp * ratio; 
    }

}
