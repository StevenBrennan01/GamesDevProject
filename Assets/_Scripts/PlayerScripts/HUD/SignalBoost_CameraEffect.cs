using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SignalBoost_CameraEffect : MonoBehaviour
{
    private PlayerInputs playerInput;
    private BatteryManager batteryManager;

    [Tooltip("Cinemachine VCam for the placed head cam")]
    [SerializeField] private CinemachineCamera placedVirtualCamera;
    [SerializeField] private Volume globalVolume;

    [Header("Target Settings")]
    private float originalFOV;
    [SerializeField] private float targetFOV = 100f;
    private float originalBloom;
    [SerializeField] private float targetBloom = 5f;
    private float originalChromAbberation;
    [SerializeField] private float targetChromAbberation = 1f;

    [Header("Timings")]
    [SerializeField] private float returnDuration = .75f;

    private Bloom bloom;
    private ChromaticAberration chromaticAberration;
    private Coroutine signalBoostRoutine;
    private int signalCost = 2;
    
    private void Awake()
    {
        if(placedVirtualCamera != null)
            originalFOV = placedVirtualCamera.Lens.FieldOfView;

        if(globalVolume != null && globalVolume.profile != null)
        {
            if(globalVolume.profile.TryGet(out Bloom bloomOverride))
            {
                bloom = bloomOverride;
                originalBloom = bloom.intensity.value;
            }

            if(globalVolume.profile.TryGet<ChromaticAberration>(out ChromaticAberration chromaticAberrationOverride))
            {
                chromaticAberration = chromaticAberrationOverride;
                originalChromAbberation = chromaticAberration.intensity.value;
            }
        }

        playerInput = FindAnyObjectByType<PlayerInputs>();
        batteryManager = FindAnyObjectByType<BatteryManager>();
    }

    private void OnEnable()
    {
        playerInput.OnSignalBoost += TriggerSignalBoostEffect;
    }

    private void OnDisable()
    {
        playerInput.OnSignalBoost -= TriggerSignalBoostEffect;
    }

    public void TriggerSignalBoostEffect()
    {
        if(!batteryManager.DepleteBattery(signalCost)) // deplete battery cells as signal boost is triggered
        {
            Debug.Log("Not enough battery cells to trigger signal boost effect.");
            return;
        }

        signalBoostRoutine = StartCoroutine(SignalBoostEffectCoroutine());
    }

    private IEnumerator SignalBoostEffectCoroutine()
    {
        if (placedVirtualCamera != null)
        {
            originalFOV = placedVirtualCamera.Lens.FieldOfView;
            placedVirtualCamera.Lens.FieldOfView = targetFOV;
        } // set the bloom and chromatic aberration to their target values immediately

        if (bloom != null)
        {
            originalBloom = bloom.intensity.value;
            bloom.intensity.value = targetBloom;
        } // set the chromatic aberration to its target value immediately

        if (chromaticAberration != null)
        {
            originalChromAbberation = chromaticAberration.intensity.value;
            chromaticAberration.intensity.value = targetChromAbberation;
        } // set the chromatic aberration to its target value immediately

        float timer = 0f;

        float changedFOV = placedVirtualCamera != null ? placedVirtualCamera.Lens.FieldOfView : 0f; // uses the current, active FOV
        float changedBloom = bloom != null ? bloom.intensity.value : 0f; // uses the current, active bloom value
        float changedChromatic = chromaticAberration != null ? chromaticAberration.intensity.value : 0f; // uses the current, active chromatic value

        while (timer < returnDuration) // smoothly interpolate back to the original values over the return duration
        {
            timer += Time.deltaTime;
            float t = timer / returnDuration;
            t = Mathf.SmoothStep(0f, 1f, t); // Smooth interpolation

            if(placedVirtualCamera != null)
            placedVirtualCamera.Lens.FieldOfView = Mathf.Lerp(changedFOV, originalFOV, t);

            if(bloom != null)
                bloom.intensity.value = Mathf.Lerp(changedBloom, originalBloom, t);

            if(chromaticAberration != null)
                chromaticAberration.intensity.value = Mathf.Lerp(changedChromatic, originalChromAbberation, t);

            yield return null;
        }

        if(placedVirtualCamera != null)
            placedVirtualCamera.Lens.FieldOfView = originalFOV;

        if(bloom != null)
            bloom.intensity.value = originalBloom;

        if(chromaticAberration != null)
            chromaticAberration.intensity.value = originalChromAbberation;

        signalBoostRoutine = null;
    }
}