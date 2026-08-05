using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-67)]
public class Study2Trial2SummaryDashboard : MonoBehaviour
{
    private const string ControlledSceneName = OfficeSceneSupport.OfficeLoggedInNoBot;
    private const int TargetTrialNumber = 2;
    private const float SampleInterval = 0.5f;
    private const float AttemptThreshold = 70f;

    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, -0.08f, 0.74f);
    [SerializeField] private Vector2 panelSize = new Vector2(720f, 340f);
    [SerializeField] private float worldScale = 0.00115f;

    private readonly List<MemberEntry> members = new List<MemberEntry>();
    private Camera cachedCamera;
    private GameObject dashboardRoot;
    private RectTransform panelRect;
    private Sprite roundedFillSprite;
    private Sprite roundedFrameSprite;
    private int recordingBlockNumber = -1;
    private int recordingTrialNumber = -1;
    private float nextSampleTime;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI subtitleText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForScene()
    {
        TryCreateForCurrentScene();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateBootstrap()
    {
        if (FindObjectOfType<Study2Trial2SummaryDashboardBootstrap>() != null)
        {
            return;
        }

        GameObject bootstrap = new GameObject("Study2 Trial2 Summary Dashboard Bootstrap");
        DontDestroyOnLoad(bootstrap);
        bootstrap.AddComponent<Study2Trial2SummaryDashboardBootstrap>();
    }

    public static void TryCreateForCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!ShouldCreateForScene(activeScene) || FindObjectOfType<Study2Trial2SummaryDashboard>() != null)
        {
            return;
        }

        GameObject manager = new GameObject("Study2 Trial2 Summary Dashboard");
        manager.AddComponent<Study2Trial2SummaryDashboard>();
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

        cachedCamera = Camera.main;
        RebuildMembers();
    }

    private void Update()
    {
        if (!CanShowForCurrentUser())
        {
            SetDashboardVisible(false);
            return;
        }

        EnsureMembers();
        SampleCurrentTrialIfNeeded();

        int blockNumber;
        int trialNumber;
        int phaseNumber;
        bool shouldShow = Block1EpisodeSync.TryReadRoomState(out blockNumber, out trialNumber, out phaseNumber, out _) &&
            trialNumber == TargetTrialNumber &&
            phaseNumber == Study2TrialPhaseInfo.Summary;

        if (!shouldShow)
        {
            SetDashboardVisible(false);
            return;
        }

        EnsureDashboardBuilt();
        EnsureCameraAttachment();
        RefreshDashboard(blockNumber, trialNumber);
        SetDashboardVisible(true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!ShouldCreateForScene(scene))
        {
            DestroyDashboard();
            return;
        }

        cachedCamera = Camera.main;
        RebuildMembers();
    }

    private void SampleCurrentTrialIfNeeded()
    {
        int blockNumber;
        int trialNumber;
        int phaseNumber;
        double startTime;
        if (!Block1EpisodeSync.TryReadRoomState(out blockNumber, out trialNumber, out phaseNumber, out startTime))
        {
            return;
        }

        if (trialNumber != TargetTrialNumber)
        {
            return;
        }

        if (recordingBlockNumber != blockNumber || recordingTrialNumber != trialNumber)
        {
            ResetRecording(blockNumber, trialNumber);
        }

        if (Time.time < nextSampleTime)
        {
            return;
        }

        nextSampleTime = Time.time + SampleInterval;
        for (int i = 0; i < members.Count; i++)
        {
            MemberEntry member = members[i];
            if (member.Intention == null)
            {
                continue;
            }

            float value = Mathf.Clamp(member.Intention.speaking_intention, 0f, 100f);
            member.Samples.Add(new ValueSample(Time.time, value));
            member.MinValue = member.Samples.Count == 1 ? value : Mathf.Min(member.MinValue, value);
            member.MaxValue = member.Samples.Count == 1 ? value : Mathf.Max(member.MaxValue, value);

            bool aboveThreshold = value >= AttemptThreshold;
            if (!member.WasAboveThreshold && aboveThreshold)
            {
                member.Attempts++;
            }

            member.WasAboveThreshold = aboveThreshold;
        }
    }

    private void ResetRecording(int blockNumber, int trialNumber)
    {
        recordingBlockNumber = blockNumber;
        recordingTrialNumber = trialNumber;
        nextSampleTime = 0f;

        for (int i = 0; i < members.Count; i++)
        {
            members[i].Samples.Clear();
            members[i].Attempts = 0;
            members[i].WasAboveThreshold = false;
            members[i].MinValue = 0f;
            members[i].MaxValue = 0f;
        }
    }

    private void RebuildMembers()
    {
        if (dashboardRoot != null)
        {
            Destroy(dashboardRoot);
            dashboardRoot = null;
            panelRect = null;
            titleText = null;
            subtitleText = null;
        }

        members.Clear();
        AddMember("ZHZ");
        AddMember("DCY");
        AddMember("ZJR");
        recordingBlockNumber = -1;
        recordingTrialNumber = -1;
    }

    private void EnsureMembers()
    {
        bool missing = members.Count == 0;
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i].Intention == null)
            {
                missing = true;
                break;
            }
        }

        if (missing)
        {
            RebuildMembers();
        }
    }

    private void AddMember(string memberName)
    {
        members.Add(new MemberEntry
        {
            MemberName = memberName,
            Intention = FindIntentionByObjectName(memberName)
        });
    }

    private void BuildDashboard()
    {
        if (dashboardRoot != null)
        {
            Destroy(dashboardRoot);
        }

        dashboardRoot = new GameObject("Trial2 SpeakingIntention Summary Dashboard", typeof(RectTransform));
        Canvas canvas = dashboardRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        DashboardOverlayRendering.ConfigureCanvas(canvas, 650);
        dashboardRoot.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 12f;

        RectTransform rootRect = dashboardRoot.GetComponent<RectTransform>();
        rootRect.sizeDelta = panelSize;

        GameObject shadowObject = CreateRectObject("Panel Shadow", dashboardRoot.transform);
        RectTransform shadowRect = shadowObject.GetComponent<RectTransform>();
        shadowRect.anchorMin = new Vector2(0.5f, 0.5f);
        shadowRect.anchorMax = new Vector2(0.5f, 0.5f);
        shadowRect.pivot = new Vector2(0.5f, 0.5f);
        shadowRect.anchoredPosition = new Vector2(8f, -8f);
        shadowRect.sizeDelta = panelSize;
        Image shadow = shadowObject.AddComponent<Image>();
        shadow.sprite = roundedFillSprite;
        shadow.color = new Color(0f, 0f, 0f, 0.36f);
        shadow.raycastTarget = false;
        DashboardOverlayRendering.ApplyToGraphic(shadow);

        GameObject panelObject = CreateRectObject("Panel", dashboardRoot.transform);
        panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;

        Image panel = panelObject.AddComponent<Image>();
        panel.sprite = roundedFillSprite;
        panel.color = new Color(0.055f, 0.068f, 0.085f, 0.94f);
        panel.raycastTarget = false;
        DashboardOverlayRendering.ApplyToGraphic(panel);

        Image frame = CreateImage("Frame", panelRect, Vector2.zero, panelSize + new Vector2(6f, 6f), roundedFrameSprite, new Color(0.62f, 0.72f, 0.86f, 0.5f));
        frame.raycastTarget = false;

        titleText = CreateText("Title", panelRect, "Trial 2 SpeakingIntention Summary", new Vector2(0f, 132f), new Vector2(640f, 34f), 24f, FontStyles.Bold, TextAlignmentOptions.Center);
        subtitleText = CreateText("Subtitle", panelRect, string.Empty, new Vector2(0f, 102f), new Vector2(640f, 24f), 16f, FontStyles.Normal, TextAlignmentOptions.Center);
        CreateText("Member Header", panelRect, "Member", new Vector2(-294f, 68f), new Vector2(110f, 24f), 17f, FontStyles.Normal, TextAlignmentOptions.Left);
        CreateText("Attempts Header", panelRect, "Attempts", new Vector2(-178f, 68f), new Vector2(90f, 24f), 17f, FontStyles.Normal, TextAlignmentOptions.Center);
        CreateText("Range Header", panelRect, "Range", new Vector2(-60f, 68f), new Vector2(110f, 24f), 17f, FontStyles.Normal, TextAlignmentOptions.Center);
        CreateText("Peak Header", panelRect, "Peak", new Vector2(58f, 68f), new Vector2(80f, 24f), 17f, FontStyles.Normal, TextAlignmentOptions.Center);
        CreateText("Trend Header", panelRect, "Trial trend", new Vector2(218f, 68f), new Vector2(250f, 24f), 17f, FontStyles.Normal, TextAlignmentOptions.Center);

        float firstRowY = 24f;
        float rowHeight = 56f;
        for (int i = 0; i < members.Count; i++)
        {
            MemberEntry member = members[i];
            float y = firstRowY - i * rowHeight;

            Image rowBackground = CreateImage(
                "Row " + member.MemberName,
                panelRect,
                new Vector2(0f, y),
                new Vector2(650f, 48f),
                roundedFillSprite,
                i % 2 == 0 ? new Color(0.1f, 0.12f, 0.15f, 0.46f) : new Color(0.075f, 0.09f, 0.115f, 0.46f));
            rowBackground.raycastTarget = false;

            member.NameText = CreateText(member.MemberName + " Name", panelRect, member.MemberName, new Vector2(-294f, y), new Vector2(110f, 30f), 20f, FontStyles.Normal, TextAlignmentOptions.Left);
            member.AttemptsText = CreateText(member.MemberName + " Attempts", panelRect, "0", new Vector2(-178f, y), new Vector2(80f, 30f), 20f, FontStyles.Normal, TextAlignmentOptions.Center);
            member.RangeText = CreateText(member.MemberName + " Range", panelRect, "0-0", new Vector2(-60f, y), new Vector2(104f, 30f), 18f, FontStyles.Bold, TextAlignmentOptions.Center);
            member.PeakImage = CreateImage(member.MemberName + " Peak", panelRect, new Vector2(58f, y), new Vector2(78f, 30f), roundedFillSprite, Color.white);
            member.PeakText = CreateText(member.MemberName + " Peak Text", member.PeakImage.rectTransform, string.Empty, Vector2.zero, new Vector2(68f, 26f), 17f, FontStyles.Bold, TextAlignmentOptions.Center);
            member.TrendImage = CreateImage(member.MemberName + " Trend", panelRect, new Vector2(218f, y), new Vector2(240f, 30f), roundedFillSprite, Color.white);
            member.TrendText = CreateText(member.MemberName + " Trend Text", member.TrendImage.rectTransform, string.Empty, Vector2.zero, new Vector2(228f, 26f), 18f, FontStyles.Bold, TextAlignmentOptions.Center);
        }

        DashboardOverlayRendering.ApplyToRoot(dashboardRoot);
        dashboardRoot.SetActive(false);
        EnsureCameraAttachment();
    }

    private void EnsureDashboardBuilt()
    {
        if (dashboardRoot != null)
        {
            return;
        }

        roundedFillSprite = roundedFillSprite != null ? roundedFillSprite : CreateRoundedFillSprite(128, 12);
        roundedFrameSprite = roundedFrameSprite != null ? roundedFrameSprite : CreateRoundedFrameSprite(128, 12, 3);
        BuildDashboard();
    }

    private void EnsureCameraAttachment()
    {
        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        if (dashboardRoot == null || cachedCamera == null)
        {
            return;
        }

        Transform rootTransform = dashboardRoot.transform;
        if (rootTransform.parent != cachedCamera.transform)
        {
            rootTransform.SetParent(cachedCamera.transform, false);
        }

        rootTransform.localPosition = cameraLocalPosition;
        rootTransform.localRotation = Quaternion.identity;
        rootTransform.localScale = Vector3.one * worldScale;

        Canvas canvas = dashboardRoot.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = cachedCamera;
        }
    }

    private void RefreshDashboard(int blockNumber, int trialNumber)
    {
        if (titleText != null)
        {
            titleText.text = "Block " + blockNumber + " / Trial " + trialNumber + " SpeakingIntention Summary";
            ApplyReadableWhiteText(titleText);
        }

        if (subtitleText != null)
        {
            subtitleText.text = "Recorded from Trial 2 start to Summary Stage";
            ApplyReadableWhiteText(subtitleText);
        }

        for (int i = 0; i < members.Count; i++)
        {
            RefreshRow(members[i]);
        }
    }

    private void RefreshRow(MemberEntry member)
    {
        if (member.NameText == null || member.AttemptsText == null || member.RangeText == null || member.PeakImage == null || member.PeakText == null || member.TrendImage == null || member.TrendText == null)
        {
            return;
        }

        int sampleCount = member.Samples.Count;
        float startValue = sampleCount > 0 ? member.Samples[0].Value : 0f;
        float endValue = sampleCount > 0 ? member.Samples[sampleCount - 1].Value : 0f;
        float peakValue = sampleCount > 0 ? member.MaxValue : 0f;
        float minValue = sampleCount > 0 ? member.MinValue : 0f;

        member.NameText.text = member.MemberName;
        member.AttemptsText.text = member.Attempts.ToString();
        member.RangeText.text = Mathf.RoundToInt(minValue) + "-" + Mathf.RoundToInt(peakValue);
        Color peakColor = GradedHaloDisplayManager.GetGradedColor(peakValue);
        member.PeakImage.color = new Color(peakColor.r, peakColor.g, peakColor.b, 0.94f);
        member.PeakText.text = Mathf.RoundToInt(peakValue).ToString();
        TrialTrendState trendState = GetTrendState(member.Samples, startValue, endValue);
        member.TrendImage.color = GetTrendColor(trendState);
        member.TrendText.text = GetTrendLabel(trendState, startValue, endValue);

        ApplyReadableWhiteText(member.NameText);
        ApplyReadableWhiteText(member.AttemptsText);
        ApplyReadableWhiteText(member.RangeText);
        ApplyReadableWhiteText(member.PeakText);
        ApplyReadableWhiteText(member.TrendText);
    }

    private static string GetTrendLabel(TrialTrendState state, float startValue, float endValue)
    {
        float delta = endValue - startValue;
        switch (state)
        {
            case TrialTrendState.Active:
                return "Active +" + Mathf.RoundToInt(delta);
            case TrialTrendState.Negative:
                return "Negative " + Mathf.RoundToInt(delta);
            case TrialTrendState.Calm:
                return delta >= 0f ? "Calm +" + Mathf.RoundToInt(delta) : "Calm " + Mathf.RoundToInt(delta);
            case TrialTrendState.Stable:
                return "Stable";
            default:
                return "No data";
        }
    }

    private static TrialTrendState GetTrendState(List<ValueSample> samples, float startValue, float endValue)
    {
        if (samples.Count == 0)
        {
            return TrialTrendState.NoData;
        }

        float delta = endValue - startValue;
        if (delta >= 30f)
        {
            return TrialTrendState.Active;
        }

        if (delta <= -30f)
        {
            return TrialTrendState.Negative;
        }

        if (Mathf.Abs(delta) > 8f)
        {
            return TrialTrendState.Calm;
        }

        return TrialTrendState.Stable;
    }

    private static Color GetTrendColor(TrialTrendState state)
    {
        switch (state)
        {
            case TrialTrendState.Active:
                return new Color(1f, 0.48f, 0.12f, 0.96f);
            case TrialTrendState.Negative:
                return new Color(0.9f, 0.12f, 0.18f, 0.96f);
            case TrialTrendState.Calm:
                return new Color(0.25f, 0.55f, 0.95f, 0.96f);
            case TrialTrendState.Stable:
                return new Color(0.28f, 0.78f, 0.45f, 0.96f);
            default:
                return new Color(0.28f, 0.32f, 0.38f, 0.86f);
        }
    }

    private void SetDashboardVisible(bool visible)
    {
        if (dashboardRoot != null && dashboardRoot.activeSelf != visible)
        {
            dashboardRoot.SetActive(visible);
        }
    }

    private void DestroyDashboard()
    {
        if (dashboardRoot != null)
        {
            Destroy(dashboardRoot);
            dashboardRoot = null;
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
            SceneManager.GetActiveScene().name == ControlledSceneName;
    }

    private static SpeakingIntention FindIntentionByObjectName(string objectName)
    {
        SpeakingIntention[] intentions = FindObjectsOfType<SpeakingIntention>(true);
        for (int i = 0; i < intentions.Length; i++)
        {
            if (IsSelfOrParentNamed(intentions[i].transform, objectName))
            {
                return intentions[i];
            }
        }

        return null;
    }

    private static bool IsSelfOrParentNamed(Transform transform, string objectName)
    {
        Transform current = transform;
        while (current != null)
        {
            if (current.name == objectName)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static Image CreateImage(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, Sprite sprite, Color color)
    {
        GameObject imageObject = CreateRectObject(objectName, parent);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = imageObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        DashboardOverlayRendering.ApplyToGraphic(image);
        return image;
    }

    private static TextMeshProUGUI CreateText(string objectName, Transform parent, string text, Vector2 anchoredPosition, Vector2 size, float fontSize, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject textObject = CreateRectObject(objectName, parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
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
        label.outlineColor = new Color(0f, 0f, 0f, 0.72f);
        label.outlineWidth = 0.1f;
        label.enableVertexGradient = false;
        label.overrideColorTags = true;

        if (label.fontSharedMaterial != null)
        {
            label.fontMaterial = new Material(label.fontSharedMaterial);
            label.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
            label.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0f, 0f, 0f, 0.72f));
            label.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.1f);
        }

        DashboardOverlayRendering.ApplyToText(label);
    }

    private static GameObject CreateRectObject(string objectName, Transform parent)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static Sprite CreateRoundedFillSprite(int size, int radius)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float alpha = RoundedRectAlpha(x, y, size, radius);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Sprite CreateRoundedFrameSprite(int size, int radius, int thickness)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float outer = RoundedRectAlpha(x, y, size, radius);
                float inner = RoundedRectAlpha(x - thickness, y - thickness, size - thickness * 2, Mathf.Max(0, radius - thickness));
                float alpha = Mathf.Clamp01(outer - inner);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private static float RoundedRectAlpha(int x, int y, int size, int radius)
    {
        if (size <= 0 || x < 0 || y < 0 || x >= size || y >= size)
        {
            return 0f;
        }

        float px = Mathf.Clamp(x, 0, size - 1);
        float py = Mathf.Clamp(y, 0, size - 1);
        float innerMin = radius;
        float innerMax = size - 1 - radius;
        float cx = Mathf.Clamp(px, innerMin, innerMax);
        float cy = Mathf.Clamp(py, innerMin, innerMax);
        float distance = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
        return Mathf.Clamp01(radius - distance + 1f);
    }

    private readonly struct ValueSample
    {
        public readonly float Time;
        public readonly float Value;

        public ValueSample(float time, float value)
        {
            Time = time;
            Value = value;
        }
    }

    private enum TrialTrendState
    {
        NoData,
        Stable,
        Calm,
        Active,
        Negative
    }

    private sealed class MemberEntry
    {
        public string MemberName;
        public SpeakingIntention Intention;
        public int Attempts;
        public bool WasAboveThreshold;
        public float MinValue;
        public float MaxValue;
        public readonly List<ValueSample> Samples = new List<ValueSample>();
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI AttemptsText;
        public TextMeshProUGUI RangeText;
        public Image PeakImage;
        public TextMeshProUGUI PeakText;
        public Image TrendImage;
        public TextMeshProUGUI TrendText;
    }
}

public class Study2Trial2SummaryDashboardBootstrap : MonoBehaviour
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
        Study2Trial2SummaryDashboard.TryCreateForCurrentScene();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextCheckTime)
        {
            return;
        }

        nextCheckTime = Time.unscaledTime + 0.5f;
        Study2Trial2SummaryDashboard.TryCreateForCurrentScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Study2Trial2SummaryDashboard.TryCreateForCurrentScene();
    }
}
