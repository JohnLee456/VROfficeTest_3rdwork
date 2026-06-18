using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-70)]
public class Block1MemberEpisodeTimerPanel : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private const string ControlledSceneName = OfficeSceneSupport.OfficeLoggedInNoBot;

    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, -0.18f, 0.95f);
    [SerializeField] private Vector2 panelSize = new Vector2(420f, 170f);
    [SerializeField] private float worldScale = 0.0018f;
    [SerializeField] private KeyCode toggleKey = KeyCode.V;
#if UNITY_EDITOR
    [SerializeField] private KeyCode editorDebugStartKey = KeyCode.B;
#endif

    private Camera cachedCamera;
    private GameObject panelRoot;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI statusText;
    private TextMeshProUGUI timerText;

    private int currentTrialNumber;
    private int currentEpisodeNumber;
    private double episodeStartTime;
    private bool hasEpisodeStart;
    private bool panelVisible = true;
#if UNITY_EDITOR
    private int editorDebugTrialNumber = 1;
    private int editorDebugEpisodeNumber = 1;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForScene()
    {
        TryCreateForCurrentScene();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateBootstrap()
    {
        if (FindObjectOfType<Block1MemberEpisodeTimerPanelBootstrap>() != null)
        {
            return;
        }

        GameObject bootstrap = new GameObject("Block1 Member Episode Timer Bootstrap");
        DontDestroyOnLoad(bootstrap);
        bootstrap.AddComponent<Block1MemberEpisodeTimerPanelBootstrap>();
    }

    public static void TryCreateForCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!ShouldCreateForScene(activeScene) || FindObjectOfType<Block1MemberEpisodeTimerPanel>() != null)
        {
            return;
        }

        GameObject manager = new GameObject("Block1 Member Episode Timer Panel");
        manager.AddComponent<Block1MemberEpisodeTimerPanel>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Start()
    {
        if (!CanShowForCurrentUser())
        {
            enabled = false;
            return;
        }

        BuildPanel();
        TryApplyRoomState();
        RefreshText();
    }

    private void Update()
    {
        if (!CanShowForCurrentUser())
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            return;
        }

        EnsureCameraAttachment();

        if (Input.GetKeyDown(toggleKey))
        {
            panelVisible = !panelVisible;
            if (panelRoot != null)
            {
                panelRoot.SetActive(panelVisible);
            }
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(editorDebugStartKey))
        {
            SimulateEpisodeStartInEditor();
        }
#endif

        RefreshText();
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != Block1EpisodeSync.EpisodeStartedEventCode)
        {
            return;
        }

        int trialNumber;
        int episodeNumber;
        double startTime;
        if (Block1EpisodeSync.TryParsePayload(photonEvent.CustomData, out trialNumber, out episodeNumber, out startTime))
        {
            ApplyEpisodeStart(trialNumber, episodeNumber, startTime);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(Block1EpisodeSync.TrialKey) ||
            propertiesThatChanged.ContainsKey(Block1EpisodeSync.EpisodeKey) ||
            propertiesThatChanged.ContainsKey(Block1EpisodeSync.EpisodeStartTimeKey))
        {
            TryApplyRoomState();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!ShouldCreateForScene(scene))
        {
            DestroyPanel();
            return;
        }

        if (panelRoot == null)
        {
            BuildPanel();
        }

        TryApplyRoomState();
    }

    private void TryApplyRoomState()
    {
        int trialNumber;
        int episodeNumber;
        double startTime;
        if (Block1EpisodeSync.TryReadRoomState(out trialNumber, out episodeNumber, out startTime))
        {
            ApplyEpisodeStart(trialNumber, episodeNumber, startTime);
        }
    }

    private void ApplyEpisodeStart(int trialNumber, int episodeNumber, double startTime)
    {
        currentTrialNumber = trialNumber;
        currentEpisodeNumber = episodeNumber;
        episodeStartTime = startTime;
        hasEpisodeStart = true;
        panelVisible = true;

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        RefreshText();
    }

