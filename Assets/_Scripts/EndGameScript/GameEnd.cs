using UnityEngine;
using System.Collections;

public class GameEnd : MonoBehaviour
{
    private string colliderTag = "Elevator"; //The Player Tag
    private string gameSceneName = "MainMenu"; //The Game Scene Name

    [SerializeField] private float fadeInSeconds = 0.5f;
    [SerializeField] private float fadeOutSeconds = 1f;
    [SerializeField] private float waitBeforeStartingTransition = 4f;
    [SerializeField] private BoxCollider endCollider;

    private void Awake()
    {
        endCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == colliderTag)
        {
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