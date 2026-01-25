using System.Collections;
using UnityEngine;

public class DoorInteraction : MonoBehaviour, IInteraction
{
    [SerializeField] private Animator anchorAnimator = null;

    [SerializeField] private bool hasDoorBlocker;
    [SerializeField] private bool closeDoorAfterEntry;

    private PlayerStateController playerState;

    private BoxCollider doorCloseCollider;

    [Tooltip("Match this as close to the Animation Length as possible so you can interact straight after")]
    [SerializeField, Range(0, 5)] public float interactBlockSeconds;

    private bool isOpen = false;
    private bool isAnimating = false;

    private void Awake()
    {
        playerState = FindFirstObjectByType<PlayerStateController>();

        doorCloseCollider = GetComponent<BoxCollider>();
        doorCloseCollider.enabled = false;
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
        if (isAnimating) return;

        StartCoroutine(BeginAnimation());
    }

    private IEnumerator BeginAnimation()
    {
        if (!isOpen)
        {
            isAnimating = true;
            anchorAnimator.Play("DoorOpenAnim");
            isOpen = true;

            if (hasDoorBlocker)
            {
                doorCloseCollider.enabled = true;
            }
        }
        else
        {
            isAnimating = true;
            anchorAnimator.Play("DoorCloseAnim");
            isOpen = false;
        }

        yield return new WaitForSeconds(interactBlockSeconds);
        isAnimating = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (closeDoorAfterEntry)
        {
            isAnimating = true;
            StartCoroutine(BeginAnimation());

            doorCloseCollider.enabled = false;
        }
        else return;
    }
}
