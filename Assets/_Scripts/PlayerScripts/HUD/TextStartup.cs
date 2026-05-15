using System.Collections;
using TMPro;
using UnityEngine;

public class TextStartupSequence : MonoBehaviour
{
    private HUDStartup hudStartup;

    [Header("-= SFX & Audio Sources =-")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip BootStartupSFX;
    [SerializeField, Range(1f, 5f)] private float audioFadeOutDuration = 2f;

    [Header("-= Startup Phases =-")]
    [SerializeField, Range(1f, 5f)] private float delayedStartupAmount = 2.25f;

    [System.Serializable]
    public class StartupPhase
    {
        public TMP_Text textComponent;

        [TextArea(3, 8)] public string message;

        [Min(15f)] public float charactersPerSecond = 18f;

        [Min(0f)] public float delayAfter = 0.5f;
    }

    [SerializeField] private StartupPhase[] phases;

    private void Awake()
    {
        this.gameObject.SetActive(true);
    }

    private void Start()
    {
        hudStartup = FindAnyObjectByType<HUDStartup>();
        
        StartCoroutine(PlayStartupSequence());
    }

    private IEnumerator PlayStartupSequence()
    {
        yield return new WaitForSeconds(delayedStartupAmount);
        audioSource.PlayOneShot(BootStartupSFX);

        yield return new WaitForSeconds(2f);

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
            if(i == phases.Length - 1)
                StartCoroutine(FadeOutStartupAudio(audioSource, audioFadeOutDuration));

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

    private IEnumerator FadeOutStartupAudio(AudioSource source, float audioFadeOutDuration)
    {
        if(source == null) yield break;

        float startVolume = source.volume;
        float elapsed = 0f;

        if(audioFadeOutDuration <= 0f)
        {
            source.volume = 0f;
            source.Stop();
            source.volume = startVolume;
            yield break;
        }

        float fadeRate = startVolume / audioFadeOutDuration;

        while (source.volume > 0f)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / audioFadeOutDuration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;

        this.gameObject.SetActive(false);
    }
}