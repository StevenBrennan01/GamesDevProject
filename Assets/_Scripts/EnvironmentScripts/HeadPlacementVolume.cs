using UnityEngine;

[RequireComponent (typeof(BoxCollider))]
public class HeadPlacementVolume : MonoBehaviour
{
    [Tooltip("The anchor that the player head will become a child of.")]
    [SerializeField] private Transform placementAnchor;

    [Tooltip("Trigger collider to detect for player presence")]
    private BoxCollider volumeCollider;

    private string playerTag = "Player"; //The Player Tag

    private void Awake()
    {
        volumeCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;
        // Allow for placement etc.
    }


    private void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;
        // Disallow for placement etc.
    }


}
