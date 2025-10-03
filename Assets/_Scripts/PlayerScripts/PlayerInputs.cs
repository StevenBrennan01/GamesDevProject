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

    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    //public bool canInteract { get; private set; }

    public event Action OnJump;
    public event Action OnInteract;
    //public event Action OnSwitchPerspective

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void Update()
    {
        
    }


}
