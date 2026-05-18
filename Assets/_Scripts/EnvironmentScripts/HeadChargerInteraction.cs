using UnityEngine;

public class HeadChargerInteraction : MonoBehaviour, IInteraction
{
    public BatteryManager batteryManager;

    private void Awake()
    {
        batteryManager = FindAnyObjectByType<BatteryManager>();
    }

    public void PerformInteraction(GameObject interactor)
    {
        batteryManager.StartChargingBattery();
    }
}