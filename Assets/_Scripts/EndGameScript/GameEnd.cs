using UnityEngine;

public class GameEnd : MonoBehaviour
{
    private string playerTag = "Player"; //The Player Tag
    private string gameSceneName = "MainMenu"; //The Player Tag

    [SerializeField] private float fadeInSeconds = 0.5f;
    [SerializeField] private float fadeOutSeconds = 1f;
    [SerializeField] private BoxCollider endCollider;

    private void Awake()
    {
        endCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == playerTag)
        {
            Debug.Log("Fade should start");

            AudioManager.instance.FadeMusic(0f, fadeOutSeconds);

            //StartCoroutine(ReturnMouseControls());

            if (ScreenFadeManager.instance != null)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                ScreenFadeManager.instance.TransitionToScene(gameSceneName, fadeOutSeconds: fadeOutSeconds, holdBlackSeconds: 0.25f, fadeInSeconds: fadeInSeconds);
            }
        }
    }

    //private IEnumerator ReturnMouseControls()
    //{
    //    yield return new WaitForSeconds(fadeOutSeconds);

    //    Cursor.visible = true;
    //    Cursor.lockState = CursorLockMode.None;
    //}
}