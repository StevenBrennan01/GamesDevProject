using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent (typeof(UIDocument))]
public class ScreenFadeManager : MonoBehaviour
{
    public static ScreenFadeManager instance;
    private LoadingScreen loadingScreen;

    [Header("Fade UXML")]
    [SerializeField] private VisualTreeAsset fadeOverlay;
    private string fadeRootName = "FadeRoot";
    private float fadeAlpha = 0f;
    private UIDocument doc;
    private VisualElement root;
    private VisualElement fadeRoot;

    [SerializeField] private float fadeInOutSeconds = 1f;
    [SerializeField] private float holdBlackSeconds = 3f;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        loadingScreen = FindAnyObjectByType<LoadingScreen>();

        instance = this;
        DontDestroyOnLoad(gameObject);

        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement;

        BuildFadeUI();
        SetAlpha(0f);
        HideFadeUI();
    }

    private void BuildFadeUI()
    {
        root.Clear();

        if (fadeOverlay == null)
        {
            Debug.LogError("ScreenFader: fadeScreen VisualTreeAsset not assigned.", this);
            return;
        }

        fadeOverlay.CloneTree(root);
        fadeRoot = root.Q<VisualElement>(fadeRootName);
    }

    public Coroutine TransitionToScene(string sceneName, float fadeOutSeconds, float holdBlackSeconds, float fadeInSeconds)
    {
        return StartCoroutine(LoadPersistentScene(sceneName, fadeOutSeconds, holdBlackSeconds, fadeInSeconds));
    }

    private IEnumerator LoadPersistentScene(string sceneName, float fadeOutSeconds, float holdBlackSeconds, float fadeInSeconds)
    {
        ShowFadeUI();
        yield return FadingActive(1f, fadeOutSeconds);

        if(sceneName != "MainMenu") // Returning to menu is fast, no camera setup behind the scenes so no need for loading screen
        {
            loadingScreen?.DisplayLoadingIcon();
        }
        
        SceneManager.LoadScene(sceneName); // load scene while black
        
        // optional hold
        if (holdBlackSeconds > 0f)
        {
            yield return new WaitForSecondsRealtime(holdBlackSeconds);
        }

        AudioManager.instance?.RestoreMusicInstant();

        yield return FadingActive(0f, fadeInSeconds);
        HideFadeUI();
    }

    public void TransitionToNextScene()
    {

        StartCoroutine(FadeToBlack(fadeInOutSeconds));
    }

    private IEnumerator FadeToBlack(float seconds)
    {
        ShowFadeUI();
        yield return FadingActive(1f, seconds);

        StartCoroutine(HoldBlack(holdBlackSeconds));
    }

    private IEnumerator HoldBlack(float seconds)
    {
        ShowFadeUI();
        loadingScreen?.DisplayLoadingIcon();
        yield return new WaitForSecondsRealtime(seconds);

        StartCoroutine(FadeToClear(fadeInOutSeconds));
    }

    private IEnumerator FadeToClear(float seconds)
    {
        yield return FadingActive(0f, seconds);
        HideFadeUI();
    }

    public IEnumerator FadingActive(float targetAlpha, float seconds)
    {
        if (fadeRoot == null) yield break;

        targetAlpha = Mathf.Clamp01(targetAlpha);

        float start = fadeAlpha;
        float t = 0f;

        // Use realtime so it works even if timeScale changes
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = seconds <= 0f ? 1f : Mathf.Clamp01(t / seconds);
            SetAlpha(Mathf.Lerp(start, targetAlpha, a));
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float a)
    {
        fadeAlpha = Mathf.Clamp01(a);
        fadeRoot.style.opacity = fadeAlpha;
    }

    private void ShowFadeUI()
    {
        root.style.display = DisplayStyle.Flex;
        root.pickingMode = PickingMode.Position; // block clicks
    }

    private void HideFadeUI()
    {
        root.style.display = DisplayStyle.None;
        root.pickingMode = PickingMode.Ignore; // allows for clicks
    }
}