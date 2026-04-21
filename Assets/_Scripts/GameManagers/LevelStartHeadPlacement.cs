using UnityEngine;

public class LevelStartHeadPlacement : MonoBehaviour
{
    private PlayerStateController playerState;
    [SerializeField] public HeadPlacementVolume startVolume;

    private void Awake()
    {
        if (playerState == null) playerState = FindFirstObjectByType<PlayerStateController>();
    }

    void Start()
    {
        if (startVolume != null)
        {
            playerState.PlaceHeadOnStart(startVolume);
        }
    }
}