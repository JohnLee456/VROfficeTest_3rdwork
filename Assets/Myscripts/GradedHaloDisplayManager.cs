using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GradedHaloDisplayManager : MonoBehaviour
{
    private const string DisplayRootName = "Graded Halo Block";

    [SerializeField] private Vector3 headLocalOffset = new Vector3(0f, 0.48f, 0f);
    [SerializeField] private Vector2 blockSize = new Vector2(0.4f, 0.16f);
    [SerializeField] private Vector2 depthOffset = new Vector2(0.018f, -0.026f);

    private readonly List<DisplayEntry> displays = new List<DisplayEntry>();
    private Camera cachedCamera;
    private Sprite roundedFillSprite;
    private Sprite roundedFrameSprite;
    private Sprite roundedGlossSprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!OfficeSceneSupport.ShouldShowRuntimeUi(activeScene.name) || FindObjectOfType<GradedHaloDisplayManager>() != null)
        {
            return;
        }

        GameObject manager = new GameObject("Graded Halo Display Manager");
        manager.AddComponent<GradedHaloDisplayManager>();
    }

    private IEnumerator Start()
    {
        yield return null;
        cachedCamera = Camera.main;
        roundedFillSprite = CreateRoundedFillSprite(160, 40);
        roundedFrameSprite = CreateRoundedFrameSprite(160, 40, 5);
        roundedGlossSprite = CreateRoundedGlossSprite(160, 40);
        RebuildDisplays();
    }

    private void Update()
    {
        bool shouldShow = DiskSelectorController.IsGradedHaloSelected &&
            !Study2HaloVisibilityPolicy.ShouldSuppressHaloForCurrentPhase();
        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        for (int i = 0; i < displays.Count; i++)
        {
            DisplayEntry display = displays[i];
            if (display.Root == null || display.Intention == null || display.Body == null)
            {
                continue;
            }

            float value = display.Intention.speaking_intention;
            bool shouldShowDisplay = shouldShow && value >= 60f;
            if (display.Root.activeSelf != shouldShowDisplay)
            {
                display.Root.SetActive(shouldShowDisplay);
            }

            if (!shouldShowDisplay)
            {
                continue;
            }

            Color color = GetGradedColor(value);
            float pulse = 0.76f + Mathf.PingPong(Time.time * 0.55f, 0.16f);
            Color darkColor = Color.Lerp(color, Color.black, 0.38f);

            display.Shadow.color = new Color(0f, 0f, 0f, 0.36f);
            display.Depth.color = new Color(darkColor.r, darkColor.g, darkColor.b, 0.92f);
            display.Glow.color = new Color(color.r, color.g, color.b, 0.22f * pulse);
            display.Frame.color = Color.Lerp(Color.white, color, 0.18f);
            display.Body.color = new Color(color.r, color.g, color.b, 0.98f);
            display.Gloss.color = new Color(1f, 1f, 1f, 0.2f);

            if (cachedCamera != null)
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
            if (intentions[i] == null)
            {
                continue;
            }

            Transform anchor = FindHeadTransform(intentions[i].transform);
            if (anchor == null)
            {
                anchor = intentions[i].transform;
            }

            DestroyOldDisplays(anchor);
            displays.Add(CreateDisplay(intentions[i], anchor));
        }
    }

    private DisplayEntry CreateDisplay(SpeakingIntention intention, Transform anchor)
    {
        GameObject root = new GameObject(DisplayRootName);
        root.transform.SetParent(anchor, false);
        root.transform.localPosition = headLocalOffset;
        root.transform.localRotation = Quaternion.identity;

        SpriteRenderer shadow = CreateSpriteLayer("Shadow", root.transform, roundedFillSprite, blockSize * 1.05f, new Vector3(depthOffset.x * 1.3f, depthOffset.y * 1.3f, -0.035f), 86);
        SpriteRenderer glow = CreateSpriteLayer("Glow", root.transform, roundedFillSprite, blockSize * 1.2f, new Vector3(0f, 0f, -0.03f), 87);
        SpriteRenderer depth = CreateSpriteLayer("Depth", root.transform, roundedFillSprite, blockSize, new Vector3(depthOffset.x, depthOffset.y, -0.02f), 88);
        SpriteRenderer frame = CreateSpriteLayer("Frame", root.transform, roundedFrameSprite, blockSize * 1.04f, new Vector3(0f, 0f, -0.01f), 89);
        SpriteRenderer body = CreateSpriteLayer("Body", root.transform, roundedFillSprite, blockSize, Vector3.zero, 90);
        SpriteRenderer gloss = CreateSpriteLayer("Gloss", root.transform, roundedGlossSprite, blockSize * 0.9f, new Vector3(0f, 0.016f, 0.008f), 91);

        Color initialColor = GetGradedColor(intention.speaking_intention);
        Color initialDarkColor = Color.Lerp(initialColor, Color.black, 0.38f);
        shadow.color = new Color(0f, 0f, 0f, 0.36f);
        glow.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0.2f);
        depth.color = new Color(initialDarkColor.r, initialDarkColor.g, initialDarkColor.b, 0.92f);
        frame.color = Color.Lerp(Color.white, initialColor, 0.18f);
        body.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0.98f);
        gloss.color = new Color(1f, 1f, 1f, 0.2f);

        root.SetActive(false);
        return new DisplayEntry(intention, root, body, glow, frame, depth, shadow, gloss);
    }

    private static void DestroyOldDisplays(Transform anchor)
    {
        string[] oldNames = { DisplayRootName, "Graded Halo Bar", "Graded Halo Tile" };
        for (int i = 0; i < oldNames.Length; i++)
        {
            Transform oldDisplay = anchor.Find(oldNames[i]);
            if (oldDisplay != null)
            {
                Destroy(oldDisplay.gameObject);
            }
        }
    }

    public static Color GetGradedColor(float speakingIntention)
    {
        float value = Mathf.Clamp(speakingIntention, 0f, 100f);

        if (value < 40f)
        {
            return new Color(0f, 0f, 0f, 1f);
        }

        if (value < 60f)
        {
            return new Color(0.64f, 0.86f, 1f, 1f);
        }

        if (value < 70f)
        {
            return new Color(0.42f, 0.72f, 1f, 1f);
        }

        if (value < 80f)
        {
            return new Color(0.24f, 0.55f, 0.95f, 1f);
        }

        if (value < 90f)
        {
            return new Color(0.1f, 0.34f, 0.78f, 1f);
        }

        return new Color(0.02f, 0.12f, 0.42f, 1f);
    }

    private static SpriteRenderer CreateSpriteLayer(string layerName, Transform parent, Sprite sprite, Vector2 size, Vector3 localPosition, int sortingOrder)
    {
        GameObject layer = new GameObject(layerName);
        layer.transform.SetParent(parent, false);
        layer.transform.localPosition = localPosition;
        layer.transform.localRotation = Quaternion.identity;
        layer.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = layer.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        return renderer;
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
                float verticalShade = Mathf.Lerp(1.16f, 0.78f, y / (float)(size - 1));
                texture.SetPixel(x, y, new Color(verticalShade, verticalShade, verticalShade, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
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
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite CreateRoundedGlossSprite(int size, int radius)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float mask = RoundedRectAlpha(x, y, size, radius);
                float normalizedY = y / (float)(size - 1);
                float band = Mathf.SmoothStep(1f, 0f, Mathf.Abs(normalizedY - 0.68f) / 0.34f);
                float leftFade = Mathf.SmoothStep(0f, 1f, x / (float)(size - 1));
                float rightFade = Mathf.SmoothStep(0f, 1f, (size - 1 - x) / (float)(size - 1));
                float alpha = mask * band * Mathf.Min(leftFade, rightFade) * 0.85f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
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

    private readonly struct DisplayEntry
    {
        public readonly SpeakingIntention Intention;
        public readonly GameObject Root;
        public readonly SpriteRenderer Body;
        public readonly SpriteRenderer Glow;
        public readonly SpriteRenderer Frame;
        public readonly SpriteRenderer Depth;
        public readonly SpriteRenderer Shadow;
        public readonly SpriteRenderer Gloss;

        public DisplayEntry(
            SpeakingIntention intention,
            GameObject root,
            SpriteRenderer body,
            SpriteRenderer glow,
            SpriteRenderer frame,
            SpriteRenderer depth,
            SpriteRenderer shadow,
            SpriteRenderer gloss)
        {
            Intention = intention;
            Root = root;
            Body = body;
            Glow = glow;
            Frame = frame;
            Depth = depth;
            Shadow = shadow;
            Gloss = gloss;
        }
    }
}
