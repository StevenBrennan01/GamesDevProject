using UnityEngine;

public class LevelStartHeadPlacement : MonoBehaviour
{
    private PlayerStateController playerState;
    [SerializeField] private HeadPlacementVolume startVolume;

    private void Awake()
    {
        if (playerState == null) playerState = FindFirstObjectByType<PlayerStateController>();
        if (startVolume == null)
        {
            Debug.LogError("volume not found");
            return;
        }
    }

    void Start()
    {
        playerState.PlaceHeadOnStart(startVolume);
    }
}