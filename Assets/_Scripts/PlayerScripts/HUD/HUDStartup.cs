using System.Collections;
using UnityEngine;

public class HUDStartup : MonoBehaviour
{
    [Header("-= Parent HUD Elements =-")]
    [Space(5)]
    [SerializeField] private GameObject BatteryParent;
    [SerializeField] private GameObject SignalParent;

    [Header("-= HUD Icons =-")]
    [Space(5)]
    [SerializeField] private GameObject[] BatteryIcons;
    [SerializeField] private GameObject[] SignalIcons;

    [Header("-= SFX & Audio Sources =-")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] HUDStartupSFX;

    [SerializeField] private bool turnOffOnStartup;
    
    private BatteryManager batteryManager;

    void Awake()
    {
        if (turnOffOnStartup)
        {
            BatteryParent.SetActive(false);
            SignalParent.SetActive(false);
        }

        batteryManager = FindAnyObjectByType<BatteryManager>();
    }

    public IEnumerator BatteryStartup()
    {
        yield return new WaitForSeconds(1.25f);
        BatteryParent.SetActive(true);
        audioSource.PlayOneShot(HUDStartupSFX[0]);

        yield return new WaitForSeconds(0.15f); // originally 0.2f
        BatteryParent.SetActive(false);

        yield return new WaitForSeconds(0.15f); // originally 0.2f
        BatteryParent.SetActive(true);
        audioSource.PlayOneShot(HUDStartupSFX[0]);

        yield return new WaitForSeconds(0.15f); // originally 0.2f
        BatteryParent.SetActive(false);

        yield return new WaitForSeconds(0.15f); // originally 0.2f
        BatteryParent.SetActive(true);
        audioSource.PlayOneShot(HUDStartupSFX[0]);
        
        yield return new WaitForSeconds(.4f);

        for (int i = 0; i < BatteryIcons.Length; i++)
        {
            audioSource.PlayOneShot(HUDStartupSFX[1]);
            BatteryIcons[i].SetActive(true);
            yield return new WaitForSeconds(1f); // originally 1.42f
        }

        batteryManager.SetBatteryFull();
    }

    public IEnumerator SignalStartup()
    {
        yield return new WaitForSeconds(1.25f);
        SignalParent.SetActive(true);
        audioSource.PlayOneShot(HUDStartupSFX[0]);

        yield return new WaitForSeconds(0.15f);
        SignalParent.SetActive(false);

        yield return new WaitForSeconds(0.15f);
        SignalParent.SetActive(true);
        audioSource.PlayOneShot(HUDStartupSFX[0]);

        yield return new WaitForSeconds(0.15f);
        SignalParent.SetActive(false);

        yield return new WaitForSeconds(0.15f);
        SignalParent.SetActive(true);
        audioSource.PlayOneShot(HUDStartupSFX[0]);
        
        yield return new WaitForSeconds(.4f);

        for (int i = 0; i < SignalIcons.Length; i++)
        {
            audioSource.PlayOneShot(HUDStartupSFX[1]);
            SignalIcons[i].SetActive(true);
            yield return new WaitForSeconds(1f); // originally 1.575f
        }
    }
}
