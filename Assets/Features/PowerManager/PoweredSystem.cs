using NaughtyAttributes;
using UnityEngine;

public abstract class PoweredSystem : MonoBehaviour
{
    public int currentPower { get; private set; } = 0;
    public bool PowerFull => currentPower >= maxPower;
    public bool PowerEmpty => currentPower == 0;    
    public float PowerRatio => (float)currentPower / (float)maxPower;

    [SerializeField, Min(0)] protected int maxPower;

    

    public virtual void FillPower()
    {
        AllocatePower(maxPower);
    }

    public int GetAllocateOffset(int power)
    {
        if (currentPower + power <= maxPower) return 0;

        return currentPower + power - maxPower;
    }

    public int GetWithdrawOffset(int power)
    {
        if (currentPower - power >= 0) return 0;

        return power - currentPower;
    }

    public void AllocatePower(int power)
    {
        currentPower += power;
        currentPower = Mathf.Clamp(currentPower, 0, maxPower);
    }

    public void WithdrawPower(int power)
    {
        currentPower -= power;
        if (currentPower < 0)
        {
            Debug.LogWarning($"!!! {this.name} has somehow fallen to negative power!");
        }
    }

    public int DrainPower()
    {
        int power = currentPower;
        currentPower = 0;

        return power;
    }

    public void DrainPowerToReservoir()
    {
        PowerManager.Instance.DrainSystem(this);
    }

    [Button]
    private void DebugDrainPowerToReservoir()
    {
        DrainPowerToReservoir();
    }

    [Button]
    private void DebugGrab10PowerFromReservoir()
    {
        PowerManager.Instance.SetSystemA(PowerManager.Instance.PowerReservoir);
        PowerManager.Instance.SetSystemB(this);

        PowerManager.Instance.MovePowerFromAToB(10);

    }
}
