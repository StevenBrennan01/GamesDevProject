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

    void Awake()
    {
        BatteryParent.SetActive(false);
        SignalParent.SetActive(false);
    }

    public IEnumerator BatteryStartup()
    {
        yield return new WaitForSeconds(2f);
        BatteryParent.SetActive(true);
        audioSource.PlayOneShot(HUDStartupSFX[0]);

        yield return new WaitForSeconds(0.25f);
        BatteryParent.SetActive(false);

        yield return new WaitForSeconds(0.25f);
        BatteryParent.SetActive(true);
        audioSource.PlayOneShot(HUDStartupSFX[0]);

        yield return new WaitForSeconds(0.25f);
        BatteryParent.SetActive(false);

        yield return new WaitForSeconds(0.25f);
        BatteryParent.SetActive(true);
        audioSource.PlayOneShot(HUDStartupSFX[0]);
        yield return new WaitForSeconds(.75f);

        for (int i = 0; i < BatteryIcons.Length; i++)
        {
            audioSource.PlayOneShot(HUDStartupSFX[1]);
            BatteryIcons[i].SetActive(true);
            yield return new WaitForSeconds(1.42f);
        }
    }

    public IEnumerator SignalStartup()
    {
        yield return new WaitForSeconds(2f);
        SignalParent.SetActive(true);
        audioSource.PlayOneShot(HUDStartupSFX[0]);

        yield return new WaitForSeconds(0.25f);
        SignalParent.SetActive(false);

        yield return new WaitForSeconds(0.25f);
        SignalParent.SetActive(true);
        audioSource.PlayOneShot(HUDStartupSFX[0]);

        yield return new WaitForSeconds(0.25f);
        SignalParent.SetActive(false);

        yield return new WaitForSeconds(0.25f);
        SignalParent.SetActive(true);
        audioSource.PlayOneShot(HUDStartupSFX[0]);
        yield return new WaitForSeconds(.75f);

        for (int i = 0; i < SignalIcons.Length; i++)
        {
            audioSource.PlayOneShot(HUDStartupSFX[1]);
            SignalIcons[i].SetActive(true);
            yield return new WaitForSeconds(1.575f);
        }
    }
}
