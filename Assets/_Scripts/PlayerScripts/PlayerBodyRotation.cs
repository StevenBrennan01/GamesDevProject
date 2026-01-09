using UnityEngine;

public class PlayerBodyRotation : MonoBehaviour
{
    private PlayerInputs playerInput;
    private PlayerStateController playerState;
    private Transform bodyTransform;

    [Header("Settings")]
    [SerializeField, Tooltip("Degrees per second to rotate toward target direction")]
    private float turnSpeedDegPerSec = 540f;

    [SerializeField, Tooltip("Stick magnitude required before rotating from input")]
    private float inputDeadzone = 0.1f;

    private void Awake()
    {
        playerInput = GetComponentInParent<PlayerInputs>();
        playerState = GetComponentInParent<PlayerStateController>();
        bodyTransform = this.transform;
    }

    // Update is called once per frame
    private void Update()
    {
        if (playerInput == null || playerState == null) return;
        if (playerState.CurrentMovementMode != MovementMode.SecondPerson) return;
        if (!this.gameObject.activeInHierarchy) return;

        Vector3 targetDir = GetTargetDirection();
        if (targetDir.sqrMagnitude < 1e-4f) return; // 1e-4f is effectively 0 but apparently prevents jittery movement?

        Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
        bodyTransform.rotation = Quaternion.RotateTowards(bodyTransform.rotation, targetRot, turnSpeedDegPerSec * Time.deltaTime);
    }

    private Vector3 GetTargetDirection()
    {
        Vector2 m = playerInput.Move;
        if(m.sqrMagnitude < inputDeadzone * inputDeadzone) return Vector3.zero;

        return playerState.ComputeMovementDirection(m).normalized;
    }
}
