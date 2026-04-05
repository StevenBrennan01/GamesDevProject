using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

[RequireComponent (typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    private string gameSceneName = "FYPLevel";

    [Header("UXML Screens")]
    [SerializeField] private VisualTreeAsset mainMenuScreen;
    [SerializeField] private VisualTreeAsset optionsScreen;
    [SerializeField] private VisualTreeAsset creditsScreen;

    [Header("Scene Transition Settings")]
    [SerializeField] private float fadeSeconds = 0.75f;
    [SerializeField] private float holdBlackSeconds;

    private UIDocument doc;
    private VisualElement root;

    private void Awake()
    {
        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement;

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

        var masterSlider = root.Q<Slider>("MasterSlider");
        var musicSlider = root.Q<Slider>("MusicSlider");
        var sfxSlider = root.Q<Slider>("SFXSlider");

        if (AudioManager.instance != null)
        {
            masterSlider?.SetValueWithoutNotify(AudioManager.instance.MasterVolume01);
            musicSlider?.SetValueWithoutNotify(AudioManager.instance.MusicVolume01);
            sfxSlider?.SetValueWithoutNotify(AudioManager.instance.SfxVolume01);
        }

        // Master slider
        if (masterSlider != null)
        {
            masterSlider.RegisterValueChangedCallback(evt =>
            {
                AudioManager.instance.SetMasterVolume(evt.newValue);
                AudioManager.instance.SaveVolumes();
            });
        }
        else Debug.LogError("MasterSlider not found in Options UXML.");

        // Music slider
        if (musicSlider != null)
        {
            musicSlider.RegisterValueChangedCallback(evt =>
            {
                AudioManager.instance.SetMusicVolume(evt.newValue);
                AudioManager.instance.SaveVolumes();
            });
        }
        else Debug.LogError("MusicSlider not found in Options UXML.");

        // SFX slider + throttled preview
        float nextPreviewTime = 0f;
        const float previewCooldown = 0.10f; // tweak: 0.08�0.15 feels good

        if (sfxSlider != null)
        {
            sfxSlider.RegisterValueChangedCallback(evt =>
            {
                AudioManager.instance.SetSfxVolume(evt.newValue);
                AudioManager.instance.SaveVolumes();

                // Preview while dragging, throttled so it doesn't spam
                if (Time.unscaledTime >= nextPreviewTime)
                {
                    nextPreviewTime = Time.unscaledTime + previewCooldown;
                    AudioManager.instance.PlaySfxPreview();
                }
            });
        }
        else Debug.LogError("SFXSlider not found in Options UXML.");

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
        StartCoroutine(StartGameTransition());
    }

    private IEnumerator StartGameTransition()
    {
        AudioManager.instance?.SaveVolumes();

        AudioManager.instance.FadeMusic(0f, fadeSeconds);

        if (ScreenFadeManager.instance != null)
        {
            yield return ScreenFadeManager.instance.TransitionToScene(gameSceneName, fadeSeconds, holdBlackSeconds, fadeSeconds);
        }
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