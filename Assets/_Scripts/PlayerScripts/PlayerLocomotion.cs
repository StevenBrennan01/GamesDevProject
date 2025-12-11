using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Gravity Values")]
    [Space(10)]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedGravityPushdown = 2f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (playerInput == null) playerInput = GetComponent<PlayerInputs>();
        if (playerState == null) playerState = GetComponent<PlayerStateController>();

        if (playerInput == null) Debug.LogError("Reference to PlayerInputs is missing");
        if (playerState == null) Debug.LogError("Reference to PlayerStateController is missing");
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
        if (playerInput.inputLocked || playerState.isBlending)
        {
            ApplyVerticalGravityOnly();
            return;
        }

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
        if (playerState.CurrentMovementMode == MovementMode.FirstPerson) return;
        if (playerState.isBlending ||playerInput.inputLocked) return;
        if (!controller.isGrounded) return;

        //jump logic 
    }

    private void UpdateVerticalVelocity()
    {
        if (controller.isGrounded)
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

    private void ApplyVerticalGravityOnly()
    {
        UpdateVerticalVelocity();
        controller.Move(verticalVelocity * Time.deltaTime);
    }
}