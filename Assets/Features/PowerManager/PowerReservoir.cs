using UnityEngine;

public class PowerReservoir : PoweredSystem
{

    public static PowerReservoir Instance { get; private set; }
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
        UpdatePowerReservoirMaxPower();
    }

    public void UpdatePowerReservoirMaxPower()
    {
        maxPower = PowerManager.Instance.TotalPower;
    }

    public override void FillPower()
    {
        
    }

}
