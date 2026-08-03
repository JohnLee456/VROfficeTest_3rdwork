using System;
using UnityEngine;

public enum PhysioInterfaceMode
{
    RealtimeOrRich,
    SubtleRealtime,
    SubtleOrDelayed,
    SummaryOrFallback,
    SubtleOrSummary
}

public static class PhysioAwareFeedbackParser
{
    public const string ExpectedProtocol = "physio_aware_feedback_v1";

    public static bool TryParseResource(string resourcePath, out PhysioAwareFeedbackResult result)
    {
        result = PhysioAwareFeedbackResult.Invalid("Resource path is empty.");

        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return false;
        }

        string normalizedPath = NormalizeResourcePath(resourcePath);
        TextAsset textAsset = Resources.Load<TextAsset>(normalizedPath);
        if (textAsset == null)
        {
            result = PhysioAwareFeedbackResult.Invalid($"Resource JSON not found: {normalizedPath}");
            return false;
        }

        return TryParse(textAsset.text, out result);
    }

    public static bool TryParse(string json, out PhysioAwareFeedbackResult result)
    {
        result = PhysioAwareFeedbackResult.Invalid("JSON text is empty.");

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        PhysioAwareFeedbackPayload payload;
        try
        {
            payload = JsonUtility.FromJson<PhysioAwareFeedbackPayload>(json);
        }
        catch (Exception exception)
        {
            result = PhysioAwareFeedbackResult.Invalid($"JSON parse failed: {exception.Message}");
            return false;
        }

        if (payload == null)
        {
            result = PhysioAwareFeedbackResult.Invalid("JSON parse failed: payload is null.");
            return false;
        }

        if (!EqualsText(payload.protocol, ExpectedProtocol))
        {
            result = PhysioAwareFeedbackResult.Invalid($"Unsupported protocol: {payload.protocol}");
            return false;
        }

        if (payload.static_state == null)
        {
            result = PhysioAwareFeedbackResult.Invalid("Missing static_state.");
            return false;
        }

        if (payload.temporal_dynamics == null)
        {
            result = PhysioAwareFeedbackResult.Invalid("Missing temporal_dynamics.");
            return false;
        }

        if (payload.quality == null)
        {
            result = PhysioAwareFeedbackResult.Invalid("Missing quality.");
            return false;
        }

        result = BuildResult(payload);
        return true;
    }

    public static PhysioInterfaceMode ResolveInterfaceMode(string staticState, string trend, string qualityLevel)
    {
        if (EqualsText(qualityLevel, "very_low") || EqualsText(qualityLevel, "low"))
        {
            return PhysioInterfaceMode.SummaryOrFallback;
        }

        if (EqualsText(staticState, "Stress") || EqualsText(trend, "Pressure Increase"))
        {
            return PhysioInterfaceMode.SubtleOrDelayed;
        }

        if (EqualsText(trend, "Physiological Fluctuation"))
        {
            return PhysioInterfaceMode.SubtleRealtime;
        }

        bool calmOrNeutral = EqualsText(staticState, "Relaxed") || EqualsText(staticState, "Neutral");
        bool stableOrRelaxing = EqualsText(trend, "Stable Pattern") || EqualsText(trend, "Progressive Relaxation");
        if (calmOrNeutral && stableOrRelaxing)
        {
            return PhysioInterfaceMode.RealtimeOrRich;
        }

        return PhysioInterfaceMode.SubtleOrSummary;
    }

    public static string GetInterfaceModeKey(PhysioInterfaceMode mode)
    {
        switch (mode)
        {
            case PhysioInterfaceMode.RealtimeOrRich:
                return "realtime_or_rich";
            case PhysioInterfaceMode.SubtleRealtime:
                return "subtle_realtime";
            case PhysioInterfaceMode.SubtleOrDelayed:
                return "subtle_or_delayed";
            case PhysioInterfaceMode.SummaryOrFallback:
                return "summary_or_fallback";
            case PhysioInterfaceMode.SubtleOrSummary:
                return "subtle_or_summary";
            default:
                return "subtle_or_summary";
        }
    }

    public static string GetInterfaceCode(PhysioInterfaceMode mode)
    {
        switch (mode)
        {
            case PhysioInterfaceMode.RealtimeOrRich:
                return "A2";
            case PhysioInterfaceMode.SubtleRealtime:
                return "A4";
            case PhysioInterfaceMode.SubtleOrDelayed:
                return "A1";
            case PhysioInterfaceMode.SummaryOrFallback:
                return "D1";
            case PhysioInterfaceMode.SubtleOrSummary:
                return "D2";
            default:
                return "D2";
        }
    }

    private static PhysioAwareFeedbackResult BuildResult(PhysioAwareFeedbackPayload payload)
    {
        PhysioStaticState staticState = payload.static_state;
        PhysioTemporalDynamics dynamics = payload.temporal_dynamics;
        PhysioQuality quality = payload.quality;
        PhysioMetrics metrics = payload.metrics;
        PhysioRecommendedFeedback recommendation = payload.recommended_feedback;

        PhysioInterfaceMode mode = ResolveInterfaceMode(staticState.final_simple, dynamics.trend, quality.level);

        return new PhysioAwareFeedbackResult
        {
            IsValid = true,
            Error = string.Empty,
            RawPayload = payload,

            Protocol = Safe(payload.protocol),
            Source = Safe(payload.source),
            TimestampUnix = payload.timestamp_unix,
            TimeString = Safe(payload.time_str),
            ParticipantRole = Safe(payload.participant_role),

            StaticStateDetailed = Safe(staticState.final),
            StaticState = Safe(staticState.final_simple),
            AbsoluteState = Safe(staticState.absolute),
            BaselineShift = Safe(staticState.baseline_shift),
            BaselineReady = staticState.baseline_ready,
            HrZ = staticState.z != null ? staticState.z.hr : 0f,
            RmssdZ = staticState.z != null ? staticState.z.rmssd : 0f,
            SdnnZ = staticState.z != null ? staticState.z.sdnn : 0f,

            Trend = Safe(dynamics.trend),
            HrNormChange = dynamics.hr_norm_change,
            HrvNormChange = dynamics.hrv_norm_change,
            CoOccurrence = Safe(payload.co_occurrence),

            HrBpm = metrics != null ? metrics.hr_bpm : 0f,
            RmssdMs = metrics != null ? metrics.rmssd_ms : 0f,
            SdnnMs = metrics != null ? metrics.sdnn_ms : 0f,
            Pnn50Percent = metrics != null ? metrics.pnn50_percent : 0f,

            QualityLevel = Safe(quality.level),
            HoldUsed = quality.hold_used,
            StrictUsable = quality.strict != null && quality.strict.usable,
            StrictRawCount = quality.strict != null ? quality.strict.raw_count : 0,
            StrictWindowSec = quality.strict != null ? quality.strict.window_sec : 0f,
            StrictFraction = quality.strict != null ? quality.strict.fraction : 0f,
            SoftUsable = quality.soft != null && quality.soft.usable,
            SoftRawCount = quality.soft != null ? quality.soft.raw_count : 0,
            SoftWindowSec = quality.soft != null ? quality.soft.window_sec : 0f,
            SoftFraction = quality.soft != null ? quality.soft.fraction : 0f,

            RecommendedDisplayMode = recommendation != null ? Safe(recommendation.display_mode) : string.Empty,
            RecommendedInterfaceStrength = recommendation != null ? Safe(recommendation.interface_strength) : string.Empty,
            RecommendedReason = recommendation != null ? Safe(recommendation.reason) : string.Empty,

            InterfaceMode = mode,
            InterfaceModeKey = GetInterfaceModeKey(mode),
            InterfaceCode = GetInterfaceCode(mode)
        };
    }

    private static string NormalizeResourcePath(string resourcePath)
    {
        string path = resourcePath.Replace('\\', '/').Trim();
        const string resourcePrefix = "Assets/Resources/";

        if (path.StartsWith(resourcePrefix, StringComparison.OrdinalIgnoreCase))
        {
            path = path.Substring(resourcePrefix.Length);
        }

        if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            path = path.Substring(0, path.Length - ".json".Length);
        }

        return path;
    }

    private static bool EqualsText(string left, string right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }

    private static string Safe(string value)
    {
        return value ?? string.Empty;
    }
}

