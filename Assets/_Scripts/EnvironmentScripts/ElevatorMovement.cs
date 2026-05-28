using UnityEngine;
using System.Collections;

public class ElevatorMovement : MonoBehaviour, IInteraction
{
    [SerializeField, Range(0f, 10f)] private float moveSpeed = 2f;
    [SerializeField] private Transform targetPointTop;
    [SerializeField] private Transform targetPointBottom;

    [SerializeField] private AudioSource consistentAudioSource;
    [SerializeField] private AudioSource oneShotAudioSource;
    [SerializeField] private AudioClip elevatorMoveSFX;
    [SerializeField] private AudioClip elevatorStopSFX;

    public bool elevatorIsDown = false;
    public bool elevatorIsUp = false; 
    private bool isMoving;

    private void Awake()
    {
        consistentAudioSource.Stop();
        oneShotAudioSource.Stop();
    }

    public void MoveElevatorAutomatic(float delaySeconds)
    {
        if (isMoving) return;

        if(elevatorIsDown)
        {
            StartCoroutine(DelayedMoveElevatorRoutine(targetPointTop, delaySeconds));

            consistentAudioSource.clip = elevatorMoveSFX;
            consistentAudioSource.Play();

            elevatorIsUp = true;
            elevatorIsDown = false;
        }
        else
        {
            StartCoroutine(DelayedMoveElevatorRoutine(targetPointBottom, delaySeconds));

            // consistentAudioSource.clip = elevatorMoveSFX;
            // consistentAudioSource.Play();

            elevatorIsDown = true;
            elevatorIsUp = false;
        }
    }

    public void PerformInteraction(GameObject interactor)
    {
        if (isMoving) return;

        if(elevatorIsDown)
        {
            StartCoroutine(MoveElevatorRoutine(targetPointTop));

            consistentAudioSource.clip = elevatorMoveSFX;
            consistentAudioSource.Play();

            elevatorIsUp = true;
            elevatorIsDown = false;
        }
        else
        {
            StartCoroutine(MoveElevatorRoutine(targetPointBottom));

            consistentAudioSource.clip = elevatorMoveSFX;
            consistentAudioSource.Play();

            elevatorIsDown = true;
            elevatorIsUp = false;
        }
    }

    private IEnumerator DelayedMoveElevatorRoutine(Transform targetPoint, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        yield return MoveElevatorRoutine(targetPoint);
    }

    private IEnumerator MoveElevatorRoutine(Transform targetPoint)
    {
        while(transform.position != targetPoint.position)
        {
            isMoving = true;
            transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        isMoving = false;
        consistentAudioSource.Stop();
        oneShotAudioSource.PlayOneShot(elevatorStopSFX);
    }
}
