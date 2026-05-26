using UnityEngine;

public class HeadChargerInteraction : MonoBehaviour, IInteraction
{
    private BatteryManager batteryManager;
    private PlayerStateController playerState;

    private void Awake()
    {
        batteryManager = FindAnyObjectByType<BatteryManager>();
        playerState = FindAnyObjectByType<PlayerStateController>();
    }

    public void PerformInteraction(GameObject interactor)
    {
        if(playerState == null || batteryManager == null) return;
        if(!playerState.placedHeadVolume.isHeadCharger) return;
        
        batteryManager.StartChargingBattery();
    }
}