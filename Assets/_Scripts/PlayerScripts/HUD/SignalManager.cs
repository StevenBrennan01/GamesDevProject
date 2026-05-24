using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SignalManager : MonoBehaviour
{
    private PlayerStateController playerState;
    private SignalBoostController globalVolumeManager;
    [SerializeField] private Volume globalVolume;

    [Header("-= Signal HUD Elements =-")]
    [Space(5)]
    [SerializeField] private GameObject signalParent;
    [SerializeField] private GameObject[] signalIcons;

    [Header("-= Signal Distance Values =-")]
    [Space(5)]
    [SerializeField] private float maxSignalDistance = 10f;
    [SerializeField] private float midSignalDistance2 = 8f;
    [SerializeField] private float midSignalDistance = 6f;
    [SerializeField] private float minSignalDistance = 3f;

    [Header("-= Film Grain Intensity Values =-")]
    [Space(5)]
    [SerializeField] private float originalFilmGrainIntensity = 0.15f; //0.15 original intensity
    [SerializeField] private float midLevelFilmGrainIntensity;
    [SerializeField] private float highLevelFilmGrainIntensity;

    [Header("-= Film Grain Response Values =-")]
    [Space(5)]
    [SerializeField] private float originalFilmGrainResponse = 0.6f; //0.6 original response
    [SerializeField] private float midLevelFilmGrainResponse;
    [SerializeField] private float highLevelFilmGrainResponse;

    [Header("-= Vignette Values =-")]
    [Space(5)]
    [SerializeField] private float originalVignetteIntensity = 0.385f; //0.385 intensity, 0.45 smoothness
    [SerializeField] private float midLevelVignetteIntensity = 0.4f;
    [SerializeField] private float highLevelVignetteIntensity = 0.425f;
    
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
    private Coroutine noSignalFlickerCoroutine;
    public bool headSignalInitialized = false;

    private FilmGrain filmGrain;
    private Vignette vignette;

    private void Awake()
    {
        if (playerState == null) playerState = FindAnyObjectByType<PlayerStateController>();
        if (globalVolumeManager == null) globalVolumeManager = FindAnyObjectByType<SignalBoostController>();
        if (globalVolume == null) globalVolume = FindAnyObjectByType<Volume>();

        if(globalVolume != null && globalVolume.profile != null)
        {
            if(globalVolume.profile.TryGet(out FilmGrain filmGrainOverride))
            {
                filmGrain = filmGrainOverride;
                originalFilmGrainIntensity = filmGrain.intensity.value;
                originalFilmGrainResponse = filmGrain.response.value;
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

            if (targetSignalLevel == 0)
            {
                if (noSignalFlickerCoroutine == null)
                {
                    noSignalFlickerCoroutine = StartCoroutine(FlickerWholeSignalIcon());
                }
            }
            else
            {
                if (noSignalFlickerCoroutine != null)
                {
                    StopCoroutine(noSignalFlickerCoroutine);
                    noSignalFlickerCoroutine = null;

                    if (signalParent != null)
                    {
                        signalParent.SetActive(true);
                    }
                }
            }

            signalChangeCoroutine = StartCoroutine(SwitchSignalLevel(previousSignalLevel, targetSignalLevel));
        }
    }

    private int GetSignalLevelFromDistance(float distanceToHead)
    {
        // if (distanceToHead <= minSignalDistance)
        // {
        //     Debug.Log("Full Signal");
        //     return 4; // Full Signal
        // }
        // else if (distanceToHead > minSignalDistance && distanceToHead <= midSignalDistance)
        // {
        //     Debug.Log("Medium Signal");
        //     return 3; // Medium Signal
        // }
        // else if (distanceToHead > midSignalDistance && distanceToHead <= midSignalDistance2)
        // {
        //     Debug.Log("Low Signal");
        //     return 2; // Low Signal
        // }
        // else if (distanceToHead > midSignalDistance2 && distanceToHead <= maxSignalDistance)
        // {
        //     Debug.Log("Almost no Signal");
        //     return 1; // No Signal
        // }
        // else    
        // {
        //     Debug.Log("No Signal");
        //     return 0; // No Signal
        // }   

        if (distanceToHead <= minSignalDistance)
        {
            Debug.Log("Full Signal");
            return 3; // Full Signal
        }
        else if (distanceToHead > minSignalDistance && distanceToHead <= midSignalDistance)
        {
            Debug.Log("Medium Signal");
            return 2; // Medium Signal
        }
        else if (distanceToHead > midSignalDistance && distanceToHead <= maxSignalDistance)
        {
            Debug.Log("Low Signal");
            return 1; // Low Signal
        }
        else    
        {
            Debug.Log("No Signal");
            return 0; // No Signal
        }   
    }

    private IEnumerator FlickerWholeSignalIcon()
    {
        if(signalParent != null)
        {
            yield return new WaitForSeconds(1.5f);

            while (true)
            {
                signalParent.SetActive(false);
                PlaySignalSfx(ringGlitchSFX);
                yield return new WaitForSeconds(0.2f);

                signalParent.SetActive(true);
                yield return new WaitForSeconds(0.2f);
            }
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

        if(flickerIndex >= 0 && flickerIndex < signalIcons.Length)
        {
            signalIcons[flickerIndex].SetActive(false);
            PlaySignalSfx(ringGlitchSFX);
            yield return new WaitForSeconds(0.1f);

            signalIcons[flickerIndex].SetActive(true);
            yield return new WaitForSeconds(0.1f);

            signalIcons[flickerIndex].SetActive(false);
            PlaySignalSfx(ringGlitchSFX);
            yield return new WaitForSeconds(0.1f);

            signalIcons[flickerIndex].SetActive(true);
            yield return new WaitForSeconds(0.1f);

            signalIcons[flickerIndex].SetActive(false);
            PlaySignalSfx(ringGlitchSFX);
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            PlaySignalSfx(ringGlitchSFX);
            yield return new WaitForSeconds(0.1f);
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
                    filmGrain.response.value = originalFilmGrainResponse;
                    break;
                case 2:
                    filmGrain.intensity.value = midLevelFilmGrainIntensity;
                    filmGrain.response.value = midLevelFilmGrainResponse;
                    break;
                case 1:
                    filmGrain.intensity.value = highLevelFilmGrainIntensity;
                    filmGrain.response.value = highLevelFilmGrainResponse;
                    break;
                case 0:
                    filmGrain.intensity.value = highLevelFilmGrainIntensity + 0.1f; // Extra grain for no signal
                    filmGrain.response.value = highLevelFilmGrainResponse + 0.1f;
                    break;
            }
        }

        // if(vignette != null)
        // {
        //     switch (signalLevel)
        //     {
        //         case 3:
        //             vignette.intensity.value = originalVignetteIntensity;
        //             break;
        //         case 2:
        //             vignette.intensity.value = midLevelVignetteIntensity;
        //             break;
        //         case 1:
        //             vignette.intensity.value = highLevelVignetteIntensity;
        //             break;
        //         case 0:
        //             vignette.intensity.value = highLevelVignetteIntensity;
        //             break;
        //     }
        // }
    }

    private void SetSignalIcons(int activeCount)
    {
        for (int i = 0; i < signalIcons.Length; i++)
        {
            signalIcons[i].SetActive(i < activeCount);
        }
    }

    private void PlaySignalSfx(AudioClip clip)
    {
        if (ringAudioSource == null || clip == null) return;
        ringAudioSource.PlayOneShot(clip);
    }
}