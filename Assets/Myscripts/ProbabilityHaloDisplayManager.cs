using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProbabilityHaloDisplayManager : MonoBehaviour
{
    private const string DisplayRootName = "Probability Halo Dialog";
    private const string ControlledPlayerName = "GCHbot";

    [SerializeField] private Vector3 headLocalOffset = new Vector3(0f, 0.82f, 0f);
    [SerializeField] private Vector2 dialogSize = new Vector2(260f, 96f);
    [SerializeField] private float worldScale = 0.0025f;

    private readonly List<DisplayEntry> displays = new List<DisplayEntry>();
    private Camera cachedCamera;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != "OfficeLoggedIn" || FindObjectOfType<ProbabilityHaloDisplayManager>() != null)
        {
            return;
        }

        GameObject manager = new GameObject("Probability Halo Display Manager");
        manager.AddComponent<ProbabilityHaloDisplayManager>();
    }

    private IEnumerator Start()
    {
        yield return null;
        cachedCamera = Camera.main;
        RebuildDisplays();
    }

    private void Update()
    {
        bool shouldShow = DiskSelectorController.IsProbabilityHaloSelected;
        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        for (int i = 0; i < displays.Count; i++)
        {
            DisplayEntry display = displays[i];
            if (display.Root == null || display.Intention == null)
            {
                continue;
            }

            if (display.Root.activeSelf != shouldShow)
            {
            display.Root.SetActive(shouldShow);
            }

            display.Text.text = $"{Mathf.RoundToInt(display.Intention.speaking_intention)}%";
            ApplyReadableWhiteText(display.Text);

            if (shouldShow && cachedCamera != null)
            {
                Vector3 direction = display.Root.transform.position - cachedCamera.transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    display.Root.transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
    }

    private void RebuildDisplays()
    {
        SpeakingIntention[] intentions = FindObjectsOfType<SpeakingIntention>();
        for (int i = 0; i < intentions.Length; i++)
        {
            if (intentions[i] == null || IsControlledPlayer(intentions[i]))
            {
                continue;
            }

            Transform anchor = FindHeadTransform(intentions[i].transform);
            if (anchor == null)
            {
                anchor = intentions[i].transform;
            }

            Transform oldDisplay = anchor.Find(DisplayRootName);
            if (oldDisplay != null)
            {
                Destroy(oldDisplay.gameObject);
            }

            displays.Add(CreateDisplay(intentions[i], anchor));
        }
    }

    private DisplayEntry CreateDisplay(SpeakingIntention intention, Transform anchor)
    {
        GameObject root = new GameObject(DisplayRootName, typeof(RectTransform));
        root.transform.SetParent(anchor, false);
        root.transform.localPosition = headLocalOffset;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one * worldScale;

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 80;

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = dialogSize;

        GameObject panel = CreateRect("Panel", root.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.02f, 0.08f, 0.13f, 0.88f);

        GameObject labelObject = CreateRect("Value", panel.transform);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(12f, 8f);
        labelRect.offsetMax = new Vector2(-12f, -8f);

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 48f;
        label.fontStyle = FontStyles.Bold;
        label.raycastTarget = false;
        ApplyReadableWhiteText(label);

        GameObject tail = CreateRect("Tail", root.transform);
        RectTransform tailRect = tail.GetComponent<RectTransform>();
        tailRect.anchorMin = new Vector2(0.5f, 0f);
        tailRect.anchorMax = new Vector2(0.5f, 0f);
        tailRect.pivot = new Vector2(0.5f, 0.5f);
        tailRect.anchoredPosition = new Vector2(0f, -20f);
        tailRect.sizeDelta = new Vector2(34f, 34f);
        tailRect.localEulerAngles = new Vector3(0f, 0f, 45f);

        Image tailImage = tail.AddComponent<Image>();
        tailImage.color = panelImage.color;

        root.SetActive(false);
        return new DisplayEntry(intention, root, label);
    }

    private static Transform FindHeadTransform(Transform root)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == "Head_4" || children[i].name == "Head")
            {
                return children[i];
            }
        }

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name.ToLowerInvariant().Contains("head"))
            {
                return children[i];
            }
        }

        return null;
    }

    private static bool IsControlledPlayer(SpeakingIntention intention)
    {
        return intention != null && intention.gameObject.name == ControlledPlayerName;
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
        label.outlineWidth = 0.12f;
        label.enableVertexGradient = false;
        label.overrideColorTags = true;

        if (label.fontSharedMaterial != null)
        {
            label.fontMaterial = new Material(label.fontSharedMaterial);
            label.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
            label.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0f, 0f, 0f, 0.72f));
            label.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.12f);
        }
    }

    private static GameObject CreateRect(string objectName, Transform parent)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private readonly struct DisplayEntry
    {
        public readonly SpeakingIntention Intention;
        public readonly GameObject Root;
        public readonly TextMeshProUGUI Text;

        public DisplayEntry(SpeakingIntention intention, GameObject root, TextMeshProUGUI text)
        {
            Intention = intention;
            Root = root;
            Text = text;
        }
    }
}
