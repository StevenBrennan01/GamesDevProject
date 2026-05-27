using UnityEngine;

public class DoorInteraction : MonoBehaviour, IInteraction
{
    private PlayerStateController playerState;
    private BoxCollider doorCloseCollider;

    [SerializeField] private Animator anchorAnimator = null;
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip doorOpenSFX;
    [SerializeField] private AudioClip doorCloseSFX;

    [SerializeField] private bool hasDoorBlocker;
    private bool isOpen = false;

    private void Awake()
    {
        playerState = FindFirstObjectByType<PlayerStateController>();

        doorCloseCollider = GetComponent<BoxCollider>();
        doorCloseCollider.enabled = false;

        doorAudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (doorCloseCollider.enabled == false) return;

        if (playerState.CurrentMovementMode == MovementMode.FirstPerson)
        {
            doorCloseCollider.isTrigger = true;
        }
        else
        {
            doorCloseCollider.isTrigger = false;
        }
    }

    public void PerformInteraction(GameObject interactor)
    {
        BeginAnimation();
    }

    private void BeginAnimation()
    {
        if (!isOpen)
        {
            anchorAnimator.Play("DoorOpenAnim");
            isOpen = true;

            if (doorOpenSFX != null)
            {
                doorAudioSource.PlayOneShot(doorOpenSFX);
            }

            if (hasDoorBlocker)
            {
                doorCloseCollider.enabled = true;
            }
        }
        else
        {
            anchorAnimator.Play("DoorCloseAnim");
            if (doorCloseSFX != null)
            {
                doorAudioSource.PlayOneShot(doorCloseSFX);
            }
            isOpen = false;
        }
    }
}
