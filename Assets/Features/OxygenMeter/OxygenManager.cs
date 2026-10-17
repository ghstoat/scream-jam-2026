using NaughtyAttributes;
using UnityEngine;

public class OxygenManager : PoweredSystem
{
    [Header("Oxygen decreases 1 unit per second")]
    [SerializeField] float maxOxygen;
    public float oxygen { get; private set; }   
    public float OxygenRatio => oxygen/ maxOxygen; 
    public bool GeneratingOxygen => currentPower >= powerRequired;

    [SerializeField] float powerRequired;
    [SerializeField] float oxygenPerPower;

    public static OxygenManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"!!! Multiple {this.name} found in scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        oxygen = maxOxygen;
    }
    public void ChangeOxygen(float add)
    {
        oxygen += add;

        oxygen = Mathf.Clamp(oxygen, 0f, maxOxygen);
    }

    private void Update()
    {
        if (currentPower >= powerRequired)
        {
            GenerateOxygen();
        }
        else ChangeOxygen(-Time.deltaTime);

    }

    public void GenerateOxygen()
    {
        if (oxygen >= maxOxygen) return;

        float excessPower = currentPower - powerRequired;
        float oxygenPerSecond = excessPower * oxygenPerPower;

        ChangeOxygen(oxygenPerSecond * Time.deltaTime);

    }



}
