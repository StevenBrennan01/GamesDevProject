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
    [SerializeField] private InputActionReference placeOrPickupAction;

    [Tooltip("Button - Pausing game and triggering UI event")]
    [SerializeField] private InputActionReference pauseAction;

    [Header("Settings")]
    [Tooltip("Scales the look input (mouse/stick) before consumers read it.")]
    //[SerializeField] private Vector2 lookSensitivity = Vector2.one;

    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    //public bool canInteract { get; private set; }

    [SerializeField] public bool isCrouching;
    public bool inputLocked { get; private set; }

    public event Action OnJump;
    public event Action OnInteract;
    public event Action OnTogglePlaceOrPickup; // Deals with just movement mode change, anims, audio, etc,
    public event Action OnPaused;

    private void OnEnable()
    {
        EnableAction(moveAction);
        EnableAction(lookAction);
        EnableAction(jumpAction);
        EnableAction(crouchAction);
        EnableAction(interactAction);
        EnableAction(placeOrPickupAction);
        EnableAction(pauseAction);

        SubscribePerformed(jumpAction, HandleJump);
        SubscribePerformed(interactAction, HandleInteract);
        SubscribePerformed(placeOrPickupAction, HandlePlaceOrPickup);
        SubscribeToggled(crouchAction, HandleCrouchChanged);
        SubscribePerformed(pauseAction, TogglePaused);
    }

    private void OnDisable()
    {
        DisableAction(moveAction);
        DisableAction(lookAction);
        DisableAction(jumpAction);
        DisableAction(crouchAction);
        DisableAction(interactAction);
        DisableAction(placeOrPickupAction);
        DisableAction(pauseAction);

        UnsubscribePerformed(jumpAction, HandleJump);
        UnsubscribePerformed(interactAction, HandleInteract);
        UnsubscribePerformed(placeOrPickupAction, HandlePlaceOrPickup);
        UnsubscribePerformed(pauseAction, TogglePaused);
        UnsubscribeToggled(crouchAction, HandleCrouchChanged);
    }

    private void Update()
    {
        Move = ReadVector2(moveAction);
        Look = ReadVector2(lookAction);

        if (inputLocked)
        {
            Move = Vector2.zero;
            Look = Vector2.zero;
        }
    }

    // Public method to allow external classes access to lock/unlock input.
    public void SetInputLocked(bool lockStatus)
    {
        inputLocked = lockStatus;
    }

    private void TogglePaused(InputAction.CallbackContext context)
    {
        OnPaused?.Invoke();
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
        if (inputLocked) return;
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
        if (inputLocked) return;

        if (context.performed)
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

    private static void UnsubscribePerformed(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
    {
        if (reference == null || reference.action == null) return;
        reference.action.performed -= actionHandler;
    }

    // For the inputs that also require an exit unlike one shot actions
    private static void SubscribeToggled(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
    {
        if (reference == null || reference.action == null) return;
        reference.action.performed += actionHandler;
        reference.action.canceled += actionHandler;
    }

    private static void UnsubscribeToggled(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
    {
        if (reference == null || reference.action == null) return;
        reference.action.performed -= actionHandler;
        reference.action.canceled -= actionHandler;
    }
}