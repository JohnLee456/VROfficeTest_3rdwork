using UnityEngine;

public static class SpeakingIntentionScenarioOne
{
    public const float Duration = 30f;

    public static void Evaluate(float elapsedSeconds, out float dcy, out float zjr, out float zhz)
    {
        float t = Mathf.Clamp(elapsedSeconds, 0f, Duration);

        dcy = 0f;

        if (t >= Duration)
        {
            zjr = 0f;
        }
        else
        {
            zjr = Mathf.Min(100f, t * (10f / 3f));
            if (Mathf.Approximately(zjr, 100f))
            {
                zjr = 0f;
            }
        }

        if (t < 5f)
        {
            zhz = 20f + t * 10f;
        }
        else if (t < 15f)
        {
            zhz = 70f;
        }
        else
        {
            zhz = Mathf.Max(20f, 70f - (t - 15f) * 5f);
        }
    }
}
