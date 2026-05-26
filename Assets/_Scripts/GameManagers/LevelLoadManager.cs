using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoadManager : MonoBehaviour
{
    private BatteryManager batteryManager;
    private PlayerStateController playerStateController;
    private LevelStartHeadPlacement levelStartHeadPlacement;

    [Header("Level Loading Order")]
    [SerializeField] private string[] levelSceneNames;
    private int currentLevelIndex = -1;
    private string currentLevelSceneName;

    [SerializeField] private float sceneSwapDelay = 2f;
    [SerializeField] private float fadeOutSeconds = 1f;

    private void Awake()
    {
        batteryManager = FindAnyObjectByType<BatteryManager>();
        playerStateController = FindAnyObjectByType<PlayerStateController>();
        levelStartHeadPlacement = FindAnyObjectByType<LevelStartHeadPlacement>();
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

        // if we are on the duplicate intro scene, set currentLevelIndex back to 0 so that next level loads correctly
        // if(levelSceneNames[currentLevelIndex] == levelSceneNames[1]) // replace 1 with the actual duplicate scene index
        // {
        //     currentLevelIndex = 0;
        // }

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

        StartCoroutine(DebounceHeadReturnToPlayer());

        yield return new WaitForSeconds(sceneSwapDelay);

        // check if currentLevelIndex name is the first level, if it is, reset to the duplicate scene that excludes to intro cutscene
        // if(levelSceneNames[currentLevelIndex] == levelSceneNames[0])
        // {
        //     yield return StartCoroutine(SwapLevelRoutine(levelSceneNames[1]));
        //     // change 1 for the actual duplicated scene index
        // }

        yield return StartCoroutine(SwapLevelRoutine(levelSceneNames[currentLevelIndex]));

        ResetPersistentPlayerState();
        ResetPlayerPosToSpawn();

        levelStartHeadPlacement = FindAnyObjectByType<LevelStartHeadPlacement>();
        levelStartHeadPlacement.LookForHeadPlacement();

        AudioManager.instance.RestoreMusicInstant();
        AudioManager.instance.ApplySceneMusic(currentLevelSceneName);
    }

    private IEnumerator DebounceHeadReturnToPlayer()
    {
        // We debounce and do almost a second delay below because returning the playerhead to its original pos
        // causes it to snap back into the player for a short moment before the level restart happens.
        // So we wait for the screen to go black, then snap it back to the player when they cant see the snap happen.
        // just before the level restart happens.

        yield return new WaitForSeconds(1.5f);
        if(playerStateController != null)
        {
            playerStateController.ForceHeadBackToPlayer();
        }
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
