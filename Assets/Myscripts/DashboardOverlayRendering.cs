using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public static class DashboardOverlayRendering
{
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
        material.renderQueue = (int)RenderQueue.Overlay;
    }

    private static void SetIntIfPresent(Material material, string propertyName, int value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetInt(propertyName, value);
        }
    }
}
