using Unity.Cinemachine;
using UnityEngine;

public class SignalBoost_CameraEffect : MonoBehaviour
{
    [Tooltip("Cinemachine VCam for the placed head cam")]
    [SerializeField] private CinemachineCamera placedVirtualCamera;

    private float originalFOV;
    [SerializeField] private float targetFOV = 100f;

    private float originalBloom;
    [SerializeField] private float targetBloom = 5f;

    private float originalChromAbberation;
    [SerializeField] private float targetChromAbberation = 1f;


}
