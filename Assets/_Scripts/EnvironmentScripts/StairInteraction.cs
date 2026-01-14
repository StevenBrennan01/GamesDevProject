using System.Collections;
using UnityEngine;

public class StairInteraction : MonoBehaviour, IInteraction
{
    [SerializeField] private Animator anchorAnimator;
    [SerializeField] private bool stairsToGoDown;
    [SerializeField] private bool stairsToGoUp;

    [Tooltip("Match this as close to the Animation Length as possible so you can interact straight after")]
    [SerializeField, Range(0,5)] public float interactBlockSeconds;

    private bool stairsAreUp = false;
    private bool isAnimating = false;

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
        if (isAnimating) return;
        StartCoroutine(BeginAnimation());
    }

    private IEnumerator BeginAnimation()
    {
        if (stairsAreUp)
        {
            isAnimating = true;
            anchorAnimator.Play("StairsDescending");
            stairsAreUp = false;
        }

        else
        {
            isAnimating = true;
            anchorAnimator.Play("StairsAscending");
            stairsAreUp = true;
        }

        yield return new WaitForSeconds(interactBlockSeconds);
        isAnimating = false;
    }
}
