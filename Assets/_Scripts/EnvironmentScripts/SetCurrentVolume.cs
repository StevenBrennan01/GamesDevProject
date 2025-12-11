using UnityEngine;

[RequireComponent (typeof(HeadPlacementVolume))]
[RequireComponent (typeof(BoxCollider))]
public class SetCurrentVolume : MonoBehaviour
{
    private HeadPlacementVolume HeadPlacementVolume;

    private string playerTag = "Player"; //The Player Tag

    private void Awake()
    {
        HeadPlacementVolume = GetComponent<HeadPlacementVolume>();
        var boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;
        var state = other.GetComponent<PlayerStateController>();

        if (state != null)
        {
            state.SetCurrentPlacementVolume(this.HeadPlacementVolume);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;
        var state = other.GetComponent<PlayerStateController>();

        if (state != null)
        {
            state.SetCurrentPlacementVolume(null);
        }
    }
}
