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
    [SerializeField] private float fadeOutSeconds = 1f;

    private BatteryManager batteryManager;
    private PlayerStateController playerStateController;

    private void Awake()
    {
        batteryManager = FindAnyObjectByType<BatteryManager>();
        playerStateController = FindAnyObjectByType<PlayerStateController>();
    }

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
        if(Input.GetKeyDown(KeyCode.R))
        {
            ReloadCurrentLevel();
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

            if(loadedScene.name != levelSceneNames[0])
            {
                batteryManager.SetBatteryFull();
            }
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
        AudioManager.instance.FadeMusic(0f, fadeOutSeconds);
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

        StartCoroutine(ReloadLevelCoroutine());
    }

    private IEnumerator ReloadLevelCoroutine()
    {
        Debug.Log("Reloading current level");

        PlayerInputs inputs = FindAnyObjectByType<PlayerInputs>();
        if (inputs != null)
        {
            inputs.SetMovementAndCameraLocked(true);
        }

        AudioManager.instance.FadeMusic(0f, fadeOutSeconds);
        FadeOutCurrentLevel();

        yield return new WaitForSeconds(sceneSwapDelay);

        yield return StartCoroutine(SwapLevelRoutine(levelSceneNames[currentLevelIndex]));

        ResetPersistentPlayerState();
        ResetPlayerPosToSpawn();
        AudioManager.instance.ApplySceneMusic(currentLevelSceneName);

        Debug.Log("Reload Starting");
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

        yield return StartCoroutine(LoadLevelRoutine(nextSceneName));
    }

    private void ResetPlayerPosToSpawn()
    {
        Transform spawnPoint = GameObject.FindWithTag("LevelSpawnPoint")?.transform;
        if (playerStateController != null && spawnPoint != null)
        {
            playerStateController.ResetForLevelSpawn(spawnPoint);
        }
    }

    private void ResetPersistentPlayerState()
    {
        BatteryManager battery = FindAnyObjectByType<BatteryManager>();
        if (battery != null)
        {
            battery.SetBatteryFull();
        }

        SignalManager signal = FindAnyObjectByType<SignalManager>();
        if (signal != null)
        {
            signal.headSignalInitialized = false;
        }
    }
}
