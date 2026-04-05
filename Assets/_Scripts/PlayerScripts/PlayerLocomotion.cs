using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotion : MonoBehaviour
{
    private PlayerInputs playerInput;
    private PlayerStateController playerState;
    private CharacterController controller;
    private Vector3 verticalVelocity;

    [Header("Movement Speeds")]
    [Space(10)]
    [SerializeField, Range(0, 5)] private float walkSpeed = 1f;
    [SerializeField, Range(0, 5)] private float crouchSpeedMultiplier = 0.6f;
    [SerializeField, Range(0, 5)] private float sprintSpeedMultiplier = 1.5f;

    [Header("Jumping")]
    [SerializeField] private float jumpVelocity = 3f;

    [Header("Gravity Values")]
    [Space(10)]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedGravityPushdown = 2f;

    [Header("Crouch Capsule")]
    [SerializeField] private float standHeight = 0.85f;
    [SerializeField] private float crouchHeight = 0.3f;
    [SerializeField] private float standCenterY = 0.9f;
    [SerializeField] private float crouchLerpSeconds = 0.12f;

    [Header("Ground Check Values")]
    [SerializeField] private float groundCheckOffset = 0.1f;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private float groundCheckDistance = 0.25f;
    [SerializeField] private float maxGroundSlope = 45f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ceiling Check Values")]
    [SerializeField] private float ceilingCheckOffset = 0.1f;
    [SerializeField] private float ceilingCheckRadius = 0.15f;
    [SerializeField] private float ceilingCheckDistance = 0.25f;
    [SerializeField] private LayerMask ceilingLayer;

    public bool isGrounded;
    public bool canStand;
    public bool shouldCrouch;
    private float crouchLerpT;

    // ---- Unity Event Functions ---- //
    //public event Action JumpSuccessful;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (playerInput == null) playerInput = GetComponent<PlayerInputs>();
        if (playerState == null) playerState = GetComponent<PlayerStateController>();

        if (playerInput == null) Debug.LogError("Reference to PlayerInputs is missing");
        if (playerState == null) Debug.LogError("Reference to PlayerStateController is missing");

        controller.height = standHeight;
        controller.center = new Vector3(controller.center.x, standCenterY, controller.center.z);
        crouchLerpT = 0f;
    }

    private void OnEnable()
    {
        playerInput.OnJump += OnJump;
    }

    private void OnDisable()
    {
        playerInput.OnJump -= OnJump;
    }

    private void Update()
    {
        //Debug.Log($"Current Speed: {(isGrounded ? (shouldCrouch ? walkSpeed * crouchSpeedMultiplier : (playerInput.isSprinting ? walkSpeed * sprintSpeedMultiplier : walkSpeed)) : "Airborne")}");

        GroundCheck();
        IsCeilingBlockingStand();

        Vector2 moveInput = playerInput.Move;
        Vector3 dir = playerState.ComputeMovementDirection(moveInput);

        dir = Vector3.ClampMagnitude(dir, 1f);

        float speed = walkSpeed;

        if (playerState.CurrentMovementMode == MovementMode.SecondPerson)
        {
            // ---- Crouching Logic ---- //
            bool forcedCrouch = !canStand;
            bool wishToCrouch = playerInput.isCrouching;

            shouldCrouch = forcedCrouch || wishToCrouch;

            UpdateCrouchCapsule(shouldCrouch);

            if (shouldCrouch)
            {
                speed *= crouchSpeedMultiplier;
            }

            if (!wishToCrouch && !forcedCrouch)
            {
                controller.height = standHeight;
                controller.center = new Vector3(controller.center.x, standCenterY, controller.center.z);
                crouchLerpT = 0f;
            }
        }
        else
        {
            UpdateCrouchCapsule(false);
        }

        // ---- Sprinting Logic ---- //
        bool wishToSprint = playerInput.isSprinting;

         if(!shouldCrouch && !playerInput.isCrouching)
         {
             if(wishToSprint)
            {
                    speed *= sprintSpeedMultiplier;
            }
        }
        
        Vector3 horizontalVelocity = dir * speed;

        UpdateVerticalVelocity();

        Vector3 finalVelocity = horizontalVelocity + verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void OnJump()
    {
        if (playerState.CurrentMovementMode != MovementMode.SecondPerson) return;
        if (playerInput.inputLocked) return;
        if (playerInput.isCrouching) return;

        if (isGrounded)
        {
            verticalVelocity.y = jumpVelocity;
        }

        //JumpSuccessful?.Invoke();
    }

    private void UpdateVerticalVelocity()
    {
        if (isGrounded)
        {
            // Keep a slight downward push to remain grounded; reset accumulated fall speed
            if (verticalVelocity.y <= 0f)
            {
                verticalVelocity.y = groundedGravityPushdown;
            }
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }
    }

    //private void ApplyVerticalGravityOnly()
    //{
    //    UpdateVerticalVelocity();
    //    controller.Move(verticalVelocity * Time.deltaTime);
    //}

    private void UpdateCrouchCapsule(bool crouching)
    {
        float targetT = crouching ? 1f : 0f;

        float lerpSpeed = Mathf.Approximately(crouchLerpSeconds, 0f)
            ? float.PositiveInfinity
            : (1f / Mathf.Max(crouchLerpSeconds, 0.0001f));

        crouchLerpT = Mathf.MoveTowards(crouchLerpT, targetT, lerpSpeed * Time.deltaTime);

        float targetHeight = Mathf.Lerp(standHeight, crouchHeight, crouchLerpT);

        // Compute centerY so the capsule bottom stays constant as height changes:
        // newCenterY = standCenterY - (standHeight - targetHeight)/2
        float targetCenterY = standCenterY - (standHeight - targetHeight) * 0.5f;

        controller.height = targetHeight;
        controller.center = new Vector3(controller.center.x, targetCenterY, controller.center.z);
    }

    public void IsCeilingBlockingStand()
    {
        Vector3 origin = transform.TransformPoint(controller.center);
        origin.y += groundCheckOffset;

        float radius = Mathf.Min(controller.radius - 0.02f, groundCheckRadius);
        float castDistance = groundCheckDistance;

        if (Physics.SphereCast(origin, radius, Vector3.up, out RaycastHit hit, castDistance, ceilingLayer, QueryTriggerInteraction.Ignore))
        {
            canStand = false;
        }
        else
        {
            canStand = true;
        }
    }

    private void GroundCheck()
    {
        Vector3 origin = transform.TransformPoint(controller.center);
        origin.y += -groundCheckOffset;

        float radius = Mathf.Min(controller.radius - 0.02f, groundCheckRadius);
        float castDistance = groundCheckDistance;

        if (Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, castDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle <= maxGroundSlope)
            {
                isGrounded = true;
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (controller == null) return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 origin = transform.TransformPoint(controller.center);
        origin.y += -groundCheckOffset;
        Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, Mathf.Min(controller.radius - 0.02f, groundCheckRadius));

        Gizmos.color = canStand ? Color.green : Color.red;
        origin.y += ceilingCheckOffset;
        Gizmos.DrawWireSphere(origin + Vector3.up * ceilingCheckDistance, Mathf.Min(controller.radius - 0.02f, ceilingCheckRadius));
    }
}