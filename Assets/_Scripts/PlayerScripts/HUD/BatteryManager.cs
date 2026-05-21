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
    [SerializeField] private AudioClip cellGlitchSFX;

    [Header("-= Battery Values =-")]
    private int maxBatteryCells = 5;
    [SerializeField, Range(0, 5)] public int currentBatteryCells = 0;
    private Coroutine ChargeBatteryCoroutine;

    public void SetBatteryFull()
    {
        currentBatteryCells = maxBatteryCells;
        UpdateBatteryHUD();
    }

            // Only for testing, remove when done
            private void Update() // will however use update at some point to deplete 1 cell every 60 seconds or so
            {
                Debug.Log(this + " Is actively running the Update function, delete it when done testing");
                UpdateBatteryHUD();
            }
            // Only for testing, remove when done

    public void StartChargingBattery()
    {
        if (ChargeBatteryCoroutine == null)
        {
            ChargeBatteryCoroutine = StartCoroutine(ChargeBattery());
        }
    }

    private IEnumerator ChargeBattery()
    {
        yield return new WaitForSeconds(.85f);

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

        //audioSource.PlayOneShot(cellChangeSFX); // can maybe call audio elswhere as might want different sfx for depleting vs signalboost
        UpdateBatteryHUD();

        return true;
    }



    // Below is most likely going to be used for depleting one cell every x amount of time
    public void DepleteOneCell()
    {
        StartCoroutine(DepleteOneCellCoroutine());
    }

    private IEnumerator DepleteOneCellCoroutine()
    {
        if(currentBatteryCells <= 0)
        {
            Debug.Log("No battery cells left to deplete!");
            yield break;
        }

        BatteryIcons[currentBatteryCells - 1].SetActive(false);
        audioSource.PlayOneShot(cellGlitchSFX);

        yield return new WaitForSeconds(0.2f);
        BatteryIcons[currentBatteryCells - 1].SetActive(true);

        yield return new WaitForSeconds(0.2f);
        BatteryIcons[currentBatteryCells - 1].SetActive(false);
        audioSource.PlayOneShot(cellGlitchSFX);

        yield return new WaitForSeconds(0.2f);
        BatteryIcons[currentBatteryCells - 1].SetActive(true);

        yield return new WaitForSeconds(0.2f);
        BatteryIcons[currentBatteryCells - 1].SetActive(false);
        //audioSource.PlayOneShot(cellGlitchSFX);
        audioSource.PlayOneShot(cellChangeSFX);

        currentBatteryCells = Mathf.Max(currentBatteryCells - 1, 0);
        UpdateBatteryHUD();
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
        //audioSource.PlayOneShot(cellGlitchSFX);
        audioSource.PlayOneShot(cellChangeSFX);
        UpdateBatteryHUD();
    }
}