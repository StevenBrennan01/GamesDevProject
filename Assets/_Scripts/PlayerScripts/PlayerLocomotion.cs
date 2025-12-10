using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotion : MonoBehaviour
{
    private PlayerInputs playerInputs;
    private PlayerStateController playerState;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    [Header("Movement Speeds")]
    [SerializeField, Range(0, 5)] private float walkSpeed = 4f;
    //[SerializeField, Range(0, 5)] private float secondPersonMoveSpeed = 4f;
    [SerializeField, Range(0, 5)] private float crouchSpeedMultiplier = 0.6f;

    [Header("Gravity Values")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedGravityPushdown = 2f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (playerInputs == null) playerInputs = GetComponent<PlayerInputs>();
        if (playerState == null) playerState = GetComponent<PlayerStateController>();

        if (playerInputs == null) Debug.LogError("Reference to PlayerInputs is missing");
        if (playerState == null) Debug.LogError("Reference to PlayerStateController is missing");
    }

    private void Update()
    {
        if (playerInputs.inputLocked || playerState.isBlending)
        {
            ApplyVerticalGravityOnly();
            return;
        }

        Vector2 moveInput = playerInputs.Move;
        Vector3 dir = playerState.ComputeMovementDirection(moveInput);

        dir = Vector3.ClampMagnitude(dir, 1f);

        float speed = walkSpeed;

        if (playerInputs.isCrouching)
        {
            speed *= crouchSpeedMultiplier;
        }

        Vector3 horizontalVelocity = dir * speed;

        UpdateVerticalVelocity();

        Vector3 finalVelocity = horizontalVelocity + verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void UpdateVerticalVelocity()
    {
        if (controller.isGrounded)
        {
            // Keep a slight downward push to remain grounded; reset accumulated fall speed
            verticalVelocity.y = groundedGravityPushdown;
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