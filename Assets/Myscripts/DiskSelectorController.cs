using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DiskSelectorController : MonoBehaviour
{
    public const string BinaryHaloOption = "Binary Halo";
    public const string GradedHaloOption = "Graded Halo";
    public const string LegacyGrandeHaloOption = "Grande Halo";
    public const string ProbabilityHaloOption = "Probability Halo";
    public const string RepeatAttemptDashboardOption = "Repeat Attempt Dashboard";
    public const string TimelineDashboardOption = "Timeline Dashboard";

    [SerializeField] private KeyCode toggleKey = KeyCode.J;
    [SerializeField] private string[] options =
    {
        BinaryHaloOption,
        GradedHaloOption,
        ProbabilityHaloOption,
        "Directional Peripheral Halo",
        RepeatAttemptDashboardOption,
        TimelineDashboardOption,
        "Arousal Dashboard"
    };

    [SerializeField] private int selectedIndex;
    [SerializeField] private float optionRadius = 210f;
    [SerializeField] private float optionSize = 112f;
    [SerializeField] private float centerSize = 128f;

    private Canvas canvas;
    private RectTransform root;
    private Button[] optionButtons;
    private TextMeshProUGUI currentLabel;
    private Sprite circleSprite;

    public static string ActiveSelection { get; private set; } = BinaryHaloOption;
    public static bool IsBinaryHaloSelected => ActiveSelection == BinaryHaloOption;
    public static bool IsGradedHaloSelected => ActiveSelection == GradedHaloOption || ActiveSelection == LegacyGrandeHaloOption;
    public static bool IsProbabilityHaloSelected => ActiveSelection == ProbabilityHaloOption;
    public static bool IsRepeatAttemptDashboardSelected => ActiveSelection == RepeatAttemptDashboardOption;
    public static bool IsTimelineDashboardSelected => ActiveSelection == TimelineDashboardOption;

    public string CurrentSelection
    {
        get
        {
            if (options == null || options.Length == 0)
            {
                return string.Empty;
            }

            return options[Mathf.Clamp(selectedIndex, 0, options.Length - 1)];
        }
    }

    private void Awake()
    {
        EnsureSevenOptions();
        selectedIndex = Mathf.Clamp(selectedIndex, 0, options.Length - 1);
        circleSprite = CreateCircleSprite(128);
        EnsureEventSystem();
        BuildUi();
        SetVisible(false);
        RefreshSelection();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            SetVisible(!canvas.gameObject.activeSelf);
        }

        if (canvas.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            SetVisible(false);
        }
    }

    private void EnsureSevenOptions()
    {
        if (options == null || options.Length != 7)
        {
            options = new[]
            {
                BinaryHaloOption,
                GradedHaloOption,
                ProbabilityHaloOption,
                "Directional Peripheral Halo",
                RepeatAttemptDashboardOption,
                TimelineDashboardOption,
                "Arousal Dashboard"
            };
        }

        if (string.IsNullOrWhiteSpace(options[0]))
        {
            options[0] = BinaryHaloOption;
        }
    }

    private void BuildUi()
    {
        GameObject canvasObject = new GameObject("Disk Selector Canvas");
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject rootObject = CreateRectObject("Disk Selector", canvasObject.transform);
        root = rootObject.GetComponent<RectTransform>();
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = new Vector2(620f, 620f);

        Image backdrop = rootObject.AddComponent<Image>();
        backdrop.sprite = circleSprite;
        backdrop.color = new Color(0.04f, 0.05f, 0.07f, 0.82f);

        optionButtons = new Button[options.Length];
        for (int i = 0; i < options.Length; i++)
        {
            int index = i;
            Button button = CreateRoundButton($"Disk Option {i + 1}", root, optionSize, options[i]);
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            float angle = Mathf.PI * 2f * i / options.Length;
            buttonRect.anchoredPosition = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * optionRadius;
            button.onClick.AddListener(() => SelectOption(index));
            optionButtons[i] = button;
        }

        Button nextButton = CreateRoundButton("Disk Selector Next", root, centerSize, "Next");
        nextButton.onClick.AddListener(EndRun);
        Image nextImage = nextButton.GetComponent<Image>();
        nextImage.color = new Color(0.12f, 0.16f, 0.2f, 0.96f);

        GameObject labelObject = CreateRectObject("Disk Selector Current", root);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = new Vector2(0f, -300f);
        labelRect.sizeDelta = new Vector2(420f, 44f);
        currentLabel = labelObject.AddComponent<TextMeshProUGUI>();
        currentLabel.alignment = TextAlignmentOptions.Center;
        currentLabel.fontSize = 24f;
        currentLabel.color = Color.white;
        currentLabel.raycastTarget = false;
    }

    private void SelectOption(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, options.Length - 1);
        RefreshSelection();
    }

    private void EndRun()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void RefreshSelection()
    {
        ActiveSelection = CurrentSelection;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            Image image = optionButtons[i].GetComponent<Image>();
            image.color = i == selectedIndex
                ? new Color(0.2f, 0.78f, 1f, 0.98f)
                : new Color(0.16f, 0.18f, 0.22f, 0.94f);
        }

        currentLabel.text = CurrentSelection;
    }

    private void SetVisible(bool visible)
    {
        canvas.gameObject.SetActive(visible);
    }

    private Button CreateRoundButton(string objectName, Transform parent, float size, string text)
    {
        GameObject buttonObject = CreateRectObject(objectName, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(size, size);

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = circleSprite;
        image.type = Image.Type.Simple;
        image.color = new Color(0.16f, 0.18f, 0.22f, 0.94f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        GameObject textObject = CreateRectObject("Label", buttonObject.transform);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.16f, 0.16f);
        textRect.anchorMax = new Vector2(0.84f, 0.84f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = size > 120f ? 25f : 17f;
        label.enableWordWrapping = true;
        label.color = Color.white;
        label.raycastTarget = false;
        return button;
    }

    private static GameObject CreateRectObject(string objectName, Transform parent)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static Sprite CreateCircleSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.5f - 1f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(radius - distance + 1f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
