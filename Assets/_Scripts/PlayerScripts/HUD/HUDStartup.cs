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

    private IEnumerator BatteryStartupCoroutine;
    private IEnumerator SignalStartupCoroutine;

    void Awake()
    {
        BatteryParent.SetActive(false);
        SignalParent.SetActive(false);

        //BatteryStartupCoroutine = BatteryStartup();
        //SignalStartupCoroutine = SignalStartup();
    }

    void Start()
    {
        //StartCoroutine(BatteryStartupCoroutine);
    }

    public IEnumerator BatteryStartup()
    {
        yield return new WaitForSeconds(3f);
        BatteryParent.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        BatteryParent.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        BatteryParent.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        BatteryParent.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        BatteryParent.SetActive(true);

        for (int i = 0; i < BatteryIcons.Length; i++)
        {
            BatteryIcons[i].SetActive(true);
            yield return new WaitForSeconds(0.5f);
        }

        //StartCoroutine(SignalStartupCoroutine);
    }

    public IEnumerator SignalStartup()
    {
        yield return new WaitForSeconds(1f);
        SignalParent.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        SignalParent.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        SignalParent.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        SignalParent.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        SignalParent.SetActive(true);

        for (int i = 0; i < SignalIcons.Length; i++)
        {
            SignalIcons[i].SetActive(true);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
