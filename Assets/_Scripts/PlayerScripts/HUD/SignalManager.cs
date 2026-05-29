using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SignalManager : MonoBehaviour
{
    private PlayerStateController playerState;
    private SignalBoostController signalBoostController;
    private LevelLoadManager levelLoadManager;
    [SerializeField] private Volume globalVolume;

    [Header("-= Signal HUD Elements =-")]
    [Space(5)]
    [SerializeField] private GameObject signalParent;
    [SerializeField] private GameObject[] signalIcons;
    [SerializeField] private GameObject TimerParent;
    [SerializeField] private GameObject TimerFillImage;

     [Header("-= Signal Boost Values =-")]

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

    // [Header("-= Vignette Values =-")]
    // [Space(5)]
    // [SerializeField] private float originalVignetteIntensity = 0.385f; //0.385 intensity, 0.45 smoothness
    // [SerializeField] private float midLevelVignetteIntensity = 0.4f;
    // [SerializeField] private float highLevelVignetteIntensity = 0.425f;
    
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
    private Coroutine signalBoostCoroutine;
    public bool headSignalInitialized = false;
    private bool isSignalBoostActive = false;
    private bool signalChecksEnabled = false;

    private FilmGrain filmGrain;

    private void Awake()
    {
        if (playerState == null) playerState = FindAnyObjectByType<PlayerStateController>();
        if (signalBoostController == null) signalBoostController = FindAnyObjectByType<SignalBoostController>();
        if (levelLoadManager == null) levelLoadManager = FindAnyObjectByType<LevelLoadManager>();
        if (globalVolume == null) globalVolume = FindAnyObjectByType<Volume>();

        signalChecksEnabled = false;

        if(globalVolume != null && globalVolume.profile != null)
        {
            if(globalVolume.profile.TryGet(out FilmGrain filmGrainOverride))
            {
                filmGrain = filmGrainOverride;
                originalFilmGrainIntensity = filmGrain.intensity.value;
                originalFilmGrainResponse = filmGrain.response.value;
            }
        }

        TimerParent.SetActive(false);
        TimerFillImage.SetActive(false);
        isSignalBoostActive = false;
    }

    private void Update()
    {
        if (playerState == null) return;
        if (playerState.placedHeadVolume == null) return;
        if (signalBoostController == null) return;
        if (globalVolume == null) return;
        if (!signalChecksEnabled) return;

        headLocation = playerState.placedHeadVolume.transform;
        float distanceToHead = Vector3.Distance(playerLocation.position, headLocation.position);

        if(isSignalBoostActive)
        {
            SetSignalIcons(3);
            ApplyBoostedSignalPostFX();
            return;
        }

        ApplySignalPostFXByDistance(distanceToHead);

        int targetSignalLevel = GetSignalLevelFromDistance(distanceToHead);

        if(!headSignalInitialized)
        {
            currentSignalLevel = targetSignalLevel;
            SetSignalIcons(currentSignalLevel);
            // ApplySignalPostFX(targetSignalLevel);
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

    public void IncreaseSignalLevelForDuration(float duration)
    {
        if(signalBoostCoroutine != null)
        {
            StopCoroutine(signalBoostCoroutine);
        }

        signalBoostCoroutine = StartCoroutine(TemporaryMaxSignalBoost(duration));
    }

    public void EnableSignalChecks()
    {
        headSignalInitialized = false;
        signalChecksEnabled = true;
    }

    public void DisableSignalChecks()
    {
        signalChecksEnabled = false;

        if (noSignalFlickerCoroutine != null)
        {
            StopCoroutine(noSignalFlickerCoroutine);
            noSignalFlickerCoroutine = null;
        }

        // if (signalParent != null)
        // {
        //     signalParent.SetActive(false);
        // }

        // currentSignalLevel = 3;
        // SetSignalIcons(3);
        // ApplyBoostedSignalPostFX();
    }

    private IEnumerator TemporaryMaxSignalBoost(float duration)
    {
        isSignalBoostActive = true;
        
        TimerParent.SetActive(true);
        TimerFillImage.SetActive(true);

        var timerFillImageComponent = TimerFillImage.GetComponent<UnityEngine.UI.Image>();
        timerFillImageComponent.fillAmount = 1f;

        if(signalChangeCoroutine != null)
        {
            StopCoroutine(signalChangeCoroutine);
        }
        if(noSignalFlickerCoroutine != null)
        {
            StopCoroutine(noSignalFlickerCoroutine);
            noSignalFlickerCoroutine = null;

            if (signalParent != null)
            {
                signalParent.SetActive(true);
            }
        }

        currentSignalLevel = 3;
        SetSignalIcons(currentSignalLevel);
        ApplyBoostedSignalPostFX();

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float fillAmount = Mathf.Lerp(1f, 0f, timer / duration);
            TimerFillImage.GetComponent<UnityEngine.UI.Image>().fillAmount = fillAmount;

            yield return null;
        }

        isSignalBoostActive = false;

        timerFillImageComponent.fillAmount = 0f;

        TimerParent.SetActive(false);
        TimerFillImage.SetActive(false);
        
        signalBoostCoroutine = null;
    }

    private void ApplyBoostedSignalPostFX()
    {
        if (filmGrain != null)
        {
            filmGrain.intensity.value = originalFilmGrainIntensity;
            filmGrain.response.value = originalFilmGrainResponse;
        }
    }

    private IEnumerator FlickerWholeSignalIcon()
    {
        if(signalParent != null)
        {
            yield return new WaitForSeconds(1.25f);

            float startTime = Time.time;
            float maxDurationBeforeReset = 3f;

            while (true)
            {
                if (Time.time - startTime >= maxDurationBeforeReset)
                {
                    levelLoadManager.ReloadCurrentLevel();
                    yield break;
                }

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

            // signalIcons[flickerIndex].SetActive(true);
            // yield return new WaitForSeconds(0.1f);

            // signalIcons[flickerIndex].SetActive(false);
            // PlaySignalSfx(ringGlitchSFX);
            // yield return new WaitForSeconds(0.1f);
        }
        else
        {
            PlaySignalSfx(ringGlitchSFX);
            yield return new WaitForSeconds(0.1f);
        }

        SetSignalIcons(targetSignalLevel);
        signalChangeCoroutine = null;
    }

    private void ApplySignalPostFXByDistance(float distanceToHead)
    {
        float t = Mathf.InverseLerp(minSignalDistance, maxSignalDistance, distanceToHead);
        t = Mathf.SmoothStep(0f, 1f, t); // test with and withoout smoothstep to see if it looks better with a curve or linear

        if(filmGrain != null)
        {
            filmGrain.intensity.value = Mathf.Lerp(originalFilmGrainIntensity, highLevelFilmGrainIntensity, t);
            filmGrain.response.value = Mathf.Lerp(originalFilmGrainResponse, highLevelFilmGrainResponse, t);
        }
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