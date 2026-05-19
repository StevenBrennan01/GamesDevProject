using System.Collections;
using UnityEngine;

public class BatteryManager : MonoBehaviour
{
    [Header("-= Battery HUD Elements =-")]
    [Space(5)]
    [SerializeField] private GameObject BatteryParent;
    [SerializeField] private GameObject[] BatteryIcons;
    
    [Header("-= SFX & Audio Sources =-")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cellChangeSFX;

    [Header("-= Battery Values =-")]
    private int maxBatteryCells = 5;
    [SerializeField] public int currentBatteryCells;
    private Coroutine ChargeBatteryCoroutine;

    private void Start()
    {
        currentBatteryCells = maxBatteryCells;
        UpdateBatteryHUD();
    }

    public void StartChargingBattery()
    {
        if (ChargeBatteryCoroutine == null)
        {
            ChargeBatteryCoroutine = StartCoroutine(ChargeBattery());
        }
    }

    private IEnumerator ChargeBattery()
    {
        while (currentBatteryCells < maxBatteryCells)
        {
            currentBatteryCells++;
            audioSource.PlayOneShot(cellChangeSFX);
            UpdateBatteryHUD();

            yield return new WaitForSeconds(.85f);
        }

        ChargeBatteryCoroutine = null;
    }

    private void UpdateBatteryHUD()
    {
        for (int i = 0; i < BatteryIcons.Length; i++)
        {
            BatteryIcons[i].SetActive(i < currentBatteryCells);
            // loops through all cells and sets that one active if its index is less than the current battery cells that should be active.
        }
    }

    public bool DepleteBattery(int depleteAmount)
    {
        if(depleteAmount > currentBatteryCells)
        {
            Debug.Log("Not enough battery cells to perform action!");
            return false;
        }

        currentBatteryCells = Mathf.Max(currentBatteryCells - depleteAmount, 0);
        //Debug.Log(currentBatteryCells);

        audioSource.PlayOneShot(cellChangeSFX);// can maybe call audio elswhere as might want different sfx for depleting vs signalboost
        UpdateBatteryHUD();

        return true;
    }
}