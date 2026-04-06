using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private PlayerInputs inputs;
    private string menuSceneName = "MainMenu";

    [Header("UXML Screens")]
    [SerializeField] private VisualTreeAsset pauseScreenUXML;
    [SerializeField] private VisualTreeAsset optionsScreenUXML;
    [SerializeField] private VisualTreeAsset interactionWarningUXML;

    [Header("Scene Transition Settings")]
    [SerializeField] private float fadeInSeconds = 0.5f;
    [SerializeField] private float fadeOutSeconds = 1f;
    [SerializeField] private float holdBlackSeconds = 0.5f;

    private UIDocument doc;
    private VisualElement root;

    //private bool isPaused = false;

    private void Awake()
    {
        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement;
        inputs = FindAnyObjectByType<PlayerInputs>();

        root.Clear();
    }

    private void OnEnable()
    {
        inputs.OnPaused += DisplayPauseMenu;
    }

    private void OnDisable()
    {
        inputs.OnPaused -= DisplayPauseMenu;
    }

    private void DisplayPauseMenu()
    {
        //if (isPaused)
        //{
        //    ResumeGame();
        //    return;
        //}

        inputs.SetInputLocked(true);

        LoadScreen(pauseScreenUXML);
        //isPaused = true;

        ShowCursorAndAllowClicks();

        var ResumeButton = root.Q<Button>("ResumeButton");
        ResumeButton.clicked += () =>
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayOneShotClick();
            }

            ResumeGame();
        };

        var OptionsButton = root.Q<Button>("OptionsButton");
        OptionsButton.clicked += () =>
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayOneShotClick();
            }

            DisplayOptions();
        };

        var QuitButton = root.Q<Button>("QuitButton");
        QuitButton.clicked += () =>
        {
            root.Clear(); // Clears UI for return to Menu

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayOneShotClick();
            }
            if (AudioManager.instance != null)
            {
                AudioManager.instance.FadeMusic(0f, fadeOutSeconds);
            }

            if (ScreenFadeManager.instance != null)
            {
                UnityEngine.Cursor.visible = true;
                UnityEngine.Cursor.lockState = CursorLockMode.None;

                ScreenFadeManager.instance.TransitionToScene(menuSceneName, fadeOutSeconds, holdBlackSeconds, fadeInSeconds);
            }
        };
    }

    private void DisplayOptions()
    {
        LoadScreen(optionsScreenUXML);

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
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.SetMasterVolume(evt.newValue);
                    AudioManager.instance.SaveVolumes();
                }
            });
        }
        else Debug.LogError("MasterSlider not found in Options UXML.");

        // Music slider
        if (musicSlider != null)
        {
            musicSlider.RegisterValueChangedCallback(evt =>
            {
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.SetMusicVolume(evt.newValue);
                    AudioManager.instance.SaveVolumes();
                }
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
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.SetSfxVolume(evt.newValue);
                    AudioManager.instance.SaveVolumes();
                }
                
                // Preview while dragging, throttled so it doesn't spam
                if (Time.unscaledTime >= nextPreviewTime)
                {
                    nextPreviewTime = Time.unscaledTime + previewCooldown;
                    if (AudioManager.instance != null)
                    {
                        AudioManager.instance.PlaySfxPreview();
                    }
                }
            });
        }
        else Debug.LogError("SFXSlider not found in Options UXML.");

        var backButton = root.Q<Button>("BackButton");
        backButton.clicked += () =>
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayOneShotClick();
            }

            DisplayPauseMenu();
        };
    }
    private void LoadScreen(VisualTreeAsset uxmlAsset)
    {
        root.Clear();

        if (uxmlAsset != null)
        {
            uxmlAsset.CloneTree(root);
        }
    }

    private void ResumeGame()
    {
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        root.pickingMode = PickingMode.Position;

        inputs.SetInputLocked(false);

        //isPaused = false;

        root.Clear(); //RESUMES GAME ETC
    }

    public void CantPlaceUI()
    {
        StartCoroutine(CantPlaceCoroutine());
    }

    private IEnumerator CantPlaceCoroutine()
    {
        interactionWarningUXML.CloneTree(root);
        yield return new WaitForSeconds(4);
        root.Clear();
    }

    private void ShowCursorAndAllowClicks()
    {
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        root.pickingMode = PickingMode.Ignore;
    }
}