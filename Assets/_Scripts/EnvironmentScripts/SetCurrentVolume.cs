using UnityEngine;

[RequireComponent (typeof(HeadPlacementVolume))]
[RequireComponent (typeof(BoxCollider))]
public class SetCurrentVolume : MonoBehaviour
{
    private HeadPlacementVolume headPlacementVolume;
    private PlayerStateController playerStateController;
    private ControllerCheck controllerCheck;

    private string playerTag = "Player"; //The Player Tag

    private void Awake()
    {
        headPlacementVolume = GetComponent<HeadPlacementVolume>();
        playerStateController = FindAnyObjectByType<PlayerStateController>();
        controllerCheck = FindAnyObjectByType<ControllerCheck>();

        var boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;
        var state = other.GetComponent<PlayerStateController>();

        if (state != null)
        {
            state.SetCurrentPlacementVolume(this.headPlacementVolume);
        }

        if(playerStateController != null && playerStateController.CurrentMovementMode == MovementMode.FirstPerson)
        {
            if (controllerCheck.isUsingController)
            {
                controllerCheck.interactKeyboard_Element.SetActive(false);
                controllerCheck.interactController_Element.SetActive(true);
            }
            else
            {
                controllerCheck.interactController_Element.SetActive(false);
                controllerCheck.interactKeyboard_Element.SetActive(true);
            }
        }
        else if(playerStateController != null && playerStateController.CurrentMovementMode == MovementMode.SecondPerson)
        {
            if(playerStateController.placedHeadVolume == this.headPlacementVolume)
            {
                if (controllerCheck.isUsingController)
                {
                    controllerCheck.interactKeyboard_Element.SetActive(false);
                    controllerCheck.interactController_Element.SetActive(true);
                }
                else
                {
                    controllerCheck.interactController_Element.SetActive(false);
                    controllerCheck.interactKeyboard_Element.SetActive(true);
                }
            }
            else
            {
                if (controllerCheck != null)
                {
                    controllerCheck.interactKeyboard_Element.SetActive(false);
                    controllerCheck.interactController_Element.SetActive(false);
                }
            }
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

        if (controllerCheck != null)
        {
            controllerCheck.interactKeyboard_Element.SetActive(false);
            controllerCheck.interactController_Element.SetActive(false);
        }
    }
}
