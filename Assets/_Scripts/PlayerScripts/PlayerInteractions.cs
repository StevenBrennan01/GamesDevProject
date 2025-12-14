using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    private PlayerInputs playerInput;
    private PlayerStateController playerState;

    private InteractionVolume activeZone;

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
        if (playerState.CurrentMovementMode != MovementMode.SecondPerson) return;
        if (playerState.isBlending || playerInput.inputLocked) return;

        activeZone.ExecuteInteraction(gameObject);
    }
}
