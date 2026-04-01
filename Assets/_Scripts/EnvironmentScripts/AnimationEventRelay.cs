using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private StairInteraction stairInteraction;

    void Awake()
    {
        stairInteraction = GetComponentInParent<StairInteraction>();
        if (stairInteraction == null)
        {
            Debug.LogError("AnimationEventRelay: No StairInteraction found in parent hierarchy.");
        }
    }

    public void OnStairAnimationComplete()
    {
        if (stairInteraction != null)
        {
            stairInteraction.animationFinished = true;
        }
    }
}
