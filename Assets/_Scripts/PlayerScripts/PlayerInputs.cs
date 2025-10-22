using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    [Header("Input References")]
    [Space(10)]

    [Tooltip("Vector2 - WASD / Left Thumb Stick")]
    [SerializeField] private InputActionReference moveAction;

    [Tooltip("Vector2 - Mouse Delta / Right Thumb Stick")]
    [SerializeField] private InputActionReference lookAction;

    [Tooltip("Button - Jump")]
    [SerializeField] private InputActionReference jumpAction;

    [Tooltip("Button - Crouch")]
    [SerializeField] private InputActionReference crouchAction;

    [Tooltip("Button - Interact")]
    [SerializeField] private InputActionReference interactAction;

    [Tooltip("Button - Head placed / Perspective changed")]
    [SerializeField] private InputActionReference togglePlaceOrPickupAction;

    [Header("Settings")]
    [Tooltip("Scales the look input (mouse/stick) before consumers read it.")]
    [SerializeField] private Vector2 lookSensitivity = Vector2.one;

    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool canInteract { get; private set; }
    public bool isCrouching { get; private set; }
    public bool inputLocked { get; private set; }

    public event Action OnJump;
    public event Action OnInteract;
    public event Action OnTogglePlaceOrPickup; // Deals with Deals with just movement mode change, anims, audio, etc,
    public event Action OnCrouch;

    private void OnEnable()
    {
        EnableAction(moveAction);
        EnableAction(lookAction);
        EnableAction(jumpAction);
        EnableAction(crouchAction);
        EnableAction(interactAction);
        EnableAction(togglePlaceOrPickupAction);

        SubscribePerformed(jumpAction, HandleJump);
        SubscribePerformed(interactAction, HandleInteract);
        SubscribePerformed(togglePlaceOrPickupAction, HandlePlaceOrPickup);
        SubscribeChanged(crouchAction, HandleCrouchChanged);
    }

    private void OnDisable()
    {
        DisableAction(moveAction);
        DisableAction(lookAction);
        DisableAction(jumpAction);
        DisableAction(crouchAction);
        DisableAction(interactAction);
        DisableAction(togglePlaceOrPickupAction);

        UnsunscribePerformed(jumpAction, HandleJump);
        UnsunscribePerformed(interactAction, HandleInteract);
        UnsunscribePerformed(togglePlaceOrPickupAction, HandlePlaceOrPickup);
        UnsunscribeChanged(crouchAction, HandleCrouchChanged);
    }

    private void Update()
    {
        Move = ReadVector2(moveAction);
        Look = Vector2.Scale(ReadVector2(lookAction), lookSensitivity);

        if (inputLocked)
        {
            Move = Vector2.zero;
            Look = Vector2.zero;
        }
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
        OnJump?.Invoke();
    }

    private void HandleInteract(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke();
    }

    private void HandlePlaceOrPickup(InputAction.CallbackContext context)
    {
        OnTogglePlaceOrPickup?.Invoke();
    }

    private void HandleCrouchChanged(InputAction.CallbackContext context)
    {
        if (isCrouching)
        {
            isCrouching = false;
            return;
        }

        if (context.started)
        {
            isCrouching = true;
        }
        else if (context.canceled)
        {
            isCrouching = false;
        }
    }

    private static Vector2 ReadVector2(InputActionReference reference)
    {
        return reference != null && reference.action != null ? reference.action.ReadValue<Vector2>() : Vector2.zero;
    }

    private void EnableAction(InputActionReference reference)
    {
        if (reference == null || reference.action == null) return;
        if (!reference.action.enabled) reference.action.Enable();
    }

    private void DisableAction(InputActionReference reference)
    {
        if(reference == null || reference.action == null) return;
        if(reference.action.enabled) reference.action.Disable();
    }

    private static void SubscribePerformed(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
    {
        if(reference == null || reference.action == null) return;
        reference.action.performed += actionHandler;
    }

    private static void UnsunscribePerformed(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
    {
        if (reference == null || reference.action == null) return;
        reference.action.performed -= actionHandler;
    }

    private static void SubscribeChanged(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
    {
        if (reference == null || reference.action == null) return;
        reference.action.performed += actionHandler;
        reference.action.canceled += actionHandler;
    }

    private static void UnsunscribeChanged(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
    {
        if (reference == null || reference.action == null) return;
        reference.action.performed -= actionHandler;
        reference.action.canceled -= actionHandler;
    }

}
