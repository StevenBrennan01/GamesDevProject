using UnityEngine;

public class LevelStartHeadPlacement : MonoBehaviour
{
    private PlayerStateController playerState;
    [SerializeField] public HeadPlacementVolume startVolume;

    private void Awake()
    {
        if (playerState == null) playerState = FindFirstObjectByType<PlayerStateController>();
        if (startVolume == null)
        {
            Debug.LogWarning("Second Person start volume not found");
        }
    }

    void Start()
    {
        playerState.PlaceHeadOnStart(startVolume);
    }
}