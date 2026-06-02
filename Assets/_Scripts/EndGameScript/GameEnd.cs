using UnityEngine;
using System.Collections;

public class GameEnd : MonoBehaviour
{
    private SignalManager signalManager;
    private BatteryManager batteryManager;
    private PlayerInputs playerInputs;
    private ControllerCheck controllerCheck;
    
    private string colliderTag = "Elevator";
    private string gameSceneName = "MainMenu"; //The Menu Scene Name

    [SerializeField] private float fadeInSeconds = 0.5f;
    [SerializeField] private float fadeOutSeconds = 1f;
    [SerializeField] private float waitBeforeStartingTransition = 6f;
    private void Awake()
    {
        signalManager = FindAnyObjectByType<SignalManager>();
        batteryManager = FindAnyObjectByType<BatteryManager>();
        playerInputs = FindAnyObjectByType<PlayerInputs>();
        controllerCheck = FindAnyObjectByType<ControllerCheck>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(colliderTag))
        {
            playerInputs.SetMovementLocked(true);
            signalManager.DisableSignalChecks();

            signalManager.signalParent.SetActive(false);
            batteryManager.batteryParent.SetActive(false);

            controllerCheck.interactionTipsParent.SetActive(false);

            StartCoroutine(DebounceReturnToMenu());
        }
    }

    private IEnumerator DebounceReturnToMenu()
    {
        yield return new WaitForSeconds(waitBeforeStartingTransition);

        AudioManager.instance.FadeMusic(0f, fadeOutSeconds);
        ScreenFadeManager screenFadeManager = FindAnyObjectByType<ScreenFadeManager>();

        if (screenFadeManager != null)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            screenFadeManager.TransitionToScene(gameSceneName, fadeOutSeconds: fadeOutSeconds, holdBlackSeconds: 0.25f, fadeInSeconds: fadeInSeconds);
        }
    }
}