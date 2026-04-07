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
    public InteractionVolume activeZone = null;

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
        //if (!canInteract) return;
        if (!activeZone.canPull) return;
        if (playerState.isBlending || playerInput.movementLocked) return;
        if (playerState.CurrentMovementMode != MovementMode.SecondPerson)
        {
            Debug.Log("You need to be in second-person to interact!");
            return;
        }

        activeZone.ExecuteInteraction(gameObject);
        StartCoroutine(LockInputDuringInteraction(lockMovementSeconds)); // Pause player whilst interaction is happening
    }

    private IEnumerator LockInputDuringInteraction(float lockSeconds)
    {
        playerInput.SetMovementLocked(true);

        yield return new WaitForSeconds(lockSeconds);

        playerInput.SetMovementLocked(false);
    }
}
