using NaughtyAttributes;
using UnityEngine;

public class PowerManager : MonoBehaviour
{
    public PoweredSystem systemA { get; private set; }
    public PoweredSystem systemB { get; private set; }

    [SerializeField] int totalPower;
    public int TotalPower => totalPower;    

    [SerializeField] PoweredSystem[] allPoweredSystems;
    public PoweredSystem[] AllPoweredSystems => allPoweredSystems;

    [SerializeField] PowerReservoir powerReservoir;
    public PowerReservoir PowerReservoir => powerReservoir;


    public static PowerManager Instance { get; private set; }
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
        DrainAllSystems();
    }

    public void DrainAllSystems()
    {
        powerReservoir.UpdatePowerReservoirMaxPower();

        foreach (var system in allPoweredSystems)
        {
            DrainSystem(system);
        }

        if (!powerReservoir.PowerFull) powerReservoir.AllocatePower(totalPower); //refill any lost power
    }

    public void DrainSystem(PoweredSystem system)
    {
        int drainedPower = system.DrainPower();
        powerReservoir.AllocatePower(drainedPower);
    }

    public void SetSystemA(PoweredSystem systemA)
    {
        this.systemA = systemA; 
    }

    public void SetSystemB(PoweredSystem systemB)
    {
        this.systemB = systemB;
    }

    public void MovePowerFromAToB(int power)
    {
        if (power == 0) return;
        if (systemA == null || systemB == null)
        {
            Debug.LogWarning("PowerManager requires both systems to be assigned.", this);
            return;
        }

        int powerToMove = Mathf.Abs(power);

        PoweredSystem A = systemA;
        PoweredSystem B = systemB;

        if (power < 0)
        {
            A = systemB;
            B = systemA;
        }

        int offsetA = A.GetWithdrawOffset(powerToMove);
        int offsetB = B.GetAllocateOffset(powerToMove);

        powerToMove -= Mathf.Max(offsetA, offsetB);

        if (powerToMove <= 0) return;

        A.WithdrawPower(powerToMove);
        B.AllocatePower(powerToMove);
    }

    public bool IsSystemsABAssigned()
    {
        return systemA != null && systemB != null;
    }

    public bool IsSystemAOrBEmpty()
    {
        if (!IsSystemsABAssigned()) return false;

        return systemA.PowerEmpty || systemB.PowerEmpty;
    }

    public bool IsSystemAOrBFull()
    {
        if (!IsSystemsABAssigned()) return false;

        return systemA.PowerFull || systemB.PowerFull;
    }


    [Button]
    private void DebugDrainAllSystems()
    {
        DrainAllSystems();
    }
}
