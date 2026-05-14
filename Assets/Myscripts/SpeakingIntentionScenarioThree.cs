using UnityEngine;

public static class SpeakingIntentionScenarioThree
{
    public const float Duration = 30f;

    public static void Evaluate(float elapsedSeconds, out float dcy, out float zjr, out float zhz)
    {
        float t = Mathf.Clamp(elapsedSeconds, 0f, Duration);

        if (t < 20f)
        {
            dcy = t * 5f;
        }
        else
        {
            dcy = Mathf.Max(0f, 100f - (t - 20f) * 10f);
        }

        if (t < 15f)
        {
            zjr = t * 3f;
        }
        else if (t < 26f)
        {
            zjr = Mathf.Min(100f, 45f + (t - 15f) * 5f);
        }
        else if (t < 29f)
        {
            zjr = 100f;
        }
        else
        {
            zjr = 0f;
        }

        if (t < 15f)
        {
            zhz = 100f - t * 2f;
        }
        else
        {
            zhz = Mathf.Max(20f, 70f - (t - 15f) * 10f);
        }
    }
}
