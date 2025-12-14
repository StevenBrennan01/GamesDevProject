using UnityEngine;

[RequireComponent (typeof(BoxCollider))]
public class HeadPlacementVolume : MonoBehaviour
{
    [Tooltip("The anchor that the player head will become a child of.")]
    public Transform placementAnchor;

    [Tooltip("Trigger collider to detect for player presence")]
    private BoxCollider volumeCollider;

    private string playerTag = "Player"; //The Player Tag
    public bool canPlace;

    private void Awake()
    {
        volumeCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerStay(Collider other)// Allow for placement etc.
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;
        if (other.tag == playerTag)
        {
            canPlace = true;
        }
    }

    private void OnTriggerExit(Collider other) // Disallow for placement etc.
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;
        if (other.tag == playerTag)
        {
            canPlace = false;
        }
    }
}
