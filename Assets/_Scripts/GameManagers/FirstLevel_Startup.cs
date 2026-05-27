using UnityEngine;
using System.Collections;

public class FirstLevel_Startup : MonoBehaviour
{
    private GameObject Elevator;
    void Start()
    {
        StartCoroutine(BeginMovingElevator());
    }

    private IEnumerator BeginMovingElevator()
    {
        yield return new WaitForSeconds(2f);

        if (Elevator == null) Elevator = GameObject.FindGameObjectWithTag("Elevator");

        if (Elevator != null)
        {
            ElevatorMovement elevatorMovement = Elevator.GetComponent<ElevatorMovement>();
            if (elevatorMovement != null)
            {
                elevatorMovement.MoveElevatorAutomatic(4.5f);
            }
        }
    }
}
