using UnityEngine;

[RequireComponent (typeof(BoxCollider))]
public class HeadPlacementVolume : MonoBehaviour
{
    [Tooltip("The anchor that the player head will become a child of.")]
    public Transform placementAnchor;

    [Tooltip("The Visualiser that the player will see as a place point")]
    public GameObject headVisualiser;
    [SerializeField, Range(0.0f, 150f)] private int visualiserRotateSpeed;

    [Tooltip("Trigger collider to detect for player presence")]
    private BoxCollider volumeCollider;

    private string playerTag = "Player"; //The Player Tag

    [HideInInspector] public bool canPlace;

    private void Awake()
    {
        volumeCollider = GetComponent<BoxCollider>();

        headVisualiser.SetActive(true);
    }

    private void Update()
    {
        headVisualiser.transform.Rotate(Vector3.up * visualiserRotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)// Allow for placement etc.
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
