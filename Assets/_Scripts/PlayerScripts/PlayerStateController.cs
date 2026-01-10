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

    [SerializeField] private GameObject playerCharacter; // rotate body to face cam just before head pickup
    [SerializeField] private GameObject playerBody; // rotate body to face cam just before head pickup
    [SerializeField] private GameObject playerHead;

    [Tooltip("Cinemachine VCam for the carried mount 'hands'")]
    [SerializeField] private CinemachineCamera carriedVirtualCamera;

    [Tooltip("Cinemachine VCam for the placed head cam")]
    [SerializeField] private CinemachineCamera placedVirtualCamera;

    [Tooltip("The hands of the player")]
    [SerializeField] private Transform carriedMount; // The hands of the player

    [SerializeField] private Transform firstPersonYawRoot; //Playerbody root (the pelvis)
    [SerializeField] private Transform firstPersonPitchPivot; // In this case the Carried Cam Pivot

    [Tooltip("Priority values for Active and Inactive Vcameras")]
    private int activePriority = 5;
    private int inactivePriority = 1;

    [Header("VCam Transitions")]
    [Tooltip("Seconds that input will be locked whilst blend is taking place")]
    [SerializeField][Range(0, 2)] private float blendLockInputSeconds;

    [Header("Second-Person View Attributes")]
    [SerializeField, Range(0, 180)] private float placedYawClamp = 45f;
    [SerializeField, Range(0, 89)] private float placedPitchClamp = 30f;
    [SerializeField] private Vector2 secondPersonLookSensitivity = Vector2.one;
    [SerializeField, Range(0, 80)] private float secondPersonFieldOfView;

    [Header("First-Person View Attributes")]
    [SerializeField, Range(0, 89)] private float firstPersonPitchClamp = 85f;
    [SerializeField] private Vector2 firstPersonLookSensitivity = Vector2.one;
    [SerializeField, Range(0, 80)] private float firstPersonFieldOfView;

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
        playerBody.SetActive(false); // Currently inactive but will set body to invisible when in first person to avoid body clipping

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

        carriedVirtualCamera.Lens.FieldOfView = firstPersonFieldOfView;
        placedVirtualCamera.Lens.FieldOfView = secondPersonFieldOfView;
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
            Debug.LogWarning("Player is not within a valid head bounds");
            return;
        }

        if (playerCharacter != null && !currentPlacementVolume.canPlace)
        {
            Debug.LogWarning("Player cannot currently place");
            return;
        }

        Transform anchor = currentPlacementVolume.placementAnchor;
        if (currentPlacementVolume != null && anchor == null)
        {
            Debug.LogError("No Anchor for head placement exists within this volume");
            return;
        }

        playerBody.SetActive(true);

        // Here we will want the player body to face the head placement Volume straight away

        playerHead.transform.position = anchor.position;
        playerHead.transform.rotation = anchor.rotation;

        neutralHeadRotation = playerHead.transform.rotation;
        placedYawOffset = 0f;
        placedPitchOffset = 0f;

        if (placedVirtualCamera != null)
        {
            playerHead.transform.SetParent(anchor.transform, false);
            playerHead.transform.localPosition = Vector3.zero;
            playerHead.transform.localRotation = Quaternion.identity;
        }

        SetCameraMode(CameraMode.Placed);
        CurrentMovementMode = MovementMode.SecondPerson;
    }

    private void TryPickupHead() // Put a delay here so that the player visibly does a pickup anim then the head moves
    {
        if (currentPlacementVolume == null) return;

        //playerHead.transform.position = carriedMount.position;
        //playerHead.transform.rotation = carriedMount.rotation;

        neutralHeadRotation = playerHead.transform.rotation;
        placedYawOffset = 0f;
        placedPitchOffset = 0f;

        //if (carriedMount != null && carriedVirtualCamera != null)
        //{
        //    playerHead.transform.SetParent(carriedMount.transform, false);
        //    playerHead.transform.localPosition = Vector3.zero;
        //    playerHead.transform.localRotation = Quaternion.identity;
        //}

        // carriedMount currently equals the transform of the playerHeadPivot also.
        // This is because the head now does not actually return to the player, but remains at the current anchor -
        // until the player interacts with a new anchor. Then, the head gets put on new anchor -
        // and the camera shifts to new anchor.
        // This is ok right now as the head is invisible
        // as of RIGHT NOW, this is a fix, not perfect but it works as I can gate place points 
        // so the player cannot access 2 at once, 
        // This could be made better with some kind of headRetrieved? bool to mitigate this.

        StartCoroutine(PauseThenPickup());
    }

    private IEnumerator PauseThenPickup()
    {
        playerInput.SetInputLocked(true);
        isBlending = true;

        // Make the player face the head

        yield return new WaitForSeconds(1.5f); //1.5f = just before anim ends

        SetCameraMode(CameraMode.Carried);
        CurrentMovementMode = MovementMode.FirstPerson;

        StartCoroutine(HidePlayerBody());
    }

    private IEnumerator HidePlayerBody()
    {
        // 0.5 is just shy of the camera blend so to not see the body vanish, but not hide it too late.
        yield return new WaitForSeconds(0.5f);

        // Disabling body in first person so to avoid clipping
        playerBody.SetActive(false);
        playerInput.SetInputLocked(false);

        isBlending = false;
    }

    private void SetCameraMode(CameraMode targetMode)
    {
        CurrentCameraMode = targetMode;

        if (carriedVirtualCamera == null || placedVirtualCamera == null)
        {
            Debug.LogError("At least one VCam reference is missing");
            return;
        }

        // Use priority values to switch between modes
        if (targetMode == CameraMode.Carried)
        {
            carriedVirtualCamera.Priority = activePriority;
            placedVirtualCamera.Priority = inactivePriority;
        }
        else if (targetMode == CameraMode.Placed)
        {
            carriedVirtualCamera.Priority = inactivePriority;
            placedVirtualCamera.Priority = activePriority;

            StartCoroutine(LockInputDuringBlend(blendLockInputSeconds));
        }

        //StartCoroutine(LockInputDuringBlend(blendLockInputSeconds));
    }
    private IEnumerator LockInputDuringBlend(float lockSeconds)
    {
        if (!isBlending)
        {
            isBlending = true;
        }
        if (!playerInput.inputLocked)
        {
            playerInput.SetInputLocked(true);
        }

        // Could use CinemachineBrain.ActiveBlend == null
        // but waiting longer to avoid overlap issues and let anims play
        yield return new WaitForSeconds(lockSeconds);

        if (isBlending)
        {
            isBlending = false;
        }
        if (playerInput.inputLocked)
        {
            playerInput.SetInputLocked(false);
        }
    }

    private void InitializeCameraMode(CameraMode targetMode)
    {
        CurrentCameraMode = targetMode;

        if (carriedVirtualCamera == null || placedVirtualCamera == null)
        {
            Debug.LogError("At least one VCam reference is missing");
            return;
        }

        // INITIALIZES WITH PLACED MODE
        //carriedVirtualCamera.Priority = inactivePriority;
        //headVirtualCamera.Priority = activePriority;

        // INITIALIZES WITH CARRIED MODE
        carriedVirtualCamera.Priority = activePriority;
        placedVirtualCamera.Priority = inactivePriority;
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
        if (CurrentMovementMode == MovementMode.SecondPerson && placedVirtualCamera != null) // Is in 2nd Person
        {
            basis = placedVirtualCamera.transform;
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