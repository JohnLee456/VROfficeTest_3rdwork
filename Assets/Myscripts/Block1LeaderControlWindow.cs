using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-80)]
public class Block1LeaderControlWindow : MonoBehaviour
{
    private const string ControlledSceneName = OfficeSceneSupport.OfficeLoggedInNoBot;
    private const string ControlledAvatarName = "GCHbot";
    private const int LastTrialNumber = 2;

    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, -0.08f, 1.05f);
    [SerializeField] private Vector2 panelSize = new Vector2(620f, 330f);
    [SerializeField] private float worldScale = 0.0017f;
    [SerializeField] private float warningSecondsBeforeEpisodeEnd = 5f;
    [SerializeField] private float stayExtensionSeconds = 20f;
    [SerializeField] private KeyCode reopenTrialEndKey = KeyCode.T;

    private Block1SpeakingIntentionController controller;
    private Camera cachedCamera;
    private GameObject windowRoot;
    private RectTransform panelRect;
    private TextMeshProUGUI headerText;
    private TextMeshProUGUI bodyText;
    private TextMeshProUGUI timerText;
    private Button primaryButton;
    private Button secondaryButton;
    private TextMeshProUGUI primaryButtonText;
    private TextMeshProUGUI secondaryButtonText;

    private int pendingTrialNumber = 1;
    private int pendingEpisodeNumber = 1;
    private bool warningShownForCurrentEpisode;
    private bool stayExtensionActive;
    private bool trialEndHidden;
    private float stayExtensionRemaining;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForScene()
    {
        TryCreateForCurrentScene();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateBootstrap()
    {
        if (FindObjectOfType<Block1LeaderControlWindowBootstrap>() != null)
        {
            return;
        }

        GameObject bootstrap = new GameObject("Block1 Leader Control Window Bootstrap");
        DontDestroyOnLoad(bootstrap);
        bootstrap.AddComponent<Block1LeaderControlWindowBootstrap>();
    }

    public static void TryCreateForCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!ShouldCreateForScene(activeScene) || FindObjectOfType<Block1LeaderControlWindow>() != null)
        {
            return;
        }

        GameObject manager = new GameObject("Block1 Leader Control Window");
        manager.AddComponent<Block1LeaderControlWindow>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (!CanShowForCurrentUser())
        {
            enabled = false;
            return;
        }

        EnsureController();
        EnsureEventSystem();
        BuildWindow();
        controller.StartTrial(1);
        ShowEpisodeStart(1, 1);
    }

    private void Update()
    {
        if (!CanShowForCurrentUser())
        {
            if (windowRoot != null)
            {
                windowRoot.SetActive(false);
            }

            return;
        }

        EnsureCameraAttachment();

        if (trialEndHidden && Input.GetKeyDown(reopenTrialEndKey))
        {
            trialEndHidden = false;
            ShowTrialEnd();
        }

        if (stayExtensionActive)
        {
            stayExtensionRemaining -= Time.deltaTime;
            timerText.text = "Stay extension: " + Mathf.CeilToInt(Mathf.Max(0f, stayExtensionRemaining)) + "s";

            if (stayExtensionRemaining <= 0f)
            {
                stayExtensionActive = false;
                GoToNextEpisodeOrTrialEnd();
            }

            return;
        }

        if (controller == null || !controller.IsEpisodeRunning || warningShownForCurrentEpisode)
        {
            return;
        }

        if (controller.EpisodeRemainingSeconds <= warningSecondsBeforeEpisodeEnd)
        {
            ShowEpisodeEndWarning();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!ShouldCreateForScene(scene))
        {
            DestroyWindow();
            return;
        }

        EnsureController();
        if (windowRoot == null)
        {
            BuildWindow();
        }
    }

    private void ShowEpisodeStart(int trialNumber, int episodeNumber)
    {
        pendingTrialNumber = trialNumber;
        pendingEpisodeNumber = episodeNumber;
        warningShownForCurrentEpisode = false;
        stayExtensionActive = false;
        trialEndHidden = false;
        string title = controller.GetEpisodeTitle(trialNumber, episodeNumber);
        headerText.text = string.IsNullOrEmpty(title) ? "Episode " + episodeNumber : title;
        bodyText.text = "Trial " + trialNumber + " is ready. Start this episode when the discussion reaches this segment.";
        timerText.text = string.Empty;
        ConfigureButton(primaryButton, primaryButtonText, "Start", OnStartEpisodeClicked, true);
        ConfigureButton(secondaryButton, secondaryButtonText, string.Empty, null, false);
        SetWindowVisible(true);
    }

    private void ShowEpisodeEndWarning()
    {
        warningShownForCurrentEpisode = true;
        controller.Pause();

        headerText.text = "Episode " + controller.CurrentEpisodeNumber + " ending";
        bodyText.text = "This episode is 5 seconds from the scheduled end. Go to the next episode or stay for 20 seconds.";
        timerText.text = "Remaining: " + Mathf.CeilToInt(controller.EpisodeRemainingSeconds) + "s";
        ConfigureButton(primaryButton, primaryButtonText, "Stay", OnStayClicked, true);
        ConfigureButton(secondaryButton, secondaryButtonText, "Next Episode", OnNextEpisodeClicked, true);
        SetWindowVisible(true);
    }

    private void ShowStayExtension()
    {
        stayExtensionActive = true;
        stayExtensionRemaining = stayExtensionSeconds;
        controller.Pause();

        headerText.text = "Stay in current episode";
        bodyText.text = "Speaking intention values are frozen. The next episode panel will open automatically.";
        timerText.text = "Stay extension: " + Mathf.CeilToInt(stayExtensionRemaining) + "s";
        ConfigureButton(primaryButton, primaryButtonText, string.Empty, null, false);
        ConfigureButton(secondaryButton, secondaryButtonText, string.Empty, null, false);
        SetWindowVisible(true);
    }

    private void ShowTrialEnd()
    {
        stayExtensionActive = false;

        headerText.text = "Trial " + controller.CurrentTrialNumber + " complete";
        bodyText.text = "You can review the dashboard now. Please remove the VR headset and complete the questionnaire.";
        timerText.text = "Press T to reopen this window after hiding it.";
        ConfigureButton(primaryButton, primaryButtonText, "Hide", OnHideTrialEndClicked, true);
        ConfigureButton(secondaryButton, secondaryButtonText, "Next Trial", OnNextTrialClicked, controller.CurrentTrialNumber < LastTrialNumber);
        SetWindowVisible(true);
    }

    private void OnStartEpisodeClicked()
    {
        if (controller.CurrentTrialNumber != pendingTrialNumber)
        {
            controller.StartTrial(pendingTrialNumber);
        }

        controller.StartEpisode(pendingEpisodeNumber);
        Block1EpisodeSync.BroadcastEpisodeStarted(pendingTrialNumber, pendingEpisodeNumber);
        warningShownForCurrentEpisode = false;
        stayExtensionActive = false;
        SetWindowVisible(false);
    }

    private void OnStayClicked()
    {
        ShowStayExtension();
    }

    private void OnNextEpisodeClicked()
    {
        GoToNextEpisodeOrTrialEnd();
    }

    private void OnHideTrialEndClicked()
    {
        trialEndHidden = true;
        SetWindowVisible(false);
    }

    private void OnNextTrialClicked()
    {
        int nextTrial = controller.CurrentTrialNumber + 1;
        if (nextTrial > LastTrialNumber)
        {
            return;
        }

        controller.StartTrial(nextTrial);
        ShowEpisodeStart(nextTrial, 1);
    }

    private void GoToNextEpisodeOrTrialEnd()
    {
        int currentTrial = controller.CurrentTrialNumber;
        int currentEpisode = controller.CurrentEpisodeNumber;
        controller.EndEpisode();

        int nextEpisode = currentEpisode + 1;
        if (nextEpisode <= controller.EpisodeCount)
        {
            ShowEpisodeStart(currentTrial, nextEpisode);
        }
        else
        {
            ShowTrialEnd();
        }
    }

    private void EnsureController()
    {
        if (controller != null)
        {
            return;
        }

        controller = FindObjectOfType<Block1SpeakingIntentionController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<Block1SpeakingIntentionController>();
        }
    }

    private void BuildWindow()
    {
        if (windowRoot != null)
        {
            return;
        }

        cachedCamera = Camera.main;
        windowRoot = new GameObject("Block1 Leader Control Canvas", typeof(RectTransform));

        Canvas canvas = windowRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 900;
        DashboardOverlayRendering.ConfigureCanvas(canvas, 900);

        windowRoot.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 12f;
        windowRoot.AddComponent<GraphicRaycaster>();

        RectTransform rootRect = windowRoot.GetComponent<RectTransform>();
        rootRect.sizeDelta = panelSize;

        GameObject panelObject = CreateRect("Panel", windowRoot.transform);
        panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;

        Image panel = panelObject.AddComponent<Image>();
        panel.color = new Color(0.05f, 0.06f, 0.075f, 0.96f);
        DashboardOverlayRendering.ApplyToGraphic(panel);

        Image topRule = CreateImage("Top Rule", panelRect, new Vector2(0f, 118f), new Vector2(520f, 3f), new Color(0.42f, 0.62f, 0.82f, 0.9f));
        topRule.raycastTarget = false;

        headerText = CreateText("Header", panelRect, string.Empty, new Vector2(0f, 82f), new Vector2(520f, 62f), 28f, FontStyles.Bold, TextAlignmentOptions.Center);
        bodyText = CreateText("Body", panelRect, string.Empty, new Vector2(0f, 20f), new Vector2(510f, 74f), 19f, FontStyles.Normal, TextAlignmentOptions.Center);
        bodyText.enableWordWrapping = true;
        timerText = CreateText("Timer", panelRect, string.Empty, new Vector2(0f, -42f), new Vector2(480f, 32f), 18f, FontStyles.Bold, TextAlignmentOptions.Center);

        primaryButton = CreateButton("Primary Button", panelRect, new Vector2(-118f, -104f), new Vector2(190f, 54f), "Start", out primaryButtonText);
        secondaryButton = CreateButton("Secondary Button", panelRect, new Vector2(118f, -104f), new Vector2(190f, 54f), "Next", out secondaryButtonText);

        EnsureCameraAttachment();
        DashboardOverlayRendering.ApplyToRoot(windowRoot);
        SetWindowVisible(false);
    }

    private void EnsureCameraAttachment()
    {
        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        if (windowRoot == null || cachedCamera == null)
        {
            return;
        }

        if (windowRoot.transform.parent != cachedCamera.transform)
        {
            windowRoot.transform.SetParent(cachedCamera.transform, false);
        }

        windowRoot.transform.localPosition = cameraLocalPosition;
        windowRoot.transform.localRotation = Quaternion.identity;
        windowRoot.transform.localScale = Vector3.one * worldScale;

        Canvas canvas = windowRoot.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = cachedCamera;
        }
    }

    private void SetWindowVisible(bool visible)
    {
        if (windowRoot != null && windowRoot.activeSelf != visible)
        {
            windowRoot.SetActive(visible);
        }

        if (!visible)
        {
        }
    }

    private void DestroyWindow()
    {
        if (windowRoot != null)
        {
            Destroy(windowRoot);
            windowRoot = null;
        }

        enabled = false;
    }

    private static bool ShouldCreateForScene(Scene scene)
    {
        return scene.IsValid() && scene.name == ControlledSceneName && CanShowForCurrentUser();
    }

    private static bool CanShowForCurrentUser()
    {
        return LoginSession.HasRoute &&
            LoginSession.Role == LoginUserRole.Leader &&
            LoginSession.AvatarName == ControlledAvatarName &&
            SceneManager.GetActiveScene().name == ControlledSceneName;
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private static void ConfigureButton(Button button, TextMeshProUGUI label, string text, UnityEngine.Events.UnityAction callback, bool visible)
    {
        if (button == null)
        {
            return;
        }

        button.gameObject.SetActive(visible);
        button.onClick.RemoveAllListeners();
        if (callback != null)
        {
            button.onClick.AddListener(callback);
        }

        if (label != null)
        {
            label.text = text;
            ApplyReadableWhiteText(label);
        }
    }

    private static Button CreateButton(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, string text, out TextMeshProUGUI label)
    {
        Image image = CreateImage(objectName, parent, anchoredPosition, size, new Color(0.16f, 0.28f, 0.42f, 0.96f));
        image.raycastTarget = true;

        Button button = image.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.16f, 0.28f, 0.42f, 0.96f);
        colors.highlightedColor = new Color(0.22f, 0.38f, 0.56f, 1f);
        colors.pressedColor = new Color(0.1f, 0.2f, 0.32f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.5f);
        button.colors = colors;

        label = CreateText(objectName + " Label", image.transform, text, Vector2.zero, size, 21f, FontStyles.Bold, TextAlignmentOptions.Center);
        label.raycastTarget = false;
        return button;
    }

    private static Image CreateImage(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject imageObject = CreateRect(objectName, parent);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        DashboardOverlayRendering.ApplyToGraphic(image);
        return image;
    }

    private static TextMeshProUGUI CreateText(string objectName, Transform parent, string text, Vector2 anchoredPosition, Vector2 size, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
    {
        GameObject textObject = CreateRect(objectName, parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = alignment;
        label.enableWordWrapping = false;
        label.raycastTarget = false;
        ApplyReadableWhiteText(label);
        DashboardOverlayRendering.ApplyToText(label);
        return label;
    }

    private static void ApplyReadableWhiteText(TextMeshProUGUI label)
    {
        if (label == null)
        {
            return;
        }

        label.color = Color.white;
        label.faceColor = Color.white;
        label.outlineColor = new Color(0f, 0f, 0f, 0.7f);
        label.outlineWidth = 0.08f;
        label.enableVertexGradient = false;
        label.overrideColorTags = true;

        if (label.fontSharedMaterial != null)
        {
            label.fontMaterial = new Material(label.fontSharedMaterial);
            label.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
            label.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0f, 0f, 0f, 0.7f));
            label.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.08f);
        }

        DashboardOverlayRendering.ApplyToText(label);
    }

    private static GameObject CreateRect(string objectName, Transform parent)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }
}

public class Block1LeaderControlWindowBootstrap : MonoBehaviour
{
    private float nextCheckTime;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Block1LeaderControlWindow.TryCreateForCurrentScene();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextCheckTime)
        {
            return;
        }

        nextCheckTime = Time.unscaledTime + 0.5f;
        Block1LeaderControlWindow.TryCreateForCurrentScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Block1LeaderControlWindow.TryCreateForCurrentScene();
    }
}
