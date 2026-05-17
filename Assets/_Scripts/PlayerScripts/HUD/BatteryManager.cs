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
    [SerializeField] private int currentBatteryCells;
    private Coroutine ChargeBatteryCoroutine;
    private PlayerInputs inputs;

    private void Start()
    {
        //currentBatteryCells = maxBatteryCells;
        UpdateBatteryHUD();
        Debug.Log(currentBatteryCells);
    }

        void Awake()
        {
            inputs = FindAnyObjectByType<PlayerInputs>();

            if(inputs == null)
            {
                Debug.LogError("BatteryManager could not find PlayerInputs in the scene.");
            }
        }

        void OnEnable()
        {
            //inputs.OnSignalBoost += () => DepleteBattery(1);

            inputs.OnSignalBoost += StartChargingBattery;

            if(inputs == null)
            {
                Debug.LogError("BatteryManager could not subscribe to PlayerInputs events because PlayerInputs reference is missing.");
            }
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
            //Debug.Log(currentBatteryCells);
            yield return new WaitForSeconds(.85f);
        }
    }

    private void UpdateBatteryHUD()
    {
        for (int i = 0; i < BatteryIcons.Length; i++)
        {
            //Debug.Log(currentBatteryCells);
            BatteryIcons[i].SetActive(i < currentBatteryCells);
            // loops through all cells and sets that one active if its index is less than the current battery cells that should be active.
        }
    }

    private void DepleteBattery(int depleteAmount)
    {
        currentBatteryCells = Mathf.Max(currentBatteryCells - depleteAmount, 0);
        Debug.Log(currentBatteryCells);
        audioSource.PlayOneShot(cellChangeSFX);
        UpdateBatteryHUD();
    }
}
