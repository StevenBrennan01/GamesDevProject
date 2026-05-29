using System.Collections;
using UnityEngine;

public class StairInteraction : MonoBehaviour, IInteraction
{
    [Header("References and Settings")]
    [SerializeField] private Animator anchorAnimator;
    [SerializeField] private bool stairsToGoDown;
    [SerializeField] private bool stairsToGoUp;

    [Header("Blocker References")]
    [SerializeField] private GameObject bannisterBlocker1;
    [SerializeField] private GameObject bannisterBlocker2;
    [SerializeField] private GameObject stairBlocker;
    public bool stairsAreUp;
    public bool stairsAreDown;
    public bool animationFinished;

    [Header("-= SFX & Audio Sources =-")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip stairMoveSFX;

    private Coroutine animationFinishedCoroutine;

    private void Awake()
    {
        if (anchorAnimator == null)
        {
            anchorAnimator = GetComponentInChildren<Animator>();
        }

        if(stairsToGoUp)
        {
            stairsAreDown = true;
            bannisterBlocker1.SetActive(false);
            bannisterBlocker2.SetActive(false);
            stairBlocker.SetActive(false);
        }
        else if (stairsToGoDown)
        {
            stairsAreUp = true;
            bannisterBlocker1.SetActive(true);
            bannisterBlocker2.SetActive(true);
            stairBlocker.SetActive(true);
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void PerformInteraction(GameObject interactor)
    {
        BeginAnimation();
    }

    private IEnumerator animationFinishedDelay()
    {
        yield return new WaitUntil(() => animationFinished);

        if(stairsAreUp)
        {
            bannisterBlocker1.SetActive(true);
            bannisterBlocker2.SetActive(true);
            stairBlocker.SetActive(true);
        }
        else if(stairsAreDown)
        {
            bannisterBlocker1.SetActive(false);
            bannisterBlocker2.SetActive(false);
            stairBlocker.SetActive(false);
        }

        animationFinished = false;
    }

    private void BeginAnimation()
    {
        if (stairsAreUp)
        {
            anchorAnimator.Play("StairsDescending");
            stairsAreUp = false;
            
            if (animationFinishedCoroutine != null) StopCoroutine(animationFinishedCoroutine);
            animationFinishedCoroutine = StartCoroutine(animationFinishedDelay());

            stairsAreDown = true;
        }

        else
        {
            anchorAnimator.Play("StairsAscending");
            stairsAreDown = false;

            if (animationFinishedCoroutine != null) StopCoroutine(animationFinishedCoroutine);
            animationFinishedCoroutine = StartCoroutine(animationFinishedDelay());

            stairsAreUp = true;
        }
    }

    public void PlayStairMoveSFX()
    {
        if(audioSource == null || stairMoveSFX == null) return;
        
        audioSource.PlayOneShot(stairMoveSFX);
    }
}