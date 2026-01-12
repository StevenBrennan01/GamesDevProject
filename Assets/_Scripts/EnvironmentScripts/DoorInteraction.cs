using System.Collections;
using UnityEngine;

public class DoorInteraction : MonoBehaviour, IInteraction
{
    [SerializeField] private Animator metalDoor = null;

    [SerializeField] private bool hasDoorBlocker;

    private PlayerStateController playerState;

    private BoxCollider doorCloseCollider;

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

        StartCoroutine(IsAnimating());
    }

    private IEnumerator IsAnimating()
    {
        if (!isOpen)
        {
            isAnimating = true;
            metalDoor.Play("DoorOpenAnim");
            isOpen = true;

            if (hasDoorBlocker)
            {
                doorCloseCollider.enabled = true;
            }
        }
        else
        {
            isAnimating = true;
            metalDoor.Play("DoorCloseAnim");
            isOpen = false;
        }

        yield return new WaitForSeconds(2);
        isAnimating = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        isAnimating = true;
        metalDoor.Play("DoorCloseAnim");
        isOpen = false;
    }

    //[Header("Door Swivel Point")]
    //[SerializeField] private Transform doorAnchor;

    //[Header("Rotation")]
    //private Vector3 localRotateAxis = Vector3.up;
    //[Tooltip("The angle in degrees for the door to be closed")]
    //[SerializeField] private float doorClosedAngle = 0f;
    //[Tooltip("The angle in degrees for the door to be open")]
    //[SerializeField] private float doorOpenAngle = 95f;

    //[Header("Animation")]
    //[SerializeField] private float animateSeconds = 0.4f;
    //[SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    //private bool isOpen;
    //private bool isAnimating;
    //private Quaternion openRot;
    //private Quaternion closedRot;

    //private void Awake()
    //{
    //    if (doorAnchor == null) 
    //    { 
    //        Debug.LogWarning($"No anchor set on '{this.gameObject}': door cannot open without one");
    //        enabled = false;
    //        return;
    //    }
    //}

    //public void PerformInteraction(GameObject interactor)
    //{
    //    if (doorAnchor == null) return;
    //    if (isAnimating) return;

    //    isOpen = !isOpen;

    //}
}
