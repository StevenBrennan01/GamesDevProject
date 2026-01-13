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
    [SerializeField, Range(0, 5)] private float walkSpeed = 2f;
    [SerializeField, Range(0, 5)] private float crouchSpeedMultiplier = 0.6f;

    [Header("Jumping")]
    [SerializeField] private float jumpVelocity = 3f;

    [Header("Gravity Values")]
    [Space(10)]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedGravityPushdown = 2f;

    [Header("Crouch Capsule")]
    [SerializeField] private float standHeight = 0.85f;
    [SerializeField] private float crouchHeight = 0.4f;
    [SerializeField] private float standCenterY = 0.9f;
    [SerializeField] private float crouchCenterY = 0.6f;
    [SerializeField] private float crouchLerpSeconds = 0.12f;

    [Header("Ground Check Values")]
    [SerializeField] private float groundCheckOffset = 0.1f;
    [SerializeField] private float groundCheckRadius = 0.4f;
    [SerializeField] private float groundCheckDistance = 0.25f;
    [SerializeField] private float maxGroundSlope = 45f;
    [SerializeField] private LayerMask groundLayer;

    public bool isGrounded;

    private float crouchLerpT;

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
        UpdateCrouchCapsule(playerInput.isCrouching);

        GroundCheck();

        //if (playerInput.inputLocked || playerState.isBlending)
        //{
        //    ApplyVerticalGravityOnly();
        //    return;
        //}

        Vector2 moveInput = playerInput.Move;
        Vector3 dir = playerState.ComputeMovementDirection(moveInput);

        dir = Vector3.ClampMagnitude(dir, 1f);

        float speed = walkSpeed;

        if (playerInput.isCrouching)
        {
            speed *= crouchSpeedMultiplier;
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
        //float targetT = crouching ? 1f : 0f;

        //if (Mathf.Approximately(crouchLerpSeconds, 0f))
        //{
        //    crouchLerpT = targetT;
        //}
        //else
        //{
        //    float speed = 1f / Mathf.Max(crouchLerpSeconds, 0.0001f);
        //    crouchLerpT = Mathf.MoveTowards(crouchLerpT, targetT, speed * Time.deltaTime);
        //}

        //float height = Mathf.Lerp(standHeight, crouchHeight, crouchLerpT);
        //float centerY = Mathf.Lerp(standCenterY, crouchCenterY, crouchLerpT);
        //controller.height = height;
        //controller.center = new Vector3(controller.center.x, centerY, controller.center.z);
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
    }
}