#if UNITY_EDITOR
    private void SimulateEpisodeStartInEditor()
    {
        double startTime = PhotonNetwork.InRoom ? PhotonNetwork.Time : Time.time;
        ApplyEpisodeStart(editorDebugTrialNumber, editorDebugEpisodeNumber, startTime);
        Debug.Log("Editor debug episode start simulated: Trial " + editorDebugTrialNumber + ", Episode " + editorDebugEpisodeNumber + ".");

        editorDebugEpisodeNumber++;
        if (editorDebugEpisodeNumber > 3)
        {
            editorDebugEpisodeNumber = 1;
            editorDebugTrialNumber++;
        }

        if (editorDebugTrialNumber > 2)
        {
            editorDebugTrialNumber = 1;
        }
    }
#endif

    private void RefreshText()
    {
        if (titleText == null || statusText == null || timerText == null)
        {
            return;
        }

        titleText.text = "Episode Timer";

        if (!hasEpisodeStart)
        {
            statusText.text = "Waiting for episode start";
            timerText.text = "00:00";
            return;
        }

        statusText.text = "Trial " + currentTrialNumber + " / Episode " + currentEpisodeNumber;
        float elapsedSeconds = Mathf.Max(0f, (float)((PhotonNetwork.InRoom ? PhotonNetwork.Time : Time.time) - episodeStartTime));
        int minutes = Mathf.FloorToInt(elapsedSeconds / 60f);
        int seconds = Mathf.FloorToInt(elapsedSeconds % 60f);
        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private void BuildPanel()
    {
        if (panelRoot != null)
        {
            return;
        }

        cachedCamera = Camera.main;
        panelRoot = new GameObject("Block1 Member Episode Timer Canvas", typeof(RectTransform));

        Canvas canvas = panelRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 850;
        DashboardOverlayRendering.ConfigureCanvas(canvas, 850);

        panelRoot.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 12f;

        RectTransform rootRect = panelRoot.GetComponent<RectTransform>();
        rootRect.sizeDelta = panelSize;

        GameObject panelObject = CreateRect("Panel", panelRoot.transform);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;

        Image panel = panelObject.AddComponent<Image>();
        panel.color = new Color(0.045f, 0.055f, 0.07f, 0.94f);
        panel.raycastTarget = false;
        DashboardOverlayRendering.ApplyToGraphic(panel);

        titleText = CreateText("Title", panelRect, "Episode Timer", new Vector2(0f, 48f), new Vector2(360f, 34f), 24f, FontStyles.Bold, TextAlignmentOptions.Center);
        statusText = CreateText("Status", panelRect, "Waiting for episode start", new Vector2(0f, 8f), new Vector2(350f, 30f), 18f, FontStyles.Normal, TextAlignmentOptions.Center);
        timerText = CreateText("Timer", panelRect, "00:00", new Vector2(0f, -42f), new Vector2(320f, 48f), 36f, FontStyles.Bold, TextAlignmentOptions.Center);

        EnsureCameraAttachment();
        DashboardOverlayRendering.ApplyToRoot(panelRoot);
        panelRoot.SetActive(panelVisible);
    }

    private void EnsureCameraAttachment()
    {
        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        if (panelRoot == null || cachedCamera == null)
        {
            return;
        }

        if (panelRoot.transform.parent != cachedCamera.transform)
        {
            panelRoot.transform.SetParent(cachedCamera.transform, false);
        }

        panelRoot.transform.localPosition = cameraLocalPosition;
        panelRoot.transform.localRotation = Quaternion.identity;
        panelRoot.transform.localScale = Vector3.one * worldScale;

        Canvas canvas = panelRoot.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = cachedCamera;
        }
    }

    private void DestroyPanel()
    {
        if (panelRoot != null)
        {
            Destroy(panelRoot);
            panelRoot = null;
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
            LoginSession.Role == LoginUserRole.Member &&
            IsMemberAvatar(LoginSession.AvatarName) &&
            SceneManager.GetActiveScene().name == ControlledSceneName;
    }

    private static bool IsMemberAvatar(string avatarName)
    {
        return avatarName == "ZHZ" || avatarName == "DCY" || avatarName == "ZJR";
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

public class Block1MemberEpisodeTimerPanelBootstrap : MonoBehaviour
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
        Block1MemberEpisodeTimerPanel.TryCreateForCurrentScene();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextCheckTime)
        {
            return;
        }

        nextCheckTime = Time.unscaledTime + 0.5f;
        Block1MemberEpisodeTimerPanel.TryCreateForCurrentScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Block1MemberEpisodeTimerPanel.TryCreateForCurrentScene();
    }
}
