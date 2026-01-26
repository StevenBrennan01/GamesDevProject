using UnityEngine;

public class StairInteraction : MonoBehaviour, IInteraction
{
    [SerializeField] private Animator anchorAnimator;
    [SerializeField] private bool stairsToGoDown;
    [SerializeField] private bool stairsToGoUp;

    [Tooltip("Match this as close to the Animation Length as possible so you can interact straight after")]

    private bool stairsAreUp = false;

    private void Awake()
    {
        if (anchorAnimator == null)
        {
            anchorAnimator = GetComponentInChildren<Animator>();
        }

        if (stairsToGoDown)
        {
            stairsAreUp = true;
        }
    }

    public void PerformInteraction(GameObject interactor)
    {
        BeginAnimation();
    }

    private void BeginAnimation()
    {
        if (stairsAreUp)
        {
            anchorAnimator.Play("StairsDescending");
            stairsAreUp = false;
        }

        else
        {
            anchorAnimator.Play("StairsAscending");
            stairsAreUp = true;
        }
    }
}
