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

    void Awake()
    {
        BatteryParent.SetActive(false);
        SignalParent.SetActive(false);
    }

    public IEnumerator BatteryStartup()
    {
        yield return new WaitForSeconds(2f);
        BatteryParent.SetActive(true);

        yield return new WaitForSeconds(0.25f);
        BatteryParent.SetActive(false);

        yield return new WaitForSeconds(0.25f);
        BatteryParent.SetActive(true);

        yield return new WaitForSeconds(0.25f);
        BatteryParent.SetActive(false);

        yield return new WaitForSeconds(0.25f);
        BatteryParent.SetActive(true);

        for (int i = 0; i < BatteryIcons.Length; i++)
        {
            BatteryIcons[i].SetActive(true);
            yield return new WaitForSeconds(1.55f);
        }
    }

    public IEnumerator SignalStartup()
    {
        yield return new WaitForSeconds(2f);
        SignalParent.SetActive(true);

        yield return new WaitForSeconds(0.25f);
        SignalParent.SetActive(false);

        yield return new WaitForSeconds(0.25f);
        SignalParent.SetActive(true);

        yield return new WaitForSeconds(0.25f);
        SignalParent.SetActive(false);

        yield return new WaitForSeconds(0.25f);
        SignalParent.SetActive(true);

        for (int i = 0; i < SignalIcons.Length; i++)
        {
            SignalIcons[i].SetActive(true);
            yield return new WaitForSeconds(1.55f);
        }
    }
}
