using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent (typeof(UIDocument))]
public class ScreenFadeManager : MonoBehaviour
{
    public static ScreenFadeManager instance;

    [Header("Fade UXML")]
    [SerializeField] private VisualTreeAsset fadeOverlay;

    private string fadeRootName = "FadeRoot";

    private float fadeAlpha = 0f;

    private UIDocument doc;
    private VisualElement root;
    private VisualElement fadeRoot;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement;

        BuildFadeUI();
        SetAlpha(0f);
        Hide();
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

    public Coroutine TransitionToScene(string sceneName, float fadeOutSeconds, float holdBlackSeconds, float fadeInSeconds = -1f)
    {
        return StartCoroutine(TransitionRoutine(sceneName, fadeOutSeconds, holdBlackSeconds, fadeInSeconds));
    }

    private IEnumerator TransitionRoutine(string sceneName, float fadeOutSeconds, float holdBlackSeconds, float fadeInSeconds)
    {
        Show();
        yield return FadingActive(1f, fadeOutSeconds);

        // load scene while black
        SceneManager.LoadScene(sceneName);
        

        // optional hold
        if (holdBlackSeconds > 0f)
            yield return new WaitForSecondsRealtime(holdBlackSeconds);

        AudioManager.instance?.RestoreMusicInstant();

        yield return FadingActive(0f, fadeInSeconds);
        Hide();
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

    private void Show()
    {
        root.style.display = DisplayStyle.Flex;
        root.pickingMode = PickingMode.Position; // block clicks
    }

    private void Hide()
    {
        root.style.display = DisplayStyle.None;
        root.pickingMode = PickingMode.Ignore; // allows for clicks
    }
}