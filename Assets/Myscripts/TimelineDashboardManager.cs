using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TimelineDashboardManager : MonoBehaviour
{
    private const string ControlledPlayerName = "GCHbot";

    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, -0.04f, 1.25f);
    [SerializeField] private float worldScale = 0.0018f;
    [SerializeField] private Vector2 panelSize = new Vector2(560f, 300f);
    [SerializeField] private KeyCode toggleKey = KeyCode.K;
    [SerializeField] private float segmentDuration = 5f;
    [SerializeField] private int segmentCount = 10;

    private readonly List<MemberEntry> members = new List<MemberEntry>();
    private Camera cachedCamera;
    private GameObject dashboardRoot;
    private RectTransform panelRect;
    private Sprite roundedFillSprite;
    private Sprite roundedFrameSprite;
    private bool dashboardVisible = true;
    private float segmentTimer;
    private bool trial3PhysioTimelineRecording;
    private int lastTrial3PhysioBlockNumber = -1;
    private int lastTrial3PhysioPhaseNumber = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!OfficeSceneSupport.ShouldShowRuntimeUi(activeScene.name) || FindObjectOfType<TimelineDashboardManager>() != null)
        {
            return;
        }

        GameObject manager = new GameObject("Timeline Dashboard Manager");
        manager.AddComponent<TimelineDashboardManager>();
    }

    private IEnumerator Start()
    {
        yield return null;
        cachedCamera = Camera.main;
        segmentCount = Mathf.Max(4, segmentCount);
        segmentDuration = Mathf.Max(0.5f, segmentDuration);
        RebuildMembers();
    }

    private void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!OfficeSceneSupport.ShouldShowRuntimeUi(sceneName))
        {
            if (dashboardRoot != null && dashboardRoot.activeSelf)
            {
                dashboardRoot.SetActive(false);
            }

            return;
        }

        bool rightHandTogglePressed = sceneName == OfficeSceneSupport.OfficeLoggedIn &&
            OfficeVrControllerInput.GetBDown();
        if (DiskSelectorController.IsTimelineDashboardSelected && (Input.GetKeyDown(toggleKey) || rightHandTogglePressed))
        {
            dashboardVisible = !dashboardVisible;
        }

        bool forceTrial3SummaryDashboard = Study2Trial3PhysioFeedbackTestController.ShouldForceTimelineDashboardForTrial3Summary();
        if (forceTrial3SummaryDashboard)
        {
            dashboardVisible = true;
        }

        bool holdTrial3SummaryDashboard = Study2Trial3PhysioFeedbackTestController.ShouldHoldTimelineDashboardUntilSummaryStart();
        bool recordTimeline = ShouldRecordTimelineSamples(forceTrial3SummaryDashboard);
        if (recordTimeline)
        {
            segmentTimer += Time.deltaTime;
            while (segmentTimer >= segmentDuration)
            {
                segmentTimer -= segmentDuration;
                AdvanceTimeline();
            }
        }

        bool shouldShow = DiskSelectorController.IsTimelineDashboardSelected && dashboardVisible && !holdTrial3SummaryDashboard;
        if (!shouldShow)
        {
            if (dashboardRoot != null && dashboardRoot.activeSelf)
            {
                dashboardRoot.SetActive(false);
            }

            return;
        }

        EnsureDashboardBuilt();
        EnsureCameraAttachment();
        RefreshAllRows(recordTimeline);

        if (dashboardRoot != null && dashboardRoot.activeSelf != shouldShow)
        {
            dashboardRoot.SetActive(shouldShow);
        }
    }

    private void RebuildMembers()
    {
        members.Clear();
        SpeakingIntention[] intentions = FindObjectsOfType<SpeakingIntention>();
        List<SpeakingIntention> sortedIntentions = new List<SpeakingIntention>(intentions);
        sortedIntentions.Sort((left, right) => string.Compare(left.name, right.name, System.StringComparison.Ordinal));

        for (int i = 0; i < sortedIntentions.Count; i++)
        {
            SpeakingIntention intention = sortedIntentions[i];
            if (intention == null || IsControlledPlayer(intention))
            {
                continue;
            }

            Color[] samples = new Color[segmentCount];
            for (int j = 0; j < samples.Length; j++)
            {
                samples[j] = Color.black;
            }

            samples[samples.Length - 1] = GetTimelineColor(intention.speaking_intention);
            members.Add(new MemberEntry
            {
                Intention = intention,
                MemberName = intention.gameObject.name,
                Samples = samples
            });
        }
    }

    private void BuildDashboard()
    {
        if (dashboardRoot != null)
        {
            Destroy(dashboardRoot);
        }

        dashboardRoot = new GameObject("Timeline Dashboard", typeof(RectTransform));
        Canvas canvas = dashboardRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        DashboardOverlayRendering.ConfigureCanvas(canvas, 630);
        dashboardRoot.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 12f;

        RectTransform canvasRect = dashboardRoot.GetComponent<RectTransform>();
        canvasRect.sizeDelta = panelSize;
        dashboardRoot.transform.localScale = Vector3.one * worldScale;
        EnsureCameraAttachment();

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

        CreateText("Title", panelRect, "Recent intention timeline", new Vector2(0f, 108f), new Vector2(460f, 34f), 24f, FontStyles.Bold, TextAlignmentOptions.Center);
        CreateText("Subtitle", panelRect, "(5s per block, left = past, right = now)", new Vector2(0f, 78f), new Vector2(420f, 24f), 16f, FontStyles.Normal, TextAlignmentOptions.Center);
        CreateText("Member Header", panelRect, "Member", new Vector2(-195f, 43f), new Vector2(120f, 24f), 18f, FontStyles.Normal, TextAlignmentOptions.Left);
        float firstRowY = 10f;
        float rowHeight = 42f;
        float timelineStartX = -100f;
        float blockSize = 18f;
        float blockSpacing = 11f;
        float timelineTopY = 31f;
        float timelineHeight = members.Count > 0 ? (members.Count - 1) * rowHeight + 34f : 34f;
        float timelineCenterY = firstRowY - (members.Count - 1) * rowHeight * 0.5f;
        float labelY = timelineTopY + 19f;
        float nowMarkerX = GetTimelineBoundaryX(segmentCount, timelineStartX, blockSize, blockSpacing);

        CreateText("Now Label", panelRect, "Now", new Vector2(nowMarkerX, labelY), new Vector2(70f, 20f), 13f, FontStyles.Normal, TextAlignmentOptions.Center);
        CreateTimelineMarker("Now Marker", panelRect, string.Empty, nowMarkerX, timelineCenterY, timelineHeight, labelY, false);
        for (int blockOffset = 4; blockOffset < segmentCount; blockOffset += 4)
        {
            int blockIndex = segmentCount - blockOffset;
            int secondsAgo = Mathf.RoundToInt(blockOffset * segmentDuration);
            CreateTimelineMarker(
                secondsAgo + "s Ago Marker",
                panelRect,
                "-" + secondsAgo + "s",
                GetTimelineBoundaryX(blockIndex, timelineStartX, blockSize, blockSpacing),
                timelineCenterY,
                timelineHeight,
                labelY,
                false);
        }

        for (int i = 0; i < members.Count; i++)
        {
            MemberEntry member = members[i];
            float y = firstRowY - i * rowHeight;

            Image rowBackground = CreateImage(
                "Row " + member.MemberName,
                panelRect,
                new Vector2(0f, y),
                new Vector2(490f, 36f),
                roundedFillSprite,
                i % 2 == 0 ? new Color(0.1f, 0.12f, 0.15f, 0.46f) : new Color(0.075f, 0.09f, 0.115f, 0.46f));
            rowBackground.raycastTarget = false;

            member.NameText = CreateText(member.MemberName + " Name", panelRect, member.MemberName, new Vector2(-195f, y), new Vector2(120f, 28f), 19f, FontStyles.Normal, TextAlignmentOptions.Left);
            member.Blocks = new Image[segmentCount];

            for (int j = 0; j < segmentCount; j++)
            {
                float x = timelineStartX + j * (blockSize + blockSpacing);
                member.Blocks[j] = CreateImage(
                    member.MemberName + " Segment " + j,
                    panelRect,
                    new Vector2(x, y),
                    new Vector2(blockSize, blockSize),
                    roundedFillSprite,
                    Color.black);
            }
        }

        dashboardRoot.SetActive(false);
        DashboardOverlayRendering.ApplyToRoot(dashboardRoot);
    }

    private void EnsureDashboardBuilt()
    {
        if (dashboardRoot != null)
        {
            return;
        }

        roundedFillSprite = roundedFillSprite != null ? roundedFillSprite : CreateRoundedFillSprite(128, 10);
        roundedFrameSprite = roundedFrameSprite != null ? roundedFrameSprite : CreateRoundedFrameSprite(128, 10, 3);
        BuildDashboard();
    }

    private static float GetTimelineBoundaryX(int blockIndex, float timelineStartX, float blockSize, float blockSpacing)
    {
        return timelineStartX + blockIndex * (blockSize + blockSpacing) - blockSize * 0.5f - blockSpacing * 0.5f;
    }

    private static void CreateTimelineMarker(
        string objectName,
        Transform parent,
        string label,
        float x,
        float centerY,
        float height,
        float labelY,
        bool labelAbove)
    {
        Image line = CreateImage(
            objectName + " Line",
            parent,
            new Vector2(x, centerY),
            new Vector2(2.2f, height),
            null,
            new Color(1f, 1f, 1f, 0.86f));
        line.raycastTarget = false;

        if (!string.IsNullOrEmpty(label))
        {
            float offsetY = labelAbove ? 12f : 0f;
            CreateText(
                objectName + " Label",
                parent,
                label,
                new Vector2(x, labelY + offsetY),
                new Vector2(76f, 20f),
                13f,
                FontStyles.Normal,
                TextAlignmentOptions.Center);
        }
    }

    private bool ShouldRecordTimelineSamples(bool forceTrial3SummaryDashboard)
    {
        bool trial3PhysioActive = Study2Trial3PhysioFeedbackTestController.IsFeedbackOverrideActive;
        if (!trial3PhysioActive)
        {
            trial3PhysioTimelineRecording = false;
            lastTrial3PhysioBlockNumber = -1;
            lastTrial3PhysioPhaseNumber = -1;
            return true;
        }

        int phaseNumber = Study2Trial3PhysioFeedbackTestController.ActivePhaseNumber;
        bool isEpisodePhase = phaseNumber >= Study2TrialPhaseInfo.Episode1 &&
            phaseNumber <= Study2TrialPhaseInfo.Episode3;

        if (isEpisodePhase)
        {
            int blockNumber = Study2Trial3PhysioFeedbackTestController.ActiveBlockNumber;
            if (!trial3PhysioTimelineRecording ||
                lastTrial3PhysioBlockNumber != blockNumber ||
                phaseNumber == Study2TrialPhaseInfo.Episode1 && lastTrial3PhysioPhaseNumber != Study2TrialPhaseInfo.Episode1)
            {
                ResetTimelineSamples();
                segmentTimer = 0f;
            }

            trial3PhysioTimelineRecording = true;
            lastTrial3PhysioBlockNumber = blockNumber;
            lastTrial3PhysioPhaseNumber = phaseNumber;
            return true;
        }

        if (forceTrial3SummaryDashboard)
        {
            lastTrial3PhysioPhaseNumber = Study2TrialPhaseInfo.Summary;
            return false;
        }

        return false;
    }

    private void ResetTimelineSamples()
    {
        EnsureMembersReadyForTimelineReset();

        for (int i = 0; i < members.Count; i++)
        {
            MemberEntry member = members[i];
            if (member.Samples == null)
            {
                continue;
            }

            for (int j = 0; j < member.Samples.Length; j++)
            {
                member.Samples[j] = Color.black;
            }

            if (member.Samples.Length > 0 && member.Intention != null)
            {
                member.Samples[member.Samples.Length - 1] = GetTimelineColor(member.Intention.speaking_intention);
            }
        }
    }

    private void EnsureMembersReadyForTimelineReset()
    {
        if (members.Count == 0)
        {
            RebuildMembers();
            return;
        }

        for (int i = 0; i < members.Count; i++)
        {
            if (members[i].Intention == null || members[i].Samples == null)
            {
                RebuildMembers();
                return;
            }
        }
    }

    private void AdvanceTimeline()
    {
        for (int i = 0; i < members.Count; i++)
        {
            MemberEntry member = members[i];
            if (member.Intention == null || member.Samples == null || member.Samples.Length == 0)
            {
                continue;
            }

            for (int j = 0; j < member.Samples.Length - 1; j++)
            {
                member.Samples[j] = member.Samples[j + 1];
            }

            member.Samples[member.Samples.Length - 1] = GetTimelineColor(member.Intention.speaking_intention);
        }
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

    private void RefreshAllRows(bool updateCurrentSample)
    {
        for (int i = 0; i < members.Count; i++)
        {
            MemberEntry member = members[i];
            if (member.Intention == null || member.Samples == null || member.Blocks == null)
            {
                continue;
            }

            if (updateCurrentSample)
            {
                member.Samples[member.Samples.Length - 1] = GetTimelineColor(member.Intention.speaking_intention);
            }

            if (member.NameText != null)
            {
                ApplyReadableWhiteText(member.NameText);
            }

            int count = Mathf.Min(member.Samples.Length, member.Blocks.Length);
            for (int j = 0; j < count; j++)
            {
                if (member.Blocks[j] != null)
                {
                    member.Blocks[j].color = member.Samples[j];
                }
            }
        }
    }

    private static Color GetTimelineColor(float speakingIntention)
    {
        return GradedHaloDisplayManager.GetGradedColor(speakingIntention);
    }

    private static bool IsControlledPlayer(SpeakingIntention intention)
    {
        return intention != null && intention.gameObject.name == ControlledPlayerName;
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

    private sealed class MemberEntry
    {
        public SpeakingIntention Intention;
        public string MemberName;
        public Color[] Samples;
        public TextMeshProUGUI NameText;
        public Image[] Blocks;
    }
}
