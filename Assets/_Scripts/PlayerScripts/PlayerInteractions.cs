using System.Collections;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    private PlayerInputs playerInput;
    private PlayerStateController playerState;

    [Tooltip("How long the player movement locked for when interacting, Interaction lock is found on the Interaction Volume")]
    [SerializeField, Range(0, 5)] private float lockMovementSeconds;

    [Header("Interaction Settings")]
    [Space(5)]
    [HideInInspector] public InteractionVolume activeZone = null;

    public bool canInteract = true;

    private void Awake()
    {
        if (playerInput == null) playerInput = GetComponent<PlayerInputs>();
        if (playerInput == null) Debug.LogError("PlayerInputs reference is missing / not found");

        if (playerState == null) playerState = GetComponent<PlayerStateController>();
        if (playerState == null) Debug.LogError("PlayerState reference is missing / not found");
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
        if (!canInteract) return;
        if (playerState.isBlending || playerInput.inputLocked) return;
        if (playerState.CurrentMovementMode != MovementMode.SecondPerson)
        {
            Debug.Log("You need to be in second-person to interact!");
            // Show some UI for this or something
            return;
        }

        activeZone.ExecuteInteraction(gameObject);
        StartCoroutine(LockInputDuringBlend(lockMovementSeconds)); // Pause player whilst interaction is happening
        StartCoroutine(DebounceThenBlock());
    }

    private IEnumerator LockInputDuringBlend(float lockSeconds)
    {
        playerInput.SetInputLocked(true);

        yield return new WaitForSeconds(lockSeconds);

        playerInput.SetInputLocked(false);
    }

    // Below coroutine is used to give the animator a split second to trigger the animation
    // then set canInteract to false. This is essential as the animation is guarded by if (!canInteract)
    private IEnumerator DebounceThenBlock()
    {
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(InteractBlocker(activeZone.cooldownSeconds));
    }

    private IEnumerator InteractBlocker(float blockSeconds)
    {
        canInteract = false;

        yield return new WaitForSeconds(blockSeconds);

        canInteract = true;
    }
}
