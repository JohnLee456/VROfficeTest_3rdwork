using UnityEngine;

public static class SpeakingIntentionScenarioTwo
{
    public const float Duration = 30f;

    public static void Evaluate(float elapsedSeconds, out float dcy, out float zjr, out float zhz)
    {
        float t = Mathf.Clamp(elapsedSeconds, 0f, Duration);

        dcy = 0f;
        zjr = 0f;

        if (t < 12f)
        {
            zhz = 20f + t * 5f;
        }
        else if (t < 22f)
        {
            zhz = 80f;
        }
        else
        {
            zhz = Mathf.Min(100f, 80f + (t - 22f) * 4f);
        }
    }
}
