using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    private PlayerInputs playerInput;

    [Header("References")]
    [Space(10)]

    [Tooltip("The head/eyes of the object that the linetrace for interactions will fire from")]
    [SerializeField] private Transform playerEyes;

    private void Awake()
    {
        if (playerInput == null) playerInput = GetComponent<PlayerInputs>();
        if (playerInput == null) Debug.LogError("PlayerInputs reference is missing");
    }

    private void OnEnable()
    {
        playerInput.OnInteract += TryInteract;
    }

    private void OnDisable()
    {
        playerInput.OnInteract -= TryInteract;
    }

    private void TryInteract()
    {
        // Cast linetrace
        // Check for interact object
        // Complete interaction
    }
}
