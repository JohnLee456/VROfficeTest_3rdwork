using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class DirectionalPeripheralHaloDisplayManager : MonoBehaviour
{
    private const string ControlledPlayerName = "GCHbot";

    [SerializeField] private float intentionThreshold = 60f;
    [SerializeField] private float refreshTargetsInterval = 2f;
    [SerializeField] private float cameraDistance = 1.2f;
    [SerializeField] private float chevronWidthRatio = 0.16f;
    [SerializeField] private float chevronHeightRatio = 0.44f;
    [SerializeField] private float horizontalInsetRatio = 0.035f;
    [SerializeField] private float viewportPadding = 0.02f;
    [SerializeField] private float sideDeadZone = 0.08f;
    [SerializeField] private Color haloColor = new Color(0.12f, 0.56f, 1f, 0.92f);

    private readonly List<TargetEntry> targets = new List<TargetEntry>();
    private Camera cachedCamera;
    private Transform leftHalo;
    private Transform rightHalo;
    private Material haloMaterial;
    private float nextRefreshTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!OfficeSceneSupport.ShouldShowRuntimeUi(activeScene.name) || FindObjectOfType<DirectionalPeripheralHaloDisplayManager>() != null)
        {
            return;
        }

        GameObject manager = new GameObject("Directional Peripheral Halo Display Manager");
        manager.AddComponent<DirectionalPeripheralHaloDisplayManager>();
    }

    private IEnumerator Start()
    {
        yield return null;
        cachedCamera = Camera.main;
        EnsureIndicators();
        RebuildTargets();
    }

    private void Update()
    {
        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
            EnsureIndicators();
        }

        if (Time.time >= nextRefreshTime)
        {
            nextRefreshTime = Time.time + refreshTargetsInterval;
            RebuildTargets();
        }

        bool selected = DiskSelectorController.IsDirectionalPeripheralHaloSelected &&
            !Study2HaloVisibilityPolicy.ShouldSuppressHaloForCurrentPhase();
        if (!selected || cachedCamera == null || leftHalo == null || rightHalo == null)
        {
            SetHaloVisible(false, false);
            return;
        }

        UpdateIndicatorLayout();
        EvaluateTargets(out bool showLeft, out bool showRight);
        SetHaloVisible(showLeft, showRight);
    }

    private void EvaluateTargets(out bool showLeft, out bool showRight)
    {
        showLeft = false;
        showRight = false;

        for (int i = 0; i < targets.Count; i++)
        {
            TargetEntry target = targets[i];
            if (target.Intention == null || target.Intention.speaking_intention <= intentionThreshold)
            {
                continue;
            }

            Transform anchor = target.Anchor != null ? target.Anchor : target.Intention.transform;
            if (anchor == null || IsSelfTarget(target.Intention.transform))
            {
                continue;
            }

            Vector3 worldPosition = anchor.position;
            Vector3 viewportPosition = cachedCamera.WorldToViewportPoint(worldPosition);
            if (IsInsideCameraView(viewportPosition))
            {
                continue;
            }

            Vector3 localPosition = cachedCamera.transform.InverseTransformPoint(worldPosition);
            if (localPosition.x < -sideDeadZone)
            {
                showLeft = true;
            }
            else if (localPosition.x > sideDeadZone)
            {
                showRight = true;
            }

            if (showLeft && showRight)
            {
                return;
            }
        }
    }

    private bool IsInsideCameraView(Vector3 viewportPosition)
    {
        return viewportPosition.z > cachedCamera.nearClipPlane
            && viewportPosition.x >= viewportPadding
            && viewportPosition.x <= 1f - viewportPadding
            && viewportPosition.y >= viewportPadding
            && viewportPosition.y <= 1f - viewportPadding;
    }

    private bool IsSelfTarget(Transform targetRoot)
    {
        if (targetRoot == null || cachedCamera == null)
        {
            return false;
        }

        return targetRoot.name == ControlledPlayerName
            || targetRoot == cachedCamera.transform
            || targetRoot.IsChildOf(cachedCamera.transform)
            || cachedCamera.transform.IsChildOf(targetRoot);
    }

    private void SetHaloVisible(bool showLeft, bool showRight)
    {
        if (leftHalo != null && leftHalo.gameObject.activeSelf != showLeft)
        {
            leftHalo.gameObject.SetActive(showLeft);
        }

        if (rightHalo != null && rightHalo.gameObject.activeSelf != showRight)
        {
            rightHalo.gameObject.SetActive(showRight);
        }
    }

    private void RebuildTargets()
    {
        targets.Clear();
        SpeakingIntention[] intentions = FindObjectsOfType<SpeakingIntention>();
        for (int i = 0; i < intentions.Length; i++)
        {
            SpeakingIntention intention = intentions[i];
            if (intention == null || IsSelfTarget(intention.transform))
            {
                continue;
            }

            Transform anchor = FindHeadTransform(intention.transform);
            if (anchor == null)
            {
                anchor = intention.transform;
            }

            targets.Add(new TargetEntry(intention, anchor));
        }
    }

    private void EnsureIndicators()
    {
        if (leftHalo != null || rightHalo != null || cachedCamera == null)
        {
            return;
        }

        DestroyOldCanvasIndicator();
        haloMaterial = CreateHaloMaterial();
        leftHalo = CreateChevronIndicator("Left Directional Peripheral Halo", true);
        rightHalo = CreateChevronIndicator("Right Directional Peripheral Halo", false);
        UpdateIndicatorLayout();
        SetHaloVisible(false, false);
    }

    private Transform CreateChevronIndicator(string objectName, bool pointsLeft)
    {
        GameObject root = new GameObject(objectName);
        root.transform.SetParent(cachedCamera.transform, false);
        root.transform.localRotation = Quaternion.identity;

        AddChevronLine(root.transform, pointsLeft, "Outer Glow", new Color(haloColor.r, haloColor.g, haloColor.b, 0.18f), 0.055f, -2);
        AddChevronLine(root.transform, pointsLeft, "Middle Glow", new Color(haloColor.r, haloColor.g, haloColor.b, 0.34f), 0.032f, -1);
        AddChevronLine(root.transform, pointsLeft, "Core", haloColor, 0.014f, 0);
        AddChevronLine(root.transform, pointsLeft, "Hot Core", Color.white, 0.005f, 1);
        return root.transform;
    }

    private void AddChevronLine(Transform parent, bool pointsLeft, string objectName, Color color, float width, int sortingOrderOffset)
    {
        GameObject layerObject = new GameObject(objectName);
        layerObject.transform.SetParent(parent, false);
        layerObject.transform.localPosition = Vector3.zero;
        layerObject.transform.localRotation = Quaternion.identity;

        LineRenderer line = layerObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.positionCount = 3;
        line.loop = false;
        line.alignment = LineAlignment.View;
        line.textureMode = LineTextureMode.Stretch;
        line.numCapVertices = 10;
        line.numCornerVertices = 10;
        line.widthMultiplier = width;
        line.startColor = color;
        line.endColor = color;
        line.material = haloMaterial;
        line.sortingOrder = 490 + sortingOrderOffset;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        SetChevronLinePoints(line, pointsLeft, 1f, 1f);
    }

    private void UpdateIndicatorLayout()
    {
        if (cachedCamera == null || leftHalo == null || rightHalo == null)
        {
            return;
        }

        float height = 2f * cameraDistance * Mathf.Tan(cachedCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * Mathf.Max(0.01f, cachedCamera.aspect);
        float chevronWidth = width * chevronWidthRatio;
        float chevronHeight = height * chevronHeightRatio;
        float leftX = -width * 0.5f + width * horizontalInsetRatio + chevronWidth * 0.5f;
        float rightX = width * 0.5f - width * horizontalInsetRatio - chevronWidth * 0.5f;

        leftHalo.localPosition = new Vector3(leftX, 0f, cameraDistance);
        rightHalo.localPosition = new Vector3(rightX, 0f, cameraDistance);

        UpdateChevronLines(leftHalo, true, chevronWidth, chevronHeight);
        UpdateChevronLines(rightHalo, false, chevronWidth, chevronHeight);
    }

    private static void UpdateChevronLines(Transform root, bool pointsLeft, float width, float height)
    {
        LineRenderer[] lines = root.GetComponentsInChildren<LineRenderer>(true);
        for (int i = 0; i < lines.Length; i++)
        {
            SetChevronLinePoints(lines[i], pointsLeft, width, height);
        }
    }

    private static void SetChevronLinePoints(LineRenderer line, bool pointsLeft, float width, float height)
    {
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;
        Vector3 upper;
        Vector3 middle;
        Vector3 lower;

        if (pointsLeft)
        {
            upper = new Vector3(halfWidth, halfHeight, 0f);
            middle = new Vector3(-halfWidth, 0f, 0f);
            lower = new Vector3(halfWidth, -halfHeight, 0f);
        }
        else
        {
            upper = new Vector3(-halfWidth, halfHeight, 0f);
            middle = new Vector3(halfWidth, 0f, 0f);
            lower = new Vector3(-halfWidth, -halfHeight, 0f);
        }

        line.SetPosition(0, upper);
        line.SetPosition(1, middle);
        line.SetPosition(2, lower);
    }

    private static Material CreateHaloMaterial()
    {
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = new Material(shader);
        material.hideFlags = HideFlags.DontSave;
        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.One);
        material.SetInt("_Cull", (int)CullMode.Off);
        material.SetInt("_ZWrite", 0);
        material.SetInt("_ZTest", (int)CompareFunction.Always);
        material.renderQueue = (int)RenderQueue.Overlay;
        return material;
    }

    private void DestroyOldCanvasIndicator()
    {
        Transform oldCanvas = cachedCamera.transform.Find("Directional Peripheral Halo Canvas");
        if (oldCanvas != null)
        {
            Destroy(oldCanvas.gameObject);
        }
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

    private readonly struct TargetEntry
    {
        public readonly SpeakingIntention Intention;
        public readonly Transform Anchor;

        public TargetEntry(SpeakingIntention intention, Transform anchor)
        {
            Intention = intention;
            Anchor = anchor;
        }
    }
}
