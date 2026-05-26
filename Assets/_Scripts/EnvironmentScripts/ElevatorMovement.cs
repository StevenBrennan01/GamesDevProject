using UnityEngine;
using System.Collections;

public class ElevatorMovement : MonoBehaviour, IInteraction
{
    [SerializeField, Range(0f, 10f)] private float moveSpeed = 2f;
    [SerializeField] private Transform targetPointTop;
    [SerializeField] private Transform targetPointBottom;

    public bool elevatorIsDown = false;
    public bool elevatorIsUp = false;
    private bool isMoving;

    public void PerformInteraction(GameObject interactor)
    {
        if (isMoving) return;

        if(elevatorIsDown)
        {
            StartCoroutine(MoveElevatorRoutine(targetPointTop));
            elevatorIsUp = true;
            elevatorIsDown = false;
        }
        else
        {
            StartCoroutine(MoveElevatorRoutine(targetPointBottom));
            elevatorIsDown = true;
            elevatorIsUp = false;
        }
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
    }
}
