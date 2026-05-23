using UnityEngine;

public class DoorInteraction : MonoBehaviour, IInteraction
{
    [SerializeField] private Animator anchorAnimator = null;
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip doorOpenSFX;
    [SerializeField] private AudioClip doorCloseSFX;

    [SerializeField] private bool hasDoorBlocker;
    [SerializeField] private bool closeDoorAfterEntry;
    //[SerializeField] private float closerDoorAfter = 4f;

    private PlayerStateController playerState;

    private BoxCollider doorCloseCollider;

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

    private void OnTriggerEnter(Collider other)
    {
        // if (closeDoorAfterEntry)
        // {
        //     StartCoroutine(DoorCloseAfterSeconds(closerDoorAfter));
        // }
        // else return;
    }

    // private IEnumerator DoorCloseAfterSeconds(float seconds)
    // {
    //     doorCloseCollider.enabled = false;
    //     yield return new WaitForSeconds(seconds);
    //     BeginAnimation();
    // }
}
