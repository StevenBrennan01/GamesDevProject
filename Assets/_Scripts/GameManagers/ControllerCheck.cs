using UnityEngine;

public class ControllerCheck : MonoBehaviour
{
    [SerializeField] public GameObject interactionTipsParent;
    [SerializeField] public GameObject interactKeyboard_Element;
    [SerializeField] public GameObject interactController_Element;
    public bool isUsingController;

    private void Awake()
    {
        interactionTipsParent.SetActive(true);

        interactController_Element.SetActive(false);
        interactKeyboard_Element.SetActive(false);
    }
}
