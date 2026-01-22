using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent (typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    private string gameSceneName = "FYPLevel";

    private VisualElement mainScreen;
    //private VisualElement optionsScreen;
    //private VisualElement creditsScreen;

    private Button startButton;
    private Button optionsButton;
    private Button creditsButton;
    private Button quitButton;
    //private Button backButton;

    private const string HiddenClass = "hidden";

    private void Awake()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        mainScreen = root.Q<VisualElement>("MainMenuScreen");
        //optionsScreen = root.Q<VisualElement>("OptionsScreen");

        startButton = root.Q<Button>("StartGameButton");
        optionsButton = root.Q<Button>("OptionsButton");
        creditsButton = root.Q<Button>("CreditsButton");
        quitButton = root.Q<Button>("QuitButton");
        //backButton = root.Q<Button>("BackButton");

        startButton.clicked += StartGame;
        //optionsButton.clicked += DisplayOptions;
        //creditsButton.clicked += DisplayCredits;
        quitButton.clicked += QuitGame;
        //backButton.clicked += DisplayMain;

        //DisplayMain();
    }

    private void Update()
    {
        //if (ButtonBeingHovered())
        //{
        //    Debug.Log("Button being hovered");
        //    OnHover?.Invoke();
        //}
    }

    //private bool ButtonBeingHovered()
    //{
    //    if (startButton.hasHoverPseudoState || optionsButton.hasHoverPseudoState
    //        || creditsButton.hasHoverPseudoState || quitButton.hasHoverPseudoState)
    //    {
    //        return true;
    //    }

    //    return false;
    //}


    private void StartGame()
    {
        // after triggering coroutine for fading in or something, run below
        AudioManager.instance.PlayOneShotClick();
        SceneManager.LoadScene(gameSceneName);
    }

    //private void DisplayMain()
    //{
    //    AudioManager.instance.PlayOneShotClick();
    //    mainScreen.RemoveFromClassList(HiddenClass);
    //    creditsScreen.AddToClassList(HiddenClass);
    //    optionsScreen.AddToClassList(HiddenClass);
    //}

    //private void DisplayOptions()
    //{
    //    AudioManager.instance.PlayOneShotClick();
    //    mainScreen.AddToClassList(HiddenClass);
    //    creditsScreen.AddToClassList(HiddenClass);
    //    optionsScreen.RemoveFromClassList(HiddenClass);
    //}

    //private void DisplayCredits()
    //{
    //    AudioManager.instance.PlayOneShotClick();
    //    mainScreen.AddToClassList(HiddenClass);
    //    creditsScreen.RemoveFromClassList(HiddenClass);
    //    optionsScreen.AddToClassList(HiddenClass);
    //}

    private void QuitGame()
    {
        //    AudioManager.instance.PlayOneShotClick();
        Application.Quit();
    }
}
