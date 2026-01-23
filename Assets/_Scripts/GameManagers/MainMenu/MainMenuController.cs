using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent (typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    private string gameSceneName = "FYPLevel";

    [SerializeField] private VisualTreeAsset mainMenuScreen;
    [SerializeField] private VisualTreeAsset optionsScreen;
    [SerializeField] private VisualTreeAsset creditsScreen;
    [SerializeField] private VisualTreeAsset fadeScreen;

    private Button startButton;
    private Button optionsButton;
    private Button creditsButton;
    private Button quitButton;
    private Button backButton;

    private UIDocument doc;
    private VisualElement root;

    private void Awake()
    {
        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement;

        if (fadeScreen == null)
        {
            Debug.Log("Fade not implemented yet");
        }

        DisplayMainMenu();
    }

    private void DisplayMainMenu()
    {
        LoadScreen(mainMenuScreen);

        var startButton = root.Q<Button>("StartGameButton");
        startButton.clicked += () =>
        {
            StartGame();
            AudioManager.instance.PlayOneShotClick();
        };

        var optionsButton = root.Q<Button>("OptionsButton");
        optionsButton.clicked += () =>
        {
            DisplayOptions();
            AudioManager.instance.PlayOneShotClick();
        };

        var creditsButton = root.Q<Button>("CreditsButton");
        creditsButton.clicked += () => 
        { 
            DisplayCredits(); 
            AudioManager.instance.PlayOneShotClick(); 
        };

        var quitButton = root.Q<Button>("QuitButton");
        quitButton.clicked += () => 
        { 
            QuitGame(); 
            AudioManager.instance.PlayOneShotClick();
        };
    }

    private void DisplayOptions()
    {
        LoadScreen(optionsScreen);

        var backButton = root.Q<Button>("BackButton");
        backButton.clicked += () =>
        {
            DisplayMainMenu();
            AudioManager.instance.PlayOneShotClick();
        };
    }

    private void DisplayCredits()
    {
        LoadScreen(creditsScreen);

        var backButton = root.Q<Button>("BackButton");
        backButton.clicked += () =>
        {
            DisplayMainMenu();
            AudioManager.instance.PlayOneShotClick();
        };
    }

    private void StartGame()
    {
        // after triggering coroutine for fading in or something, run below

        SceneManager.LoadScene(gameSceneName);
    }

    private void LoadScreen(VisualTreeAsset uxmlAsset)
    {
        root.Clear();

        if (uxmlAsset != null)
        {
            uxmlAsset.CloneTree(root);
        }
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}