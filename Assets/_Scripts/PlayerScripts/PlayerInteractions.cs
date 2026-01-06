using System.Collections;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    private PlayerInputs playerInput;
    private PlayerStateController playerState;

    [SerializeField, Range(0, 5)] private float playerLockSeconds;

    [Header("Interaction Settings")]
    [Space(5)]
    [HideInInspector] public InteractionVolume activeZone = null;

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
        if (playerState.isBlending || playerInput.inputLocked) return;
        if (playerState.CurrentMovementMode != MovementMode.SecondPerson)
        {
            Debug.Log("You need to be in second-person to interact!");
            // Show some UI for this or something
            return;
        }

        activeZone.ExecuteInteraction(gameObject);
        StartCoroutine(LockInputDuringBlend(playerLockSeconds)); // Pause player whilst interaction is happening
    }

    private IEnumerator LockInputDuringBlend(float lockSeconds)
    {
        playerInput.SetInputLocked(true);

        yield return new WaitForSeconds(lockSeconds);

        playerInput.SetInputLocked(false);
    }
}
