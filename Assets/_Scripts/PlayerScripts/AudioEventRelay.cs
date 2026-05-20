using UnityEngine;

public class AudioEventRelay : MonoBehaviour
{
    private PlayerAudioController playerAudioController;

    private void Awake()
    {
        if(playerAudioController == null)
        {
            playerAudioController = GetComponentInParent<PlayerAudioController>();
        }
    }

    public void FootstepAudioRelay()
    {
        playerAudioController.PlayFootStep();
    }

    public void JumpUpAudioRelay()
    {
        playerAudioController.PlayJumpUpSFX();
    }

    public void JumpLandingAudioRelay()
    {
        playerAudioController.PlayJumpLandingSFX();
    }

    public void InteractAudioRelay()
    {
        playerAudioController.PlayHeadInteractSFX();
    }

    public void LeverPullAudioRelay()
    {
        playerAudioController.PlayLeverPullSFX();
    }
}
