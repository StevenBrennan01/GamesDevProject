using System.Collections;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    private PlayerInputs playerInput;
    private PlayerLocomotion locomotion;
    private PlayerStateController playerState;
    private PlayerInteractions interactions;

    [SerializeField] private Animator animator;
    //private CharacterController characterController;

    private static readonly int VelocityHash = Animator.StringToHash("Velocity");
    private static readonly int GroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int CrouchingHash = Animator.StringToHash("isCrouching");
    private static readonly int JumpHash = Animator.StringToHash("jumpTriggered");
    private static readonly int InteractHash = Animator.StringToHash("interactTriggered");
    private static readonly int HeadHash = Animator.StringToHash("headTriggered");

    [Tooltip("Smoothing time (seconds) for Velocity parameter damping.")]
    [SerializeField, Range(0f, 0.5f)] private float velocityDampTime = 0f;

    [Tooltip("Multiplier applied to velocity before sending to Animator.")]
    [SerializeField] private float velocityMultiplier = 1f;

    private void Awake()
    {
        playerInput = gameObject.GetComponentInParent<PlayerInputs>();
        locomotion = gameObject.GetComponentInParent<PlayerLocomotion>();
        playerState = gameObject.GetComponentInParent<PlayerStateController>();
        interactions = gameObject.GetComponentInParent<PlayerInteractions>();
    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.OnJump += HandleJumpAnim;
            playerInput.OnInteract += HandleInteractAnim;

            playerInput.OnTogglePlaceOrPickup += PlaceOrPickupAnim;
        }
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.OnJump -= HandleJumpAnim;
            playerInput.OnInteract -= HandleInteractAnim;

            playerInput.OnTogglePlaceOrPickup -= PlaceOrPickupAnim;
        }
    }

    private void Update()
    {
        if (playerInput == null || playerState == null) return;
        if (playerState.CurrentMovementMode != MovementMode.SecondPerson) return;

        animator.SetBool(CrouchingHash, playerInput.isCrouching);
        bool grounded = locomotion.isGrounded;
        animator.SetBool(GroundedHash, grounded);

        float velocity01 = ComputeVelocity();
        animator.SetFloat(VelocityHash, velocity01, velocityDampTime, Time.deltaTime);
    }

    private float ComputeVelocity()
    {
        float v = playerInput != null ? Mathf.Clamp01(playerInput.Move.magnitude) : 0f;

        //if (playerInput || characterController == null)
        //{
        //    // 0..1 based on input stick magnitude
        //    v = playerInput != null ? Mathf.Clamp01(playerInput.Move.magnitude) : 0f;
        //}
        //else // Can reduce as we will NOT be using the character controller magnitude for this purpose
        //{
        //    // world-space horizontal speed (scaled down to something usable)
        //    Vector3 hv = characterController.velocity;
        //    hv.y = 0f;
        //    v = hv.magnitude;
        //}

        return v * velocityMultiplier;
    }

    private void HandleJumpAnim()
    {
        if (playerState == null || playerInput == null || locomotion == null) return;

        if (playerState.CurrentMovementMode != MovementMode.SecondPerson) return;
        if (playerState.isBlending || playerInput.inputLocked) return;
        if (playerInput.isCrouching) return;
        if (!locomotion.isGrounded) return;

        animator.SetTrigger(JumpHash);
    }

    private void HandleInteractAnim()
    {
        if (playerState == null || playerInput == null || locomotion == null) return;

        if (!interactions.canInteract) return;
        if (playerState.CurrentMovementMode != MovementMode.SecondPerson) return;
        if (playerInput.isCrouching) return;
        if (!locomotion.isGrounded) return;
        if (interactions.activeZone == null) return;

        animator.SetTrigger(InteractHash);
    }

    private void PlaceOrPickupAnim()
    {
        if (playerState == null || playerInput == null || locomotion == null) return;

        if (playerInput.isCrouching) return;
        if (!locomotion.isGrounded) return;
        if (playerState.currentPlacementVolume != playerState.placedHeadVolume) return;

        StartCoroutine(DebounceAnimation());
    }

    // Coroutine waits for body to be activated in Controller
    // then plays animation, so to not animate nothing
    private IEnumerator DebounceAnimation()
    {
        yield return new WaitForSeconds(0.1f);

        animator.SetTrigger(HeadHash);
    }
}