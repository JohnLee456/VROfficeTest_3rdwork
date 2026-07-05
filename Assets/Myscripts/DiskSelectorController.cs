using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DiskSelectorController : MonoBehaviour
{
    public const string BinaryHaloOption = "Binary Halo";
    public const string GradedHaloOption = "Graded Halo";
    public const string LegacyGrandeHaloOption = "Grande Halo";
    public const string ProbabilityHaloOption = "Probability Halo";
    public const string DirectionalPeripheralHaloOption = "Directional Peripheral Halo";
    public const string RepeatAttemptDashboardOption = "Repeat Attempt Dashboard";
    public const string TimelineDashboardOption = "Timeline Dashboard";
    public const string ArousalDashboardOption = "Arousal Dashboard";

    [SerializeField] private KeyCode toggleKey = KeyCode.J;
    [SerializeField] private KeyCode previousOptionKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode nextOptionKey = KeyCode.RightArrow;
    [SerializeField] private KeyCode closeKey = KeyCode.Return;
    [SerializeField] private string[] options =
    {
        BinaryHaloOption,
        GradedHaloOption,
        ProbabilityHaloOption,
        DirectionalPeripheralHaloOption,
        RepeatAttemptDashboardOption,
        TimelineDashboardOption,
        ArousalDashboardOption
    };

    [SerializeField] private int selectedIndex;
    [SerializeField] private float diskSize = 450f;
    [SerializeField] private float optionRadius = 154f;
    [SerializeField] private float optionSize = 82f;
    [SerializeField] private float centerSize = 94f;
    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, -0.02f, 1.12f);
    [SerializeField] private float worldScale = 0.0018f;

    private Camera cachedCamera;
    private Canvas canvas;
    private RectTransform root;
    private Button[] optionButtons;
    private TextMeshProUGUI currentLabel;
    private Sprite circleSprite;

    public static string ActiveSelection { get; private set; } = BinaryHaloOption;
    public static bool IsBinaryHaloSelected => ActiveSelection == BinaryHaloOption;
    public static bool IsGradedHaloSelected => ActiveSelection == GradedHaloOption || ActiveSelection == LegacyGrandeHaloOption;
    public static bool IsProbabilityHaloSelected => ActiveSelection == ProbabilityHaloOption;
    public static bool IsDirectionalPeripheralHaloSelected => ActiveSelection == DirectionalPeripheralHaloOption;
    public static bool IsRepeatAttemptDashboardSelected => ActiveSelection == RepeatAttemptDashboardOption;
    public static bool IsTimelineDashboardSelected => ActiveSelection == TimelineDashboardOption;
    public static bool IsArousalDashboardSelected => ActiveSelection == ArousalDashboardOption;

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
        OfficeXrUiSupport.EnsureEventSystem();
        BuildUi();
        SetVisible(false);
        RefreshSelection();
    }

    private void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!OfficeSceneSupport.ShouldShowRuntimeUi(sceneName))
        {
            SetVisible(false);
            return;
        }

        bool rightHandTogglePressed = sceneName == OfficeSceneSupport.OfficeLoggedIn &&
            OfficeVrControllerInput.GetADown();
        if (Input.GetKeyDown(toggleKey) || rightHandTogglePressed)
        {
            SetVisible(!canvas.gameObject.activeSelf);
        }

        if (!canvas.gameObject.activeSelf)
        {
            return;
        }

        EnsureCameraAttachment();
        HandleKeyboardSelection();

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(closeKey) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SetVisible(false);
        }
    }

    private void HandleKeyboardSelection()
    {
        for (int i = 0; i < options.Length; i++)
        {
            KeyCode alphaKey = (KeyCode)((int)KeyCode.Alpha1 + i);
            KeyCode keypadKey = (KeyCode)((int)KeyCode.Keypad1 + i);
            if (Input.GetKeyDown(alphaKey) || Input.GetKeyDown(keypadKey))
            {
                SelectOption(i);
                return;
            }
        }

        if (Input.GetKeyDown(nextOptionKey) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            SelectOption((selectedIndex + 1) % options.Length);
            return;
        }

        if (Input.GetKeyDown(previousOptionKey) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            SelectOption((selectedIndex - 1 + options.Length) % options.Length);
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
                DirectionalPeripheralHaloOption,
                RepeatAttemptDashboardOption,
                TimelineDashboardOption,
                ArousalDashboardOption
            };
        }

        if (string.IsNullOrWhiteSpace(options[0]))
        {
            options[0] = BinaryHaloOption;
        }
    }

    private void BuildUi()
    {
        cachedCamera = Camera.main;
        GameObject canvasObject = new GameObject("Disk Selector Canvas", typeof(RectTransform));
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 500;
        DashboardOverlayRendering.ConfigureCanvas(canvas, 500);
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 12f;
        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(diskSize + 60f, diskSize + 96f);

        GameObject rootObject = CreateRectObject("Disk Selector", canvasObject.transform);
        root = rootObject.GetComponent<RectTransform>();
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = new Vector2(diskSize, diskSize);

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
        labelRect.anchoredPosition = new Vector2(0f, -218f);
        labelRect.sizeDelta = new Vector2(320f, 36f);
        currentLabel = labelObject.AddComponent<TextMeshProUGUI>();
        currentLabel.alignment = TextAlignmentOptions.Center;
        currentLabel.fontSize = 18f;
        currentLabel.raycastTarget = false;
        ApplyReadableWhiteText(currentLabel);

        EnsureCameraAttachment();
        OfficeXrUiSupport.ConfigureCanvasForXr(canvas, true);
        DashboardOverlayRendering.ApplyToRoot(canvasObject);
    }

    private void SelectOption(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, options.Length - 1);
        RefreshSelection();
    }

    private void EndRun()
    {
        ReturnToLoginAfterLeavingRoom.StartReturn();
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
        if (visible)
        {
            EnsureCameraAttachment();
        }
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
        label.fontSize = text == "Next" ? 19f : 13f;
        label.enableWordWrapping = true;
        label.raycastTarget = false;
        ApplyReadableWhiteText(label);
        return button;
    }

    private static void ApplyReadableWhiteText(TextMeshProUGUI label)
    {
        if (label == null)
        {
            return;
        }

        label.color = Color.white;
        label.faceColor = Color.white;
        label.outlineColor = new Color(0f, 0f, 0f, 0.72f);
        label.outlineWidth = 0.08f;
        label.enableVertexGradient = false;
        label.overrideColorTags = true;

        if (label.fontSharedMaterial != null)
        {
            label.fontMaterial = new Material(label.fontSharedMaterial);
            label.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
            label.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0f, 0f, 0f, 0.72f));
            label.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.08f);
        }

        DashboardOverlayRendering.ApplyToText(label);
    }

    private static GameObject CreateRectObject(string objectName, Transform parent)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private void EnsureCameraAttachment()
    {
        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        if (canvas == null || cachedCamera == null)
        {
            return;
        }

        Transform canvasTransform = canvas.transform;
        if (canvasTransform.parent != cachedCamera.transform)
        {
            canvasTransform.SetParent(cachedCamera.transform, false);
        }

        canvasTransform.localPosition = cameraLocalPosition;
        canvasTransform.localRotation = Quaternion.identity;
        canvasTransform.localScale = Vector3.one * worldScale;
        canvas.worldCamera = cachedCamera;
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
