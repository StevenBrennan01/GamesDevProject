using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof(Collider))]
public class InteractionVolume : MonoBehaviour
{
    [Tooltip("Object/Script being interacted with that utilises the IInteraction contract")]
    [SerializeField] private MonoBehaviour[] interactionBehaviours;
    private IInteraction[] interactions;
    private Collider volumeTrigger;

    [Header("Interaction Settings")]
    [Space(5)]
    private bool isLeverPulled = false;

    public bool canPull /* { get; private set; } */ = true; 
    private Animator leverAnim = null;

    [Space(5)]
    [Header("Lever Settings")]
    [Space(5)]
    [SerializeField] private GameObject LEDLight;
    [SerializeField] private Material OnMat;
    [SerializeField] private Material OffMat; // Make it so that it switches based on lever being up or down

    [Header("Execution")]
    [Tooltip("If true, run interactions one after another with a delay; otherwise run all immediately.")]
    [SerializeField] private bool runSequentially = false;

    [Tooltip("Delay between sequential interactions (seconds). Only used if runSequentially is true.")]
    [SerializeField, Range(0f, 5f)] private float sequentialDelaySeconds = 0f;

    [Tooltip("If true, this volume can only be executed once.")]
    [SerializeField] private bool executeOnce = false;

    [SerializeField] private bool resetInteraction;
    [SerializeField, Range(0, 10)] private float resetAfter;

    [Space(5)]
    [Header("Lever and Interaction Block Seconds")]
    [SerializeField, Range(0f, 15f)] public float cooldownSeconds = 0f;

    private float lastExecuteTime = -Mathf.Infinity;
    private bool hasExecutedOnce = false;

    private void Awake()
    {
        volumeTrigger = GetComponent<Collider>();
        leverAnim = GetComponentInChildren<Animator>();

        if (interactionBehaviours != null && interactionBehaviours.Length > 0)
        {
            interactions = interactionBehaviours
                .Where(b => b != null) // Filter out null entries to avoid errors
                .Select(b => b as IInteraction) // Cast to IInteraction, will be null if it doesn't implement the interface
                .Where(i => i != null) // Finally, filter out any that don't implement IInteraction
                .ToArray();

            foreach (var b in interactionBehaviours)
            {
                if (b != null && !(b is IInteraction))
                {
                    Debug.LogWarning($"InteractionVolume '{name}': Assigned behaviour '{b.GetType().Name}' does not implement IInteraction and will be ignored.", this);
                }
            }
        }

        else
        {
            interactions = System.Array.Empty<IInteraction>();
        }

        if (interactionBehaviours == null || interactionBehaviours.Length == 0)
        {
            Debug.LogError($"InteractionZone '{name}': Requires a script/interaction behaviour");
        }
    }

    public void ExecuteInteraction(GameObject interactor)
    {
        if (executeOnce && hasExecutedOnce) return;

        if (Time.time < lastExecuteTime + cooldownSeconds) return;

        if (interactions == null || interactions.Length == 0) return;

        lastExecuteTime = Time.time;
        if (runSequentially && sequentialDelaySeconds > 0f) // Delay Interactions
        {
            StartCoroutine(ExecuteSequentially(interactor));
        }
        else // Run all interactions immediately
        {
            for (int i = 0; i < interactions.Length; i++)
            {
                try
                {
                    interactions[i].PerformInteraction(interactor);

                    if (!isLeverPulled && canPull)
                    {
                        leverAnim.Play("LeverPullAnim");

                        StartCoroutine(LeverPullCountdown());
                        isLeverPulled = true;
                    }
                    else if (isLeverPulled && canPull)
                    {
                        leverAnim.Play("LeverPushAnim");

                        StartCoroutine(LeverPullCountdown());
                        isLeverPulled = false;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"InteractionVolume '{name}': Error executing interaction {i}: {ex}", this);
                }
            }
        }

        if (executeOnce) hasExecutedOnce = true;
        if (resetInteraction)
        {
            StartCoroutine(ResetInteractionAfter(resetAfter, interactor));
        }
    }

    private IEnumerator ResetInteractionAfter(float seconds, GameObject interactor)
    {
        yield return new WaitForSeconds(seconds);

        foreach (var behaviour in interactionBehaviours)
        {
            if (behaviour is IInteraction interactable)
            {
                interactable.PerformInteraction(interactor);
            }
        }
    }

    private IEnumerator ExecuteSequentially(GameObject interactor)
    {
        for (int i = 0; i < interactions.Length; i++)
        {
            IInteraction interaction = interactions[i];
            if(interaction == null) continue;

            try
            {
                interaction.PerformInteraction(interactor);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"InteractionVolume '{name}': Error executing interaction {i}: {ex}", this);
            }

            if (i < interactions.Length - 1 && sequentialDelaySeconds > 0)
            {
                yield return new WaitForSeconds(sequentialDelaySeconds);
            }
        }
    }

    public IEnumerator LeverPullCountdown()
    {
        yield return new WaitForSeconds(0.05f); // Short debounce to allow interact animation to trigger before blocking interaction
        canPull = false;

        yield return new WaitForSeconds(cooldownSeconds);
        canPull = true;
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
