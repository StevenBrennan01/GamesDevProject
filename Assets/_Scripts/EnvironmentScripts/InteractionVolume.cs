using System.Collections;
using UnityEngine;

[RequireComponent (typeof(Collider))]
//[RequireComponent (typeof(Rigidbody))]
public class InteractionVolume : MonoBehaviour
{
    [Tooltip("Object/Script being interacted with that utilises the IInteraction contract")]
    [SerializeField] private MonoBehaviour interactionObject;
    private IInteraction interaction;
    private Collider volumeTrigger;
    //private Rigidbody volumeRB;

    [Header("Interaction Settings")]
    [Space(5)]
    [SerializeField] private bool isDoorInteraction;
    private bool isLeverPulled = false;
    private bool canPull = true;
    private Animator leverAnim = null;

    [SerializeField] private bool isPlatformInteraction;

    [SerializeField, Range(0, 5)] private float interactBlockSeconds = 2f;

    [Space(5)]
    [Header("Lever Settings")]
    [Space(5)]
    [SerializeField] private GameObject LEDLight;
    [SerializeField] private Material OnMat;
    [SerializeField] private Material OffMat; // Make it so that it switches based on lever being up or down

    private void Awake()
    {
        interaction = interactionObject as IInteraction;

        leverAnim = GetComponentInChildren<Animator>();

        volumeTrigger = GetComponent<Collider>();
        if (!volumeTrigger.isTrigger)
        {
            volumeTrigger.isTrigger = true;
        }

        if (interactionObject == null || interaction == null)
        {
            Debug.LogError($"InteractionZone '{name}': Requires a script/interaction behaviour");
        }

        //volumeRB = GetComponent<Rigidbody>();
        //if (volumeRB != null)
        //{
        //    volumeRB.isKinematic = true;
        //    volumeRB.useGravity = false;
        //}
    }

    //public bool HasValidInteraction => interaction != null;

    public void ExecuteInteraction(GameObject interactor)
    {
        if (interaction != null)
        {
            interaction.PerformInteraction(interactor);

            if (isDoorInteraction)
            {
                if (!isLeverPulled && canPull)
                {
                    leverAnim.Play("LeverPullAnim");
                    canPull = false;

                    StartCoroutine(LeverPullCountdown());
                    isLeverPulled = true;
                }
                else if (isLeverPulled && canPull)
                {
                    leverAnim.Play("LeverPushAnim");
                    canPull = false;

                    StartCoroutine(LeverPullCountdown());
                    isLeverPulled = false;
                }
            }

            else
            {
                // Platform interactions and stuff
            }
        }
    }

    private IEnumerator LeverPullCountdown()
    {
        if (canPull == false)
        {
            yield return new WaitForSeconds(interactBlockSeconds);
            canPull = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var playerInteractions = other.GetComponentInParent<PlayerInteractions>();
        if (playerInteractions != null)
        {
            playerInteractions.SetCurrentZone(this);
            // Optional: show prompt UI here via a separate component or event.
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var playerInteractions = other.GetComponentInParent<PlayerInteractions>();
        if (playerInteractions != null)
        {
            playerInteractions.ClearCurrentZone(this);
            // Optional: hide prompt UI here.
        }
    }
}
