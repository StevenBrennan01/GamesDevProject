using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoadManager : MonoBehaviour
{
    [Header("Level Loading Order")]
    [SerializeField] private string[] levelSceneNames;

    private int currentLevelIndex = -1;
    private string currentLevelSceneName;

    [SerializeField] private float sceneSwapDelay = 2f;

    private void Start()
    {
        if(levelSceneNames == null || levelSceneNames.Length == 0)
        {
            Debug.LogError("LevelLoadManager: No level scene names assigned in the inspector.");
            return;
        }

        StartCoroutine(LoadLevelRoutine(levelSceneNames[0]));
        currentLevelIndex = 0;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine(LoadNextLevel());
        }
    }

    private IEnumerator LoadLevelRoutine(string sceneName)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            yield return null;
        }

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        if (loadedScene.IsValid() && loadedScene.isLoaded)
        {
            SceneManager.SetActiveScene(loadedScene);
            currentLevelSceneName = sceneName;
        }
        else
        {
            Debug.LogError($"LevelLoadManager: Failed to load scene '{sceneName}'.");
        }
    }

    // private IEnumerator FadeOutCurrentLevel(float fadeSeconds)
    // {
    //     ScreenFadeManager.instance.TransitionToNextScene();
    //     yield return new WaitForSeconds(fadeSeconds);
    // }

    private void FadeOutCurrentLevel()
    {
        ScreenFadeManager.instance.TransitionToNextScene();
    }

    public IEnumerator LoadNextLevel()
    {
        FadeOutCurrentLevel();
        yield return new WaitForSeconds(sceneSwapDelay);

        if (currentLevelIndex < 0 || currentLevelIndex >= levelSceneNames.Length) yield break;

        int nextLevelIndex = currentLevelIndex + 1;
        if (nextLevelIndex >= levelSceneNames.Length) yield break;

        StartCoroutine(SwapLevelRoutine(levelSceneNames[nextLevelIndex]));
        currentLevelIndex = nextLevelIndex;
    }

    public void ReloadCurrentLevel()
    {
        if (currentLevelIndex < 0 || currentLevelIndex >= levelSceneNames.Length) return;
        StartCoroutine(SwapLevelRoutine(levelSceneNames[currentLevelIndex]));
    }

    private IEnumerator SwapLevelRoutine(string nextSceneName)
    {
        if (!string.IsNullOrEmpty(currentLevelSceneName))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentLevelSceneName);
            while (unloadOp != null && !unloadOp.isDone)
            {
                yield return null;
            }
        }

        yield return LoadLevelRoutine(nextSceneName);
    }
}
