using System.Collections;
using TMPro;
using UnityEngine;

public class TextStartupSequence : MonoBehaviour
{
    private HUDStartup hudStartup;

    [System.Serializable]
    public class StartupPhase
    {
        public TMP_Text textComponent;

        [TextArea(3, 8)] public string message;

        [Min(15f)] public float charactersPerSecond = 18f;

        [Min(0f)] public float delayAfter = 0.5f;
    }

    [SerializeField] private StartupPhase[] phases;

    private void Start()
    {
        hudStartup = FindAnyObjectByType<HUDStartup>();

        StartCoroutine(PlayStartupSequence());
    }

    private IEnumerator PlayStartupSequence()
    {
        for (int i = 0; i < phases.Length; i++)
        {
            StartupPhase phase = phases[i];
            if (phase.textComponent == null)
            {
                Debug.Log("Startup Code Phase is missing a text component reference. Skipping this phase.");
                break;
            }

            phase.textComponent.gameObject.SetActive(true);
            phase.textComponent.text = phase.message;
            phase.textComponent.maxVisibleCharacters = 0;
            phase.textComponent.ForceMeshUpdate();

            if(i == 1)
                StartCoroutine(hudStartup.BatteryStartup());
            else if(i == 2)
                StartCoroutine(hudStartup.SignalStartup());

            yield return StartCoroutine(TypeText(phase.textComponent, phase.charactersPerSecond));

            yield return new WaitForSeconds(phase.delayAfter);

            phase.textComponent.gameObject.SetActive(false);
        }
    }

    private IEnumerator TypeText(TMP_Text textComponent, float charactersPerSecond)
    {
        int totalCharacters = textComponent.textInfo.characterCount;
        float delay = 1f / charactersPerSecond;

        for (int i = 0; i <= totalCharacters; i++)
        {
            textComponent.maxVisibleCharacters = i;
            yield return new WaitForSeconds(delay);
        }
    }
}