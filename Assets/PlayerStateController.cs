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
    [Header("References")]
    [Space(10)]

    private PlayerInputs playerInput;

    [Tooltip("Cinemachine VCam for the carried mount 'hands'")]
    [SerializeField] private CinemachineCamera carriedVcam;

    [Tooltip("Cinemachine VCam for the placed head cam")]
    [SerializeField] private CinemachineCamera placedVcam;
    private int activePriority = 20;
    private int inactivePriority = 10;

    [Header("Transitions")]
    [Tooltip("Seconds that input will be locked whilst blend is taking place")]
    [SerializeField][Range(0, 1)] private float blendLockInputSeconds = 0.75f;

    public MovementMode CurrentMovementMode { get; private set; } = MovementMode.FirstPerson;
    public CameraMode CurrentCameraMode { get; private set; } = CameraMode.Carried;

    public bool isBlending { get; private set; }
}
