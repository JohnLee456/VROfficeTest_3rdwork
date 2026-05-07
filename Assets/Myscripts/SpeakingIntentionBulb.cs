using UnityEngine;

public class SpeakingIntentionBulb : MonoBehaviour
{
    [SerializeField] private SpeakingIntention speakingIntention;
    [SerializeField] private float onThreshold = 70f;
    [SerializeField] private Color offColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color onColor = new Color(1f, 0.86f, 0.18f, 1f);
    [SerializeField] private float onEmissionIntensity = 2.5f;

    private Renderer[] renderers;
    private Light[] lights;
    private bool isOn;
    private bool isVisibleForCurrentMode;

    private void Awake()
    {
        if (speakingIntention == null)
        {
            speakingIntention = GetComponentInParent<SpeakingIntention>();
        }

        renderers = GetComponentsInChildren<Renderer>(true);
        lights = GetComponentsInChildren<Light>(true);
        ApplyState(false, DiskSelectorController.IsBinaryHaloSelected);
    }

    private void Update()
    {
        bool shouldBeVisible = DiskSelectorController.IsBinaryHaloSelected;
        bool shouldBeOn = speakingIntention != null && speakingIntention.speaking_intention > onThreshold;
        if (shouldBeOn != isOn || shouldBeVisible != isVisibleForCurrentMode)
        {
            ApplyState(shouldBeOn, shouldBeVisible);
        }
    }

    private void ApplyState(bool turnOn, bool visible)
    {
        isOn = turnOn;
        isVisibleForCurrentMode = visible;
        Color baseColor = turnOn ? onColor : offColor;
        Color emissionColor = turnOn ? onColor * onEmissionIntensity : Color.black;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer targetRenderer = renderers[i];
            if (targetRenderer == null)
            {
                continue;
            }

            targetRenderer.enabled = visible;
            Material material = targetRenderer.material;
            material.color = baseColor;
            material.SetColor("_EmissionColor", emissionColor);

            if (turnOn)
            {
                material.EnableKeyword("_EMISSION");
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }
        }

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                lights[i].enabled = visible && turnOn;
            }
        }
    }
}
