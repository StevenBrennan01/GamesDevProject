using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public enum MovementMode
{
    FirstPerson,
    SecondPerson
}

public enum CameraMode
{
    Carried,
    Placed
}

public class PlayerStateController : MonoBehaviour
{
    private PlayerInputs playerInput;

    [Header("References")]
    [Space(10)]

    [SerializeField] private GameObject playerBody;
    [SerializeField] private GameObject playerHead;

    [Tooltip("Cinemachine VCam for the carried mount 'hands'")]
    [SerializeField] private CinemachineCamera carriedVirtualCamera;

    [Tooltip("Cinemachine VCam for the placed head cam")]
    [SerializeField] private CinemachineCamera headVirtualCamera;

    [Tooltip("The hands of the player")]
    [SerializeField] private Transform carriedMount; // The hands of the player

    [SerializeField] private Transform firstPersonYawRoot; //Playerbody root
    [SerializeField] private Transform firstPersonPitchPivot; // In this case the Carried Cam Pivot

    [Tooltip("Priority values for Active and Inactive Vcameras")]
    private int activePriority = 5;
    private int inactivePriority = 1;

    [Header("VCam Transitions")]
    [Tooltip("Seconds that input will be locked whilst blend is taking place")]
    [SerializeField][Range(0, 1)] private float blendLockInputSeconds = 0.75f;

    [Header("Second-Person View Attributes")]
    [Space(2)]
    [Header("Second-Person Look Bounds")]
    [SerializeField, Range(0, 180)] private float placedYawClamp = 45f;
    [SerializeField, Range(0, 89)] private float placedPitchClamp = 30f;
    [SerializeField] private Vector2 secondPersonLookSensitivity = Vector2.one;

    [Header("First-Person View Attributes")]
    [Space(2)]
    [Header("First-Person Look Bounds")]
    [SerializeField, Range(0, 89)] private float firstPersonPitchClamp = 85f;
    [SerializeField] private Vector2 firstPersonLookSensitivity = Vector2.one;


    [Header("Currently Active Placement Volume")]
    [Space(2)]
    // public for testing, privatise when done
    public HeadPlacementVolume currentPlacementVolume;

    public MovementMode CurrentMovementMode { get; private set; } = MovementMode.FirstPerson;
    public CameraMode CurrentCameraMode { get; private set; } = CameraMode.Carried;
    public bool isBlending /*{ get; private set; }*/ = false;

    // Neutral rotation for the head and placement, rotations and offsets are applied relative to this
    private Quaternion neutralHeadRotation;
    private float placedYawOffset;
    private float placedPitchOffset;

    private float fpYaw;
    private float fpPitch;

