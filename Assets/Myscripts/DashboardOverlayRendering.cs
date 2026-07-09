using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public static class DashboardOverlayRendering
{
    private const int AlwaysOnTopRenderQueue = 5000;
    private static Material uiOverlayMaterial;

    public static void ConfigureCanvas(Canvas canvas, int sortingOrder)
    {
        if (canvas == null)
        {
            return;
        }

        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
    }

    public static void ApplyToRoot(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            ApplyToGraphic(graphics[i]);
        }
    }

    public static void ApplyToGraphic(Graphic graphic)
    {
        if (graphic == null)
        {
            return;
        }

        TextMeshProUGUI text = graphic as TextMeshProUGUI;
        if (text != null)
        {
            ApplyToText(text);
            return;
        }

        graphic.material = GetUiOverlayMaterial();
    }

    public static void ApplyToText(TextMeshProUGUI text)
    {
        if (text == null || text.fontMaterial == null)
        {
            return;
        }

        ConfigureOverlayMaterial(text.fontMaterial);
    }

    private static Material GetUiOverlayMaterial()
    {
        if (uiOverlayMaterial != null)
        {
            return uiOverlayMaterial;
        }

        Shader shader = Shader.Find("UI/Default");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        uiOverlayMaterial = new Material(shader)
        {
            hideFlags = HideFlags.DontSave
        };
        ConfigureOverlayMaterial(uiOverlayMaterial);
        return uiOverlayMaterial;
    }

    private static void ConfigureOverlayMaterial(Material material)
    {
        if (material == null)
        {
            return;
        }

        SetIntIfPresent(material, "_ZTest", (int)CompareFunction.Always);
        SetIntIfPresent(material, "_ZTestMode", (int)CompareFunction.Always);
        SetIntIfPresent(material, "unity_GUIZTestMode", (int)CompareFunction.Always);
        SetIntIfPresent(material, "_ZWrite", 0);
        material.renderQueue = AlwaysOnTopRenderQueue;
    }

    private static void SetIntIfPresent(Material material, string propertyName, int value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetInt(propertyName, value);
        }
    }
}

public static class OfficeXrUiSupport
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeSceneHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ConfigureLoadedScene()
    {
        ConfigureInteractiveCanvasesInScene();
    }

    public static void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            eventSystem = Object.FindObjectOfType<EventSystem>();
        }

        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        XRUIInputModule xrInputModule = eventSystem.GetComponent<XRUIInputModule>();
        if (xrInputModule == null)
        {
            xrInputModule = eventSystem.gameObject.AddComponent<XRUIInputModule>();
        }

        xrInputModule.enableXRInput = true;
        xrInputModule.enableMouseInput = true;
        xrInputModule.enableTouchInput = true;
        xrInputModule.enableBuiltinActionsAsFallback = true;
        xrInputModule.enableGamepadInput = true;
        xrInputModule.enableJoystickInput = true;

        StandaloneInputModule standaloneInputModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (standaloneInputModule != null)
        {
            standaloneInputModule.enabled = false;
        }
    }

    public static void ConfigureCanvasForXr(Canvas canvas, bool forceRaycaster)
    {
        if (canvas == null)
        {
            return;
        }

        if (!forceRaycaster && !HasSelectable(canvas.transform))
        {
            return;
        }

        EnsureEventSystem();
        EnsureCanvasCamera(canvas);

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        TrackedDeviceGraphicRaycaster trackedRaycaster = canvas.GetComponent<TrackedDeviceGraphicRaycaster>();
        if (trackedRaycaster == null)
        {
            trackedRaycaster = canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        }

        trackedRaycaster.enabled = true;
        RefreshRayInteractors();
    }

    public static void ConfigureCanvasesIn(GameObject root, bool forceRaycaster)
    {
        if (root == null)
        {
            return;
        }

        Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
        for (int i = 0; i < canvases.Length; i++)
        {
            ConfigureCanvasForXr(canvases[i], forceRaycaster);
        }
    }

    public static void ConfigureInteractiveCanvasesInScene()
    {
        EnsureEventSystem();
        RefreshRayInteractors();

        Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
        for (int i = 0; i < canvases.Length; i++)
        {
            ConfigureCanvasForXr(canvases[i], false);
        }
    }

    public static void EnsureCanvasCamera(Canvas canvas)
    {
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null && canvas.worldCamera != mainCamera)
        {
            canvas.worldCamera = mainCamera;
        }
    }

    public static void RefreshRayInteractors()
    {
        XRRayInteractor[] rayInteractors = Object.FindObjectsOfType<XRRayInteractor>(true);
        int uiLayer = LayerMask.NameToLayer("UI");
        int uiMask = uiLayer >= 0 ? 1 << uiLayer : 0;

        for (int i = 0; i < rayInteractors.Length; i++)
        {
            XRRayInteractor rayInteractor = rayInteractors[i];
            if (rayInteractor == null)
            {
                continue;
            }

            rayInteractor.enableUIInteraction = true;
            if (uiMask != 0)
            {
                rayInteractor.raycastMask |= uiMask;
            }
        }
    }

    private static bool HasSelectable(Transform root)
    {
        return root != null && root.GetComponentInChildren<Selectable>(true) != null;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConfigureInteractiveCanvasesInScene();
    }
}
