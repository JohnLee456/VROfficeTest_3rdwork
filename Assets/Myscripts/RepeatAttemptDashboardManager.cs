using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RepeatAttemptDashboardManager : MonoBehaviour
{
    private const float AttemptThreshold = 70f;
    private const string ControlledPlayerName = "GCHbot";

    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, -0.04f, 1.25f);
    [SerializeField] private float worldScale = 0.0018f;
    [SerializeField] private Vector2 panelSize = new Vector2(540f, 310f);
    [SerializeField] private KeyCode toggleKey = KeyCode.K;

    private readonly List<MemberEntry> members = new List<MemberEntry>();
    private Camera cachedCamera;
    private GameObject dashboardRoot;
    private RectTransform panelRect;
    private Sprite roundedFillSprite;
    private Sprite roundedFrameSprite;
    private bool dashboardVisible = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!OfficeSceneSupport.ShouldShowRuntimeUi(activeScene.name) || FindObjectOfType<RepeatAttemptDashboardManager>() != null)
        {
            return;
        }

        GameObject manager = new GameObject("Repeat Attempt Dashboard Manager");
        manager.AddComponent<RepeatAttemptDashboardManager>();
    }

    private IEnumerator Start()
    {
        yield return null;
        cachedCamera = Camera.main;
        roundedFillSprite = CreateRoundedFillSprite(128, 12);
        roundedFrameSprite = CreateRoundedFrameSprite(128, 12, 3);
        RebuildMembers();
        BuildDashboard();
        RefreshAllRows();
    }

    private void Update()
    {
        if (DiskSelectorController.IsRepeatAttemptDashboardSelected && (Input.GetKeyDown(toggleKey) || OfficeVrControllerInput.GetYDown()))
        {
            dashboardVisible = !dashboardVisible;
        }

        bool shouldShow = DiskSelectorController.IsRepeatAttemptDashboardSelected && dashboardVisible;
        EnsureCameraAttachment();

        for (int i = 0; i < members.Count; i++)
        {
            MemberEntry member = members[i];
            if (member.Intention == null)
            {
                continue;
            }

            bool isAboveThreshold = member.Intention.speaking_intention >= AttemptThreshold;
            if (!member.WasAboveThreshold && isAboveThreshold)
            {
                member.Attempts++;
            }

            member.WasAboveThreshold = isAboveThreshold;
            RefreshRow(member);
        }

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

            members.Add(new MemberEntry
            {
                Intention = intention,
                MemberName = intention.gameObject.name,
                WasAboveThreshold = intention.speaking_intention >= AttemptThreshold
            });
        }
    }

    private void BuildDashboard()
    {
        if (dashboardRoot != null)
        {
            Destroy(dashboardRoot);
        }

        dashboardRoot = new GameObject("Repeat Attempt Dashboard", typeof(RectTransform));
        Canvas canvas = dashboardRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        DashboardOverlayRendering.ConfigureCanvas(canvas, 620);
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

        CreateText("Title", panelRect, "Recent attempt summary", new Vector2(0f, 112f), new Vector2(440f, 34f), 24f, FontStyles.Bold, TextAlignmentOptions.Center);
        CreateText("Member Header", panelRect, "Member", new Vector2(-160f, 66f), new Vector2(140f, 24f), 18f, FontStyles.Normal, TextAlignmentOptions.Left);
        CreateText("Attempts Header", panelRect, "Attempts", new Vector2(30f, 66f), new Vector2(110f, 24f), 18f, FontStyles.Normal, TextAlignmentOptions.Center);
        CreateText("Recent Level Header", panelRect, "Recent level", new Vector2(168f, 66f), new Vector2(150f, 24f), 18f, FontStyles.Normal, TextAlignmentOptions.Center);

        float firstRowY = 28f;
        float rowHeight = 44f;
        for (int i = 0; i < members.Count; i++)
        {
            MemberEntry member = members[i];
            float y = firstRowY - i * rowHeight;

            Image rowBackground = CreateImage(
                "Row " + member.MemberName,
                panelRect,
                new Vector2(0f, y),
                new Vector2(470f, 38f),
                roundedFillSprite,
                i % 2 == 0 ? new Color(0.1f, 0.12f, 0.15f, 0.46f) : new Color(0.075f, 0.09f, 0.115f, 0.46f));
            rowBackground.raycastTarget = false;

            member.NameText = CreateText(member.MemberName + " Name", panelRect, member.MemberName, new Vector2(-160f, y), new Vector2(140f, 30f), 20f, FontStyles.Normal, TextAlignmentOptions.Left);
            member.AttemptsText = CreateText(member.MemberName + " Attempts", panelRect, "0", new Vector2(30f, y), new Vector2(90f, 30f), 20f, FontStyles.Normal, TextAlignmentOptions.Center);
            member.RecentLevelImage = CreateImage(member.MemberName + " Recent Level", panelRect, new Vector2(168f, y), new Vector2(104f, 30f), roundedFillSprite, Color.white);
            member.RecentLevelText = CreateText(member.MemberName + " Recent Level Text", member.RecentLevelImage.rectTransform, string.Empty, Vector2.zero, new Vector2(92f, 26f), 17f, FontStyles.Bold, TextAlignmentOptions.Center);
        }

        dashboardRoot.SetActive(false);
        DashboardOverlayRendering.ApplyToRoot(dashboardRoot);
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

    private void RefreshAllRows()
    {
        for (int i = 0; i < members.Count; i++)
        {
            RefreshRow(members[i]);
        }
    }

    private static void RefreshRow(MemberEntry member)
    {
        if (member.Intention == null || member.AttemptsText == null || member.RecentLevelImage == null || member.RecentLevelText == null)
        {
            return;
        }

        float value = member.Intention.speaking_intention;
        Color levelColor = GradedHaloDisplayManager.GetGradedColor(value);
        member.AttemptsText.text = member.Attempts.ToString();
        ApplyReadableWhiteText(member.AttemptsText);
        ApplyReadableWhiteText(member.NameText);
        member.RecentLevelImage.color = levelColor;
        member.RecentLevelText.text = GetLevelLabel(value);
        ApplyReadableWhiteText(member.RecentLevelText);
    }

    private static bool IsControlledPlayer(SpeakingIntention intention)
    {
        return intention != null && intention.gameObject.name == ControlledPlayerName;
    }

    private static string GetLevelLabel(float speakingIntention)
    {
        float value = Mathf.Clamp(speakingIntention, 0f, 100f);
        if (value < 40f)
        {
            return "0-40";
        }

        if (value < 60f)
        {
            return "40-60";
        }

        if (value < 70f)
        {
            return "60-70";
        }

        if (value < 80f)
        {
            return "70-80";
        }

        if (value < 90f)
        {
            return "80-90";
        }

        return "90-100";
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
        public int Attempts;
        public bool WasAboveThreshold;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI AttemptsText;
        public Image RecentLevelImage;
        public TextMeshProUGUI RecentLevelText;
    }
}
