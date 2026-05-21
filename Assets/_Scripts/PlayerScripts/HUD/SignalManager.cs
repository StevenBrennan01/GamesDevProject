using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SignalManager : MonoBehaviour
{
    private PlayerStateController playerState;
    private GlobalVolumeManager globalVolumeManager;
    [SerializeField] private Volume globalVolume;

    [Header("-= Signal HUD Elements =-")]
    [Space(5)]
    [SerializeField] private GameObject SignalParent;
    [SerializeField] private GameObject[] SignalIcons;

    [Header("-= Signal Distance Values =-")]
    [Space(5)]
    [SerializeField] private float maxSignalDistance = 25f;
    [SerializeField] private float midSignalDistance = 15f;
    [SerializeField] private float minSignalDistance = 7.5f;

    [Header("-= Film Grain Values =-")]
    [Space(5)]
    [SerializeField] private float originalFilmGrainIntensity = 0.15f; //0.15 intensity, 0.6 response
    [SerializeField] private float midLevelFilmGrainIntensity = 0.2f;
    [SerializeField] private float highLevelFilmGrainIntensity = 0.3f;

    [Header("-= Vignette Values =-")]
    [Space(5)]
    [SerializeField] private float originalVignetteIntensity = 0.385f; //0.385 intensity, 0.45 smoothness
    [SerializeField] private float midLevelVignetteIntensity = 0.45f;
    [SerializeField] private float highLevelVignetteIntensity = 0.525f;
    
    [Header("-= SFX & Audio Sources =-")]
    [SerializeField] private AudioSource ringAudioSource;
    [SerializeField] private AudioSource staticAudioSource;
    [SerializeField] private AudioClip ringChangeSFX;
    [SerializeField] private AudioClip ringGlitchSFX;
    [Space(5)]

    [Header("-= Comparison Transforms // PlayerPos vs HeadPos (HeadPos is Private) =-")]
    [SerializeField] private Transform playerLocation;
    private Transform headLocation;

    [Header("-= Other References =-")]
    private int currentSignalLevel = -1;
    private Coroutine signalChangeCoroutine;
    private bool headSignalInitialized = false;

    private FilmGrain filmGrain;
    private Vignette vignette;

    private void Awake()
    {
        if (playerState == null) playerState = FindAnyObjectByType<PlayerStateController>();
        if (globalVolumeManager == null) globalVolumeManager = FindAnyObjectByType<GlobalVolumeManager>();
        if (globalVolume == null) globalVolume = FindAnyObjectByType<Volume>();

        if(globalVolume != null && globalVolume.profile != null)
        {
            if(globalVolume.profile.TryGet(out FilmGrain filmGrainOverride))
            {
                filmGrain = filmGrainOverride;
                originalFilmGrainIntensity = filmGrain.intensity.value;
            }
            if(globalVolume.profile.TryGet(out Vignette vignetteOverride))
            {
                vignette = vignetteOverride;
                originalVignetteIntensity = vignette.intensity.value;
            }
        }
    }

    private void Update()
    {
        if (playerState == null) return;
        if (playerState.placedHeadVolume == null) return;
        if (globalVolumeManager == null) return;
        if (globalVolume == null) return;

        headLocation = playerState.placedHeadVolume.transform;
        float distanceToHead = Vector3.Distance(playerLocation.position, headLocation.position);

        int targetSignalLevel = GetSignalLevelFromDistance(distanceToHead);

        if(!headSignalInitialized)
        {
            currentSignalLevel = targetSignalLevel;
            SetSignalIcons(currentSignalLevel);
            ApplySignalPostFX(targetSignalLevel);
            headSignalInitialized = true;
            return;
        }

        if(targetSignalLevel != currentSignalLevel)
        {
            int previousSignalLevel = currentSignalLevel;
            currentSignalLevel = targetSignalLevel;

            if(signalChangeCoroutine != null)
            {
                StopCoroutine(signalChangeCoroutine);
            }

            signalChangeCoroutine = StartCoroutine(SwitchSignalLevel(previousSignalLevel, targetSignalLevel));
        }
    }

    private int GetSignalLevelFromDistance(float distanceToHead)
    {
        if (distanceToHead <= minSignalDistance)
        {
            return 3; // Full Signal
        }
        else if (distanceToHead <= midSignalDistance)
        {
            return 2; // Medium Signal
        }
        else if (distanceToHead <= maxSignalDistance)
        {
            return 1; // Low Signal
        }
        else
        {
            return 0; // No Signal
        }
    }

    private IEnumerator SwitchSignalLevel(int previousSignalLevel, int targetSignalLevel)
    {
        int flickerIndex = -1;

        if(targetSignalLevel > previousSignalLevel)
        {
            flickerIndex = targetSignalLevel - 1;
            SetSignalIcons(targetSignalLevel);
        }
        else if(targetSignalLevel < previousSignalLevel)
        {
            flickerIndex = targetSignalLevel; //- 1;
            SetSignalIcons(targetSignalLevel);
        }
        else
        {
            SetSignalIcons(targetSignalLevel);
        }

        if(flickerIndex >= 0 && flickerIndex < SignalIcons.Length)
        {
            SignalIcons[flickerIndex].SetActive(false);
            PlaySignalSfx(ringGlitchSFX);
            yield return new WaitForSeconds(0.2f);

            SignalIcons[flickerIndex].SetActive(true);
            yield return new WaitForSeconds(0.2f);

            SignalIcons[flickerIndex].SetActive(false);
            PlaySignalSfx(ringGlitchSFX);
            yield return new WaitForSeconds(0.2f);

            SignalIcons[flickerIndex].SetActive(true);
            yield return new WaitForSeconds(0.2f);

            SignalIcons[flickerIndex].SetActive(false);
            PlaySignalSfx(ringGlitchSFX);
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            PlaySignalSfx(ringGlitchSFX);
            yield return new WaitForSeconds(0.2f);
        }

        SetSignalIcons(targetSignalLevel);
        ApplySignalPostFX(targetSignalLevel);
        signalChangeCoroutine = null;
    }

    private void ApplySignalPostFX(int signalLevel)
    {
        if(filmGrain != null)
        {
            switch (signalLevel)
            {
                case 3:
                    filmGrain.intensity.value = originalFilmGrainIntensity;
                    break;
                case 2:
                    filmGrain.intensity.value = midLevelFilmGrainIntensity;
                    break;
                case 1:
                    filmGrain.intensity.value = highLevelFilmGrainIntensity;
                    break;
                case 0:
                    filmGrain.intensity.value = highLevelFilmGrainIntensity + 0.1f; // Extra grain for no signal
                    break;
            }
        }

        if(vignette != null)
        {
            switch (signalLevel)
            {
                case 3:
                    vignette.intensity.value = originalVignetteIntensity;
                    break;
                case 2:
                    vignette.intensity.value = midLevelVignetteIntensity;
                    break;
                case 1:
                    vignette.intensity.value = highLevelVignetteIntensity;
                    break;
                case 0:
                    vignette.intensity.value = highLevelVignetteIntensity + 0.05f; // Extra vignette for no signal
                    break;
            }
        }
    }

    private void SetSignalIcons(int activeCount)
    {
        for (int i = 0; i < SignalIcons.Length; i++)
        {
            SignalIcons[i].SetActive(i < activeCount);
        }
    }

    private void PlaySignalSfx(AudioClip clip)
    {
        if (ringAudioSource == null || clip == null) return;
        ringAudioSource.PlayOneShot(clip);
    }
}