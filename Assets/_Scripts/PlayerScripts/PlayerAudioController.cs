using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    [Header("Script References")]
    private PlayerStateController playerState;
    private PlayerLocomotion playerLocomotion;
    private AnimatorController playerAnimatorController;
    private PlayerInputs playerInputs;
    private BatteryManager batteryManager;

    [Header("Movement Audio Sources")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource interactionSource;
    [SerializeField] private AudioSource localSource;

    [Header("Movement Audio Clips")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip headInteractClip;
    [SerializeField] private AudioClip leverPullClip;
    [SerializeField] private AudioClip jumpUpClip;
    [SerializeField] private AudioClip jumpLandingClip;

    [Header("Interactions/Ability Audio Clips")]
    [SerializeField] private AudioClip signalBoostClip;

    [Header("Footstep Audio Settings")]
    [SerializeField, Range(0, 0.25f)] private float movementThreshold = 0.1f;
    [SerializeField] private float firstPersonStepInterval = 0.45f;
    
    private float stepTimer;

    private void Awake()
    {
        if (playerState == null) playerState = GetComponent<PlayerStateController>();
        if (playerLocomotion == null) playerLocomotion = GetComponent<PlayerLocomotion>();
        if (playerAnimatorController == null) playerAnimatorController = GetComponent<AnimatorController>();
        if (playerInputs == null) playerInputs = GetComponent<PlayerInputs>();
        if (batteryManager == null) batteryManager = FindAnyObjectByType<BatteryManager>();
    }

    private void Update()
    {
        if(!ShouldAutoStep())
        {
            stepTimer = 0f;
            return;
        }

        stepTimer += Time.deltaTime;

        if(stepTimer > firstPersonStepInterval)
        {
            stepTimer = 0f;
            PlayFootStep();
        }
    }

    // --- || Footstep Audio Methods || ---
    private bool ShouldAutoStep()
    {
        if(playerInputs == null || playerLocomotion == null || playerState == null) return false;
        if(playerState.CurrentMovementMode != MovementMode.FirstPerson) return false;
        if(playerInputs.movementLocked) return false;
        if(!playerLocomotion.isGrounded) return false;

        return playerInputs.Move.sqrMagnitude > movementThreshold;
    }

    public void PlayFootStep()
    {
        if(footstepSource == null) return;
        if(footstepClips == null || footstepClips.Length == 0) return;

        int index = Random.Range(0, footstepClips.Length);
        footstepSource.PlayOneShot(footstepClips[index]);
    }


    // --- || Jumping Audio Methods || ---
    public void PlayJumpUpSFX()
    {
        interactionSource.PlayOneShot(jumpUpClip);
    }

    public void PlayJumpLandingSFX()
    {
        interactionSource.PlayOneShot(jumpLandingClip);
    }

    // --- || Interacting Audio Methods || ---
    public void PlayHeadInteractSFX()
    {
        interactionSource.PlayOneShot(headInteractClip);
    }

    public void PlayLeverPullSFX()
    {
        interactionSource.PlayOneShot(leverPullClip);
    }

    // Signal Boost SFX / Still technically an interaction
    public void PlaySignalBoostSFX()
    {
        localSource.PlayOneShot(signalBoostClip);
    }
}