[Serializable]
public sealed class PhysioAwareFeedbackResult
{
    public bool IsValid;
    public string Error;
    public PhysioAwareFeedbackPayload RawPayload;

    public string Protocol;
    public string Source;
    public double TimestampUnix;
    public string TimeString;
    public string ParticipantRole;

    public string StaticStateDetailed;
    public string StaticState;
    public string AbsoluteState;
    public string BaselineShift;
    public bool BaselineReady;
    public float HrZ;
    public float RmssdZ;
    public float SdnnZ;

    public string Trend;
    public float HrNormChange;
    public float HrvNormChange;
    public string CoOccurrence;

    public float HrBpm;
    public float RmssdMs;
    public float SdnnMs;
    public float Pnn50Percent;

    public string QualityLevel;
    public bool HoldUsed;
    public bool StrictUsable;
    public int StrictRawCount;
    public float StrictWindowSec;
    public float StrictFraction;
    public bool SoftUsable;
    public int SoftRawCount;
    public float SoftWindowSec;
    public float SoftFraction;

    public string RecommendedDisplayMode;
    public string RecommendedInterfaceStrength;
    public string RecommendedReason;

    public PhysioInterfaceMode InterfaceMode;
    public string InterfaceModeKey;
    public string InterfaceCode;

    public static PhysioAwareFeedbackResult Invalid(string error)
    {
        return new PhysioAwareFeedbackResult
        {
            IsValid = false,
            Error = error ?? string.Empty
        };
    }
}

[Serializable]
public sealed class PhysioAwareFeedbackPayload
{
    public string protocol;
    public string source;
    public double timestamp_unix;
    public string time_str;
    public string participant_role;
    public PhysioStaticState static_state;
    public PhysioTemporalDynamics temporal_dynamics;
    public string co_occurrence;
    public PhysioMetrics metrics;
    public PhysioQuality quality;
    public PhysioRecommendedFeedback recommended_feedback;
}

[Serializable]
public sealed class PhysioStaticState
{
    public string final;
    public string final_simple;
    public string absolute;
    public string baseline_shift;
    public bool baseline_ready;
    public PhysioZScores z;
}

[Serializable]
public sealed class PhysioZScores
{
    public float hr;
    public float rmssd;
    public float sdnn;
}

[Serializable]
public sealed class PhysioTemporalDynamics
{
    public string trend;
    public float hr_norm_change;
    public float hrv_norm_change;
}

[Serializable]
public sealed class PhysioMetrics
{
    public float hr_bpm;
    public float rmssd_ms;
    public float sdnn_ms;
    public float pnn50_percent;
}

[Serializable]
public sealed class PhysioQuality
{
    public string level;
    public bool hold_used;
    public PhysioQualityWindow strict;
    public PhysioQualityWindow soft;
}

[Serializable]
public sealed class PhysioQualityWindow
{
    public bool usable;
    public bool enough_ppi;
    public bool range_ok;
    public bool stability_ok;
    public int raw_count;
    public int clean_count;
    public float window_sec;
    public float fraction;
}

[Serializable]
public sealed class PhysioRecommendedFeedback
{
    public string display_mode;
    public string interface_strength;
    public string reason;
}
