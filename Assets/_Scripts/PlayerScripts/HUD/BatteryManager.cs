using System.Collections;
using TMPro;
using UnityEngine;

public class BatteryManager : MonoBehaviour
{
    private LevelLoadManager levelLoadManager;
    private PlayerInputs playerInputs;

    [Header("-= Battery HUD Elements =-")]
    [Space(5)]
    [SerializeField] public GameObject batteryParent;
    [SerializeField] private GameObject[] batteryIcons;
    [SerializeField] private GameObject countdownParent;
    [SerializeField] private GameObject[] batteryCountdownIcons;
    [Space(10)]
    [SerializeField] private TextMeshProUGUI batteryFullText;
    [SerializeField] private TextMeshProUGUI batteryLowText;
    
    [Header("-= SFX & Audio Sources =-")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cellChangeSFX;
    [SerializeField] private AudioClip cellGlitchSFX;

    [Header("-= Battery Values =-")]
    private int maxBatteryCells = 5;
    [SerializeField, Range(0, 5)] public int currentBatteryCells = 0;
    private Coroutine ChargeBatteryCoroutine;
    private Coroutine FlickerBatteryCoroutine;
    private Coroutine CountDownToResetCoroutine;

    public bool isCharging;
    public bool isCountingDownToReset;

    private void Awake()
    {
        if(levelLoadManager == null) levelLoadManager = FindAnyObjectByType<LevelLoadManager>();
        if(levelLoadManager == null) Debug.LogError("LevelLoadManager reference is missing / not found");

        if(playerInputs == null) playerInputs = FindAnyObjectByType<PlayerInputs>();
        if(playerInputs == null) Debug.LogError("PlayerInputs reference is missing / not found");

        batteryFullText.gameObject.SetActive(false);
        batteryLowText.gameObject.SetActive(false);
        countdownParent.SetActive(false);
    }

    public void SetBatteryFull()
    {
        currentBatteryCells = maxBatteryCells;

        if (FlickerBatteryCoroutine != null)
        {
            StopCoroutine(FlickerBatteryCoroutine);
            FlickerBatteryCoroutine = null;
        }

        if (CountDownToResetCoroutine != null)
        {
            StopCoroutine(CountDownToResetCoroutine);
            CountDownToResetCoroutine = null;
        }

        if (ChargeBatteryCoroutine != null)
        {
            StopCoroutine(ChargeBatteryCoroutine);
            ChargeBatteryCoroutine = null;
        }

        batteryParent.SetActive(true);
        batteryLowText.gameObject.SetActive(false);
        batteryFullText.gameObject.SetActive(false);
        countdownParent.SetActive(false);
        HideAllCountdownIcons();

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
        if(FlickerBatteryCoroutine != null)
        {
            StopCoroutine(FlickerBatteryCoroutine);
            FlickerBatteryCoroutine = null;
            batteryParent.SetActive(true);
            batteryLowText.gameObject.SetActive(false);
        }
        if(CountDownToResetCoroutine != null)
        {
            StopCoroutine(CountDownToResetCoroutine);
            CountDownToResetCoroutine = null;
            isCountingDownToReset = false;
            countdownParent.SetActive(false);
            
            HideAllCountdownIcons();
        }

        isCharging = true;
        yield return new WaitForSeconds(.85f);

        while (currentBatteryCells < maxBatteryCells)
        {
            currentBatteryCells++;
            audioSource.PlayOneShot(cellChangeSFX);
            UpdateBatteryHUD();

            yield return new WaitForSeconds(.85f);
        }

        isCharging = false;
        ChargeBatteryCoroutine = null;

        batteryFullText.gameObject.SetActive(true);
        audioSource.PlayOneShot(cellGlitchSFX);
        yield return new WaitForSeconds(.25f);
        batteryFullText.gameObject.SetActive(false);
        yield return new WaitForSeconds(.25f);
        batteryFullText.gameObject.SetActive(true);
        audioSource.PlayOneShot(cellGlitchSFX);
        yield return new WaitForSeconds(.25f);
        batteryFullText.gameObject.SetActive(false);
    }

    private void UpdateBatteryHUD()
    {
        for (int i = 0; i < batteryIcons.Length; i++)
        {
            batteryIcons[i].SetActive(i < currentBatteryCells);
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

        audioSource.PlayOneShot(cellChangeSFX); // can maybe call audio elswhere as might want different sfx for depleting vs signalboost
        UpdateBatteryHUD();

        if(currentBatteryCells <= 1 && FlickerBatteryCoroutine == null)
        {
            FlickerBatteryCoroutine = StartCoroutine(FlickerWholeBatteryIcon());
        }
        if(currentBatteryCells <= 0)
        {
            if(CountDownToResetCoroutine == null)
            {
                CountDownToResetCoroutine = StartCoroutine(StartCountDownToLevelReset());
            }
        }

        return true;
    }

    private IEnumerator StartCountDownToLevelReset()
    {
        isCountingDownToReset = true;
        countdownParent.SetActive(true);

        HideAllCountdownIcons();

        for(int i = 0; i < batteryCountdownIcons.Length; i++)
        {
            if(currentBatteryCells > 0)
            {
                countdownParent.SetActive(false);
                isCountingDownToReset = false;
                CountDownToResetCoroutine = null;
                yield break;
            }

            batteryCountdownIcons[i].SetActive(true);
            audioSource.PlayOneShot(cellChangeSFX);

            if(i == batteryCountdownIcons.Length - 1)
            {
                batteryParent.SetActive(false);
                batteryLowText.gameObject.SetActive(false);
                yield return new WaitForSeconds(1f);
                break;
            }

            yield return new WaitForSeconds(1f);

            HideAllCountdownIcons();
        }

        isCountingDownToReset = false;

        CountDownToResetCoroutine = null;

        if(FlickerBatteryCoroutine != null)
        {
            StopCoroutine(FlickerBatteryCoroutine);
            FlickerBatteryCoroutine = null;
        }

        batteryLowText.gameObject.SetActive(false);
        batteryParent.SetActive(false);
        countdownParent.SetActive(false);
        HideAllCountdownIcons();

        if(currentBatteryCells <= 0)
        {
            levelLoadManager.ReloadCurrentLevel();
        }
    }

    private void HideAllCountdownIcons()
    {
        foreach(GameObject icon in batteryCountdownIcons)
        {
            icon.SetActive(false);
        }
    }

    // Below is the same as the above DepleteBattery but it just triggers a coroutine to wait for a second, so signal boost
    // effect can happen, then it does the battery depletion, sound effects, that sort of thing, so it doesnt happen all at once.
    public bool DepleteBatteryAfterSignalBoost(int depleteAmount)
    {
        if(depleteAmount > currentBatteryCells)
        {
            Debug.Log("Not enough battery cells to perform action!");
            return false;
        }

        StartCoroutine(DepleteBatteryAfterSignalBoost_Coroutine(depleteAmount));
        return true;
    }

    private IEnumerator DepleteBatteryAfterSignalBoost_Coroutine(int depleteAmount)
    {
        yield return new WaitForSeconds(.75f); // delay to allow signal boost effect to trigger before battery depletes

        if(depleteAmount > currentBatteryCells)
        {
            Debug.Log("Not enough battery cells to perform action!");
            yield break;
        }

        currentBatteryCells = Mathf.Max(currentBatteryCells - depleteAmount, 0);
        audioSource.PlayOneShot(cellChangeSFX);
        UpdateBatteryHUD();

        if(currentBatteryCells <= 1 && FlickerBatteryCoroutine == null)
        {
            FlickerBatteryCoroutine = StartCoroutine(FlickerWholeBatteryIcon());
        }
        if(currentBatteryCells <= 0)
        {
            if(CountDownToResetCoroutine == null)
            {
                yield return new WaitForSeconds(1.25f);
                CountDownToResetCoroutine = StartCoroutine(StartCountDownToLevelReset());
            }
        }

        //yield break;
    }

    private IEnumerator FlickerWholeBatteryIcon()
    {
        if(batteryParent != null)
        {
            yield return new WaitForSeconds(1.25f);

            while(true)
            {
                batteryParent.SetActive(false);
                batteryLowText.gameObject.SetActive(false);
                audioSource.PlayOneShot(cellGlitchSFX);
                yield return new WaitForSeconds(0.25f);

                batteryParent.SetActive(true);
                batteryLowText.gameObject.SetActive(true);
                audioSource.PlayOneShot(cellGlitchSFX);
                yield return new WaitForSeconds(0.25f);
            }
        }
    }
}