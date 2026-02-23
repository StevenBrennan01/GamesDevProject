using Unity.Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
public class HeadBobNoiseDriver : MonoBehaviour
{
    [Header("References")]
    private PlayerStateController playerStateController;
    private PlayerInputs playerInputs;

    [SerializeField] private CinemachineCamera FirstPersonVirtualCamera;
    private CinemachineBasicMultiChannelPerlin VCamPerlin;

    [Header("Noise Tuning (Comfort)")]
    [Tooltip("Max amplitude (gain) when moving at full input.")]
    [SerializeField, Range(0f, 1.5f)] private float maxAmplitudeGain = 0.25f;

    [Tooltip("Max frequency when moving at full input.")]
    [SerializeField, Range(0f, 5f)] private float maxFrequencyGain = 1.4f;

    [Tooltip("How quickly the bob ramps in/out.")]
    [SerializeField, Range(0.01f, 0.5f)] private float gainSmoothTime = 0.12f;

    [Tooltip("Small deadzone so tiny stick/mouse drift doesn't cause bob.")]
    [SerializeField, Range(0f, 0.25f)] private float moveDeadzone = 0.05f;

    // [Header("Optional multipliers")]
    // [Tooltip("Reduce bob while crouching (if you want).")]
    // [SerializeField, Range(0f, 1f)] private float crouchMultiplier = 0.6f;
    private float amplitudeVel;
    private float frequencyVel;

    private float currentAmplitude;
    private float currentFrequency;

    private void Awake()
    {
        playerStateController = FindAnyObjectByType<PlayerStateController>();
        playerInputs = FindAnyObjectByType<PlayerInputs>();

        if (FirstPersonVirtualCamera == null) GetComponent<CinemachineCamera>();

        if(FirstPersonVirtualCamera != null)
        {
            VCamPerlin = FirstPersonVirtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    private void Update()
    {
        if(playerStateController == null || playerInputs == null || VCamPerlin == null) return;

        bool shouldBob = 
            playerStateController.CurrentCameraMode == CameraMode.Carried
            && playerStateController.CurrentMovementMode == MovementMode.FirstPerson
            && !playerStateController.isBlending
            && !playerInputs.inputLocked;

        float move01 = Mathf.Clamp01(playerInputs.Move.magnitude);
        if(move01 < moveDeadzone) move01 = 0f;

        //float crouchScale = playerInputs.isCrouching ? crouchMultiplier : 1f;

        float targetAmplitude = shouldBob ? (maxAmplitudeGain * move01 /* * crouchScale */) : 0f;
        float targetFrequency = shouldBob ? Mathf.Lerp(0f, maxFrequencyGain, move01 /* * crouchScale */) : 0f;

        currentAmplitude = Mathf.SmoothDamp(currentAmplitude, targetAmplitude, ref amplitudeVel, gainSmoothTime);
        currentFrequency = Mathf.SmoothDamp(currentFrequency, targetFrequency, ref frequencyVel, gainSmoothTime);

        VCamPerlin.AmplitudeGain = currentAmplitude;
        VCamPerlin.FrequencyGain = currentFrequency;
    }
}
