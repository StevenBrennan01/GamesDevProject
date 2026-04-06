using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent (typeof(UIDocument))]
public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen instance;

    [Header("Loading Screen UXML")]
    [SerializeField] private VisualTreeAsset loadingScreenUXML;
    private string loadingIconRootName = "LoadingIcon";
    private UIDocument doc;
    private VisualElement root;

    [Header("Loading Icon Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float activeSeconds = 2f;

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

        BuildLoadingIcon();
        HideIcon();
    }

    public void DisplayLoadingIcon()
    {
        root.style.display = DisplayStyle.Flex;
        root.pickingMode = PickingMode.Position;

        var loadingIcon = root.Q<VisualElement>(loadingIconRootName);

        StartCoroutine(RotateIcon(loadingIcon));
    }

    private void BuildLoadingIcon()
    {
        if (loadingScreenUXML == null)
        {
            Debug.LogError("LoadingScreen: loadingScreenUXML VisualTreeAsset not assigned.", this);
            return;
        }
        
        root.Clear();
        loadingScreenUXML.CloneTree(root);
    }

    private IEnumerator RotateIcon(VisualElement icon)
    {
        float timer = 0f;
        float currentRotation = 0f;
        while (timer < activeSeconds)
        {
            currentRotation += rotationSpeed * Time.deltaTime;
            icon.style.rotate = new Rotate(new Angle(currentRotation, AngleUnit.Degree));
            timer += Time.deltaTime;
            yield return null;
        }

        HideIcon();
    }

    private void HideIcon()
    {
        root.style.display = DisplayStyle.None;
    }
}
