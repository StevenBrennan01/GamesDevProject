using UnityEngine;

public class SFXEventRelay : MonoBehaviour
{
    private StairInteraction stairInteraction;
    void Awake()
    {
        stairInteraction = GetComponentInParent<StairInteraction>();
    }

    public void StairMovingSFX_Relay()
    {
        if (stairInteraction != null)
        {
            stairInteraction.PlayStairMoveSFX();
        }
    }
}
