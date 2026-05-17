using UnityEngine;

public class HeadChargerInteraction : MonoBehaviour, IInteraction
{
    public bool startCharging;

    public void PerformInteraction(GameObject interactor)
    {
        startCharging = true;
    }
}
