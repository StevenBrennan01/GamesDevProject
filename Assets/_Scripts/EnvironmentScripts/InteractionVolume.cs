using UnityEngine;

[RequireComponent (typeof(Collider))]
//[RequireComponent (typeof(Rigidbody))]
public class InteractionVolume : MonoBehaviour
{
    [Tooltip("Object/Script being interacted with that utilises the IInteraction contract")]
    [SerializeField] private MonoBehaviour interactionBehaviour;

    private IInteraction interaction;

    private Collider volumeTrigger;
    //private Rigidbody volumeRB;

    private void Awake()
    {
        interaction = interactionBehaviour as IInteraction;

        volumeTrigger = GetComponent<Collider>();
        if (!volumeTrigger.isTrigger)
        {
            volumeTrigger.isTrigger = true;
        }

        if (interactionBehaviour == null || interaction == null)
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
