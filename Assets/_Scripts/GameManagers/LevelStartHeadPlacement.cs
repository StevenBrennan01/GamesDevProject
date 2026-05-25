using UnityEngine;
using System.Collections;

public class LevelStartHeadPlacement : MonoBehaviour
{
    private PlayerStateController playerState;
    [SerializeField] public HeadPlacementVolume startVolume;

    void Start()
    {
        LookForHeadPlacement();
    }

    public void LookForHeadPlacement()
    {
        StartCoroutine(LookForHeadPlacementRoutine());
    }

    private IEnumerator LookForHeadPlacementRoutine()
    {
        yield return new WaitForSeconds(.5f);

        if (playerState == null) playerState = FindFirstObjectByType<PlayerStateController>();

        GameObject startVol = GameObject.FindGameObjectWithTag("StartHeadPlacement");
        startVolume = startVol.GetComponent<HeadPlacementVolume>();

        playerState.PlaceHeadOnStart(startVolume);
        yield break;
    }
}