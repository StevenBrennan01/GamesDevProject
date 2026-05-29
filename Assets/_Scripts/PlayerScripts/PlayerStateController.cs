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
    private LevelStartHeadPlacement startHeadPlacement;
    private SignalManager signalManager;

    [Header("References")]
    [Space(10)]

    [SerializeField] private GameObject playerCharacter; // rotate body to face cam just before head pickup
    [SerializeField] private GameObject playerBody; // rotate body to face cam just before head pickup
    [SerializeField] private GameObject playerHead;

    private CharacterController characterController;

    [Tooltip("Cinemachine VCam for the carried mount 'hands'")]
    [SerializeField] private CinemachineCamera carriedVirtualCamera;

    [Tooltip("Cinemachine VCam for the placed head cam")]
    [SerializeField] private CinemachineCamera placedVirtualCamera;

    [Tooltip("The hands of the player")]
    [SerializeField] private Transform carriedMount; // The hands of the player

    [SerializeField] private Transform firstPersonYawRoot; //Playerbody root (the pelvis)
    [SerializeField] private Transform firstPersonPitchPivot; // In this case the Carried Cam Pivot

    [Header("Player Start in First Person, or Second Person?")]
    [Space(10)]
    [SerializeField] private bool playerStartWithInputLocked;
    public bool isHeadPlaced => placedHeadVolume != null;
    [Tooltip("freeze for 32.5 for first level")]
    [SerializeField, Range(0, 40)] private float startLockInputSeconds;

    [Tooltip("Priority values for Active and Inactive Vcameras")]
    private int activePriority = 5;
    private int inactivePriority = 1;

    [Header("VCam Transitions")]
    [Tooltip("Seconds that input will be locked whilst blend is taking place")]
    [SerializeField, Range(0, 2)] private float lockSecondsAfterPlacing;
    [SerializeField, Range(0, 2)] private float waitSecondsBeforePickup;

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
    public HeadPlacementVolume potentialPlacementVolume = null;
    public HeadPlacementVolume placedHeadVolume;

    // This will be used by the Signal Manager to compare the distance between the head and the player and adjust signal accordingly
    public Transform placedHeadLocation => placedHeadVolume?.placementAnchor;

    public MovementMode CurrentMovementMode { get; private set; } = MovementMode.FirstPerson;
    public CameraMode CurrentCameraMode { get; private set; } = CameraMode.Carried;
    public bool isBlending = false;
    private bool isTransitioningHead;

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
        if (signalManager == null) signalManager = FindAnyObjectByType<SignalManager>();
        if (characterController == null) characterController = GetComponent<CharacterController>();

        if (potentialPlacementVolume != null)
        {
            potentialPlacementVolume = null;
        }

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

        if (playerStartWithInputLocked)
        {
            StartCoroutine(LockInput(startLockInputSeconds));
        }

        StartCoroutine(LockPauseMenu(startLockInputSeconds));

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        StartCoroutine(PlaceHeadOnStartRoutine());
    }

    private void OnEnable()
    {
        playerInput.OnTogglePlaceOrPickup += HandlePlaceOrPickup;
    }

    private void OnDisable ()
    {
        playerInput.OnTogglePlaceOrPickup -= HandlePlaceOrPickup;
    }

    private IEnumerator PlaceHeadOnStartRoutine()
    {
        yield return new WaitForSeconds(.75f);

        startHeadPlacement = FindAnyObjectByType<LevelStartHeadPlacement>();
        if(startHeadPlacement != null && startHeadPlacement.startVolume != null)
        {
            PlaceHeadOnStart(startHeadPlacement.startVolume);
        }
        else
        {
            Debug.LogWarning("No Start Head Placement Volume found, starting player in first person at spawn point");
            CurrentMovementMode = MovementMode.FirstPerson;
            InitializeCameraMode(CameraMode.Carried);
            playerBody.SetActive(false);
        }
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
        if (isBlending || isTransitioningHead) return;

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
        if (potentialPlacementVolume == null) return;
        if (!potentialPlacementVolume.canPlace) return;
        if (isTransitioningHead) return;
        if (potentialPlacementVolume.placementAnchor == null) return;

        StartCoroutine(PlaceHeadRoutine(potentialPlacementVolume));
    }

    private IEnumerator PlaceHeadRoutine(HeadPlacementVolume targetVolume)
    {
        isTransitioningHead = true;
        isBlending = true;
        playerInput.SetMovementLocked(true);

        Transform anchor = targetVolume.placementAnchor;

        placedHeadVolume = targetVolume;

        playerBody.SetActive(true);

        playerHead.transform.position = anchor.position;
        playerHead.transform.rotation = anchor.rotation;

        neutralHeadRotation = playerHead.transform.rotation;
        placedYawOffset = 0f;
        placedPitchOffset = 0f;

        if (placedVirtualCamera != null)
        {
            playerHead.transform.SetParent(anchor, false);
            playerHead.transform.localPosition = Vector3.zero;
            playerHead.transform.localRotation = Quaternion.identity;
        }

        SetCameraMode(CameraMode.Placed);
        CurrentMovementMode = MovementMode.SecondPerson;

        targetVolume.headVisualiser.SetActive(false);

        if (placedHeadVolume.isHeadCharger)
        {
            // Begin charging battery
        }

        yield return new WaitForSeconds(lockSecondsAfterPlacing);

        isBlending = false;
        isTransitioningHead = false;
        playerInput.SetMovementLocked(false);
    }

    private void TryPickupHead()
    {
        if (placedHeadVolume == null) return;
        if (potentialPlacementVolume != placedHeadVolume) return;
        if (isTransitioningHead) return;

        //playerHead.transform.position = carriedMount.position;
        //playerHead.transform.rotation = carriedMount.rotation;

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
        // This is ok as the head is invisible

        StartCoroutine(PickupHeadRoutine(placedHeadVolume));
    }

    private IEnumerator PickupHeadRoutine(HeadPlacementVolume pickupVolume)
    {
        isTransitioningHead = true;
        isBlending = true;
        playerInput.SetMovementLocked(true);

        neutralHeadRotation = playerHead.transform.rotation;
        placedYawOffset = 0f;
        placedPitchOffset = 0f;

        yield return new WaitForSeconds(waitSecondsBeforePickup);

        SetCameraMode(CameraMode.Carried);
        CurrentMovementMode = MovementMode.FirstPerson;

        yield return StartCoroutine(HidePlayerBody(pickupVolume));

        isTransitioningHead = false;
        isBlending = false;
    }

    private IEnumerator HidePlayerBody(HeadPlacementVolume pickupVolume)
    {
        // 0.5 is just shy of the camera blend so to not see the body vanish, but not hide it too late.
        yield return new WaitForSeconds(0.5f);

        // Disabling body in first person so to avoid clipping
        playerBody.SetActive(false);

        yield return new WaitForSeconds(0.2f);

        if(playerHead != null && firstPersonPitchPivot != null)
        {
            playerHead.transform.SetParent(firstPersonPitchPivot, false);
            playerHead.transform.localPosition = Vector3.zero;
            playerHead.transform.localRotation = Quaternion.identity;
        }

        if (pickupVolume != null)
        {
            pickupVolume.headVisualiser.SetActive(true);
        }

        //potentialPlacementVolume = null;
        placedHeadVolume = null;
        playerInput.SetMovementLocked(false);
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

            StartCoroutine(LockInputDuringBlend(lockSecondsAfterPlacing));
        }
    }
    private IEnumerator LockInputDuringBlend(float lockSeconds)
    {
        if (!isBlending)
        {
            isBlending = true;
        }
        if (!playerInput.movementLocked)
        {
            playerInput.SetMovementLocked(true);
        }

        // Could use CinemachineBrain.ActiveBlend == null
        // but waiting longer to avoid overlap issues and let anims play
        yield return new WaitForSeconds(lockSeconds);

        if (isBlending)
        {
            isBlending = false;
        }
        if (playerInput.movementLocked)
        {
            playerInput.SetMovementLocked(false);
        }
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
        potentialPlacementVolume = volume;
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
        if (targetMode == CameraMode.Placed)
        {
            carriedVirtualCamera.Priority = inactivePriority;
            placedVirtualCamera.Priority = activePriority;
        }
        
        if (targetMode == CameraMode.Carried)
        {
            // INITIALIZES WITH CARRIED MODE
            carriedVirtualCamera.Priority = activePriority;
            placedVirtualCamera.Priority = inactivePriority;
        }
    }

    public void PlaceHeadOnStart(HeadPlacementVolume startVolume)
    {        
        SetCurrentPlacementVolume(startVolume);

        startVolume.headVisualiser.SetActive(false);
        placedHeadVolume = startVolume;

        playerHead.transform.position = startVolume.placementAnchor.position;
        playerHead.transform.rotation = startVolume.placementAnchor.rotation;

        neutralHeadRotation = playerHead.transform.rotation;
        placedYawOffset = 0f;
        placedPitchOffset = 0f;

        if (placedVirtualCamera != null)
        {
            playerHead.transform.SetParent(startVolume.placementAnchor.transform, false);
            playerHead.transform.localPosition = Vector3.zero;
            playerHead.transform.localRotation = Quaternion.identity;
        }

        if(characterController != null)
        {
            characterController.enabled = true;
        }

        CurrentMovementMode = MovementMode.SecondPerson;
        InitializeCameraMode(CameraMode.Placed);

        playerBody.SetActive(true);

        GameObject spawnPoint = GameObject.FindGameObjectWithTag("LevelSpawnPoint");
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
            transform.rotation = spawnPoint.transform.rotation;
        }
        // some other things to note:
        // 1: an elevator or start scene where the body and head are far away, the signal will need to be dealt with.
        // 2: if above is true, when restarting, just make a dupe scene that gets triggered instead and doesnt have the intro
        // one final thing is that the player rotation on respawn is still wrong
        
        potentialPlacementVolume = null;
    }

    public void ForceHeadBackToPlayer()
    {
        if(playerHead != null && carriedMount != null)
        {
            playerHead.transform.SetParent(firstPersonPitchPivot, false);
            playerHead.transform.localPosition = Vector3.zero;
            playerHead.transform.localRotation = Quaternion.identity;
        }

        placedHeadVolume = null;
        potentialPlacementVolume = null;

        CurrentMovementMode = MovementMode.FirstPerson;
        InitializeCameraMode(CameraMode.Carried);
    }

    private IEnumerator LockInput(float lockSeconds)
    {
        playerInput.SetMovementLocked(true);

        yield return new WaitForSeconds(lockSeconds);

        playerInput.SetMovementLocked(false);
    }

    private IEnumerator LockPauseMenu(float lockSeconds)
    {
        playerInput.SetPauseMenuLocked(true);

        yield return new WaitForSeconds(lockSeconds);

        playerInput.SetPauseMenuLocked(false);
    }

    public void ResetForLevelSpawn(Transform spawnPoint)
    {
        StopAllCoroutines();
        StartCoroutine(LockPauseMenu(3.5f));

        playerBody.SetActive(false);

        isBlending = false;
        potentialPlacementVolume = null;
        placedHeadVolume = null;

        CurrentMovementMode = MovementMode.FirstPerson;
        InitializeCameraMode(CameraMode.Carried);

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        if (firstPersonYawRoot != null)
        {
            firstPersonYawRoot.rotation = Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f);
            fpYaw = firstPersonYawRoot.eulerAngles.y;
        }

        if (firstPersonPitchPivot != null)
        {
            firstPersonPitchPivot.localRotation = Quaternion.identity;
            fpPitch = 0f;
        }

        if(characterController != null)
        {
            characterController.enabled = true;
        }


        signalManager.IncreaseSignalLevelForDuration(.25f);

        StartCoroutine(GiveBackControlsAfterLevelRestart(3f));
    }

    private IEnumerator GiveBackControlsAfterLevelRestart(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerInput != null)
        {
            playerInput.SetMovementAndCameraLocked(false);
        }
    }

    public void MoveToSpawnAndAlign(Transform spawnPoint)
    {
        StartCoroutine(LockPauseMenu(3f));

        playerBody.SetActive(false);

        isBlending = false;
        potentialPlacementVolume = null;
        placedHeadVolume = null;

        CurrentMovementMode = MovementMode.FirstPerson;
        InitializeCameraMode(CameraMode.Carried);

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        if (firstPersonYawRoot != null)
        {
            firstPersonYawRoot.rotation = Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f);
            fpYaw = firstPersonYawRoot.eulerAngles.y;
        }

        if (firstPersonPitchPivot != null)
        {
            firstPersonPitchPivot.localRotation = Quaternion.identity;
            fpPitch = 0f;
        }

        signalManager.IncreaseSignalLevelForDuration(.25f);

        StartCoroutine(GiveBackControlsAfterLevelRestart(3f));
    }
}