    private void Awake()
    {
        if (playerInput == null) playerInput = GetComponent<PlayerInputs>();
        if (playerInput == null) { Debug.LogError("Player Input reference is missing"); }

        CurrentMovementMode = MovementMode.FirstPerson;
        InitializeCameraMode(CameraMode.Carried); // Using initialize method, no blend lock on start

        if (firstPersonYawRoot != null)
        {
            fpYaw = firstPersonYawRoot.eulerAngles.y;
        }
        if (firstPersonPitchPivot != null)
        {
            float pitch = firstPersonPitchPivot.localEulerAngles.x;
            fpPitch = (pitch > 180f) ? pitch - 360f : pitch;
            fpPitch = Mathf.Clamp(fpPitch, -firstPersonPitchClamp, firstPersonPitchClamp);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        playerInput.OnTogglePlaceOrPickup += HandlePlaceOrPickup;
    }

    private void OnDisable ()
    {
        playerInput.OnTogglePlaceOrPickup -= HandlePlaceOrPickup;
    }

    private void Update()
    {
        if (CurrentCameraMode == CameraMode.Placed)
        {
            ApplySecondPersonLook(playerInput?.Look ?? Vector2.zero);
        }
        else if (CurrentCameraMode == CameraMode.Carried)
        {
            ApplyFirstPersonLook(playerInput?.Look ?? Vector2.zero);
        }

        Vector2 moveInput = playerInput?.Move ?? Vector2.zero;
        Vector3 moveWorld = ComputeMovementDirection(moveInput);
    }

    private void HandlePlaceOrPickup()
    {
        if (isBlending) return;

        if (CurrentCameraMode == CameraMode.Carried)
        {
            TryPlaceHead();
        }
        else
        {
            TryPickupHead();
        }
    }

    private void TryPlaceHead()
    {
        if (currentPlacementVolume == null)
        {
            Debug.LogWarning("no volume to place head was found, aborting action");
            return;
        }

        if (playerBody != null && !currentPlacementVolume.canPlace)
        {
            // Player exists but not within the volume bounds
            return;
        }

        Transform anchor = currentPlacementVolume.placementAnchor;
        if (anchor == null)
        {
            Debug.LogError("No Anchor for head placement exists within this volume");
            return;
        }

        playerHead.transform.position = anchor.position;
        playerHead.transform.rotation = anchor.rotation;

        neutralHeadRotation = playerHead.transform.rotation;
        placedYawOffset = 0f;
        placedPitchOffset = 0f;

        if (headVirtualCamera != null)
        {
            playerHead.transform.SetParent(anchor.transform, false);
            playerHead.transform.localPosition = Vector3.zero;
            playerHead.transform.localRotation = Quaternion.identity;
        }

        SetCameraMode(CameraMode.Placed);
        CurrentMovementMode = MovementMode.SecondPerson;
    }

    private void TryPickupHead()
    {
        if (currentPlacementVolume == null)
        {
            Debug.LogWarning("Not within volume to pickup head");
            return;
        }

        playerHead.transform.position = carriedMount.position;
        playerHead.transform.rotation = carriedMount.rotation;

        neutralHeadRotation = playerHead.transform.rotation;
        placedYawOffset = 0f;
        placedPitchOffset = 0f;

        if (carriedMount != null && carriedVirtualCamera != null)
        {
            playerHead.transform.SetParent(carriedMount.transform, false);
            playerHead.transform.localPosition = Vector3.zero;
            playerHead.transform.localRotation = Quaternion.identity;
        }

        SetCameraMode(CameraMode.Carried);
        CurrentMovementMode = MovementMode.FirstPerson;
    }

    private void SetCameraMode(CameraMode targetMode)
    {
        CurrentCameraMode = targetMode;

        if (carriedVirtualCamera == null || headVirtualCamera == null)
        {
            Debug.LogError("At least one VCam reference is missing");
            return;
        }

        // Use priority values to switch between modes
        if (targetMode == CameraMode.Carried)
        {
            carriedVirtualCamera.Priority = activePriority;
            headVirtualCamera.Priority = inactivePriority;
        }
        else if (targetMode == CameraMode.Placed)
        {
            carriedVirtualCamera.Priority = inactivePriority;
            headVirtualCamera.Priority = activePriority;
        }

        StartCoroutine(LockInputDuringBlend(blendLockInputSeconds));
    }

    private void InitializeCameraMode(CameraMode targetMode)
    {
        CurrentCameraMode = targetMode;

        if (carriedVirtualCamera == null || headVirtualCamera == null)
        {
            Debug.LogError("At least one VCam reference is missing");
            return;
        }

        // INITIALIZING WITH PLACED MODE
        //carriedVirtualCamera.Priority = inactivePriority;
        //headVirtualCamera.Priority = activePriority;

        // INITIALIZING WITH CARRIED MODE
        carriedVirtualCamera.Priority = activePriority;
        headVirtualCamera.Priority = inactivePriority;
    }

    private IEnumerator LockInputDuringBlend(float lockSeconds)
    {
        isBlending = true;
        playerInput.SetInputLocked(true);

        // Could use CinemachineBrain.ActiveBlend == null
        // However, waiting 0.05s after blend ends to try avoid overlap issues
        yield return new WaitForSeconds(lockSeconds);

        playerInput.SetInputLocked(false);
        isBlending = false;
    }

    private void ApplySecondPersonLook(Vector2 lookDelta)
    {
        float yawDelta = lookDelta.x * secondPersonLookSensitivity.x;
        float pitchDelta = -lookDelta.y * secondPersonLookSensitivity.y;

        placedYawOffset += yawDelta;
        placedPitchOffset += pitchDelta;

        placedYawOffset = Mathf.Clamp(placedYawOffset, -placedYawClamp, placedYawClamp);
        placedPitchOffset = Mathf.Clamp(placedPitchOffset, -placedPitchClamp, placedPitchClamp);

        // Apply offsets relative to neutral rotation (anchor)
        Quaternion yawRot = Quaternion.AngleAxis(placedYawOffset, Vector3.up);
        Quaternion pitchRot = Quaternion.AngleAxis(placedPitchOffset, Vector3.right);

        playerHead.transform.rotation = neutralHeadRotation * yawRot * pitchRot;
        //currentPlacementVolume.placementAnchor.transform.rotation = neutralHeadRotation * yawRot * pitchRot;
        // Above currently works for the correct blend and detach/reattachment of the head, however currently it breaks when the player leaves
        // the currentPlacementVolume, obviously because it doesn't exist anymore.
        // Maybe the solution is setting the Vcams target to be anchor within the newest volume it enters?
        // But it also still needs a way of looking around when the player leaves the zone as when the player leaves, the above function to
        // Look around within second person breaks and fails
    }

    private void ApplyFirstPersonLook(Vector2 lookDelta)
    {
        float yawDelta = lookDelta.x * firstPersonLookSensitivity.x;
        float pitchDelta = -lookDelta.y * firstPersonLookSensitivity.y;

        fpYaw += yawDelta;
        fpPitch = Mathf.Clamp(fpPitch + pitchDelta, -firstPersonPitchClamp, firstPersonPitchClamp);

        firstPersonYawRoot.rotation = Quaternion.Euler(0f, fpYaw, 0f);
        firstPersonPitchPivot.localRotation = Quaternion.Euler(fpPitch, 0f, 0f);
    }

    public Vector3 ComputeMovementDirection(Vector2 moveInput)
    {
        if(moveInput == Vector2.zero) return Vector3.zero;

        Transform basis;
        if (CurrentMovementMode == MovementMode.SecondPerson && headVirtualCamera != null) // Is in 2nd Person
        {
            basis = headVirtualCamera.transform;
        }
        else if (carriedVirtualCamera != null) // Is in 1st Person
        {
            basis = carriedVirtualCamera.transform;
        }
        else { basis = this.transform; }

        Vector3 forward = basis.forward;
        Vector3 right = basis.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveWorld = forward * moveInput.y + right * moveInput.x;
        return moveWorld;
    }

    public void SetCurrentPlacementVolume(HeadPlacementVolume volume)
    {
        currentPlacementVolume = volume;
    }
}