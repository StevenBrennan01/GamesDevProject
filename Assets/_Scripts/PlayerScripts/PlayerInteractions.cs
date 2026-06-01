using System.Collections;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    private PlayerInputs playerInput;
    private PlayerStateController playerState;
    private PlayerLocomotion playerLocomotion;
    private BatteryManager batteryManager;
    
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip interactNotAllowedSFX;

    [Tooltip("How long the player movement locked for when interacting, Interaction lock is found on the Interaction Volume")]
    [SerializeField, Range(0, 5)] private float lockMovementSeconds;

    [Header("Interaction Settings")]
    [Space(5)]
    public InteractionVolume activeZone = null;

    private void Awake()
    {
        if (playerInput == null) playerInput = GetComponent<PlayerInputs>();
        if (playerInput == null) Debug.LogError("PlayerInputs reference is missing / not found");

        if (playerState == null) playerState = GetComponent<PlayerStateController>();
        if (playerState == null) Debug.LogError("PlayerState reference is missing / not found");

        if (playerLocomotion == null) playerLocomotion = GetComponent<PlayerLocomotion>();
        if (playerLocomotion == null) Debug.LogError("PlayerLocomotion reference is missing / not found");

        if (batteryManager == null) batteryManager = FindAnyObjectByType<BatteryManager>();
        if (batteryManager == null) Debug.LogError("BatteryManager reference is missing / not found");
    }

    private void OnEnable()
    {
        playerInput.OnInteract += TryInteract;
    }

    private void OnDisable()
    {
        playerInput.OnInteract -= TryInteract;
    }

    public void SetCurrentZone(InteractionVolume currentZone)
    {
        activeZone = currentZone;
    }

    public void ClearCurrentZone(InteractionVolume currentZone)
    {
        if (activeZone == currentZone)
        {
            activeZone = null;
        }
    }

    private void TryInteract()
    {
        if (activeZone == null) return;
        if (!activeZone.canPull) return;
        if (playerState.isBlending || playerInput.movementLocked) return;
        if (!playerLocomotion.isGrounded) return;
        if (playerState.CurrentMovementMode != MovementMode.SecondPerson)
        {
            if (audioSource != null && interactNotAllowedSFX != null)
            {
                audioSource.PlayOneShot(interactNotAllowedSFX);
            }
            return;
        }

        if (playerState.placedHeadVolume != null && playerState.placedHeadVolume.isHeadCharger)
        {
            if(activeZone == null || !activeZone.IsHeadChargerInteraction)
            {
                if(audioSource != null && interactNotAllowedSFX != null)
                {
                    audioSource.PlayOneShot(interactNotAllowedSFX);
                }
                Debug.Log("You need to be in the head charger interaction zone to interact with the head charger!");
                return;
            }
        }

        if (playerState.placedHeadVolume != null && !playerState.placedHeadVolume.isHeadCharger)
        {
            if (activeZone == null || activeZone.IsHeadChargerInteraction)
            {
                if (audioSource != null && interactNotAllowedSFX != null)
                {
                    audioSource.PlayOneShot(interactNotAllowedSFX);
                }
                Debug.Log("You need to be in the head placement interaction zone to interact with the head placement!");
                return;
            }
        }
        
        if(playerState.placedHeadVolume.isHeadCharger && activeZone != null && activeZone.IsHeadChargerInteraction)
        {
            activeZone.ExecuteInteraction(gameObject);
            StartCoroutine(LockInputDuringInteraction(lockMovementSeconds)); // Pause player whilst interaction is happening
            return;
        }

        StartCoroutine(DebounceAndDepleteBattery());

        activeZone.ExecuteInteraction(gameObject);
        StartCoroutine(LockInputDuringInteraction(lockMovementSeconds));
    }

    private IEnumerator DebounceAndDepleteBattery()
    {
        yield return new WaitForSeconds(0.3f); // Short debounce to allow interact animation to trigger, then attempt battery depletion

        if (!batteryManager.DepleteBattery(1))
        {
            if (audioSource != null && interactNotAllowedSFX != null)
            {
                audioSource.PlayOneShot(interactNotAllowedSFX);
            }
        }
    }

    private IEnumerator LockInputDuringInteraction(float lockSeconds)
    {
        playerInput.SetMovementLocked(true);

        yield return new WaitForSeconds(lockSeconds);

        playerInput.SetMovementLocked(false);
    }
}
