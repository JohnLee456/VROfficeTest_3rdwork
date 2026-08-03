using UnityEngine;

public static class Study2Trial3PhysioFeedbackTestController
{
    private const int TargetTrialNumber = 3;
    private const string BinaryHaloResourcePath = "test/BinaryHalo";
    private const string GradedHaloResourcePath = "test/GradeHalo";
    private const string DirectionalPeripheralHaloResourcePath = "test/DirectionalPeripheralHalo";
    private const string TimelineDashboardResourcePath = "test/TimelineDashboard";

    private static bool feedbackOverrideActive;
    private static int activeBlockNumber = -1;
    private static int activePhaseNumber = -1;
    private static string activeResourcePath = string.Empty;
    private static string activeUiOption = string.Empty;

    public static bool IsFeedbackOverrideActive => feedbackOverrideActive;
    public static int ActiveBlockNumber => activeBlockNumber;
    public static int ActivePhaseNumber => activePhaseNumber;
    public static string ActiveResourcePath => activeResourcePath;
    public static string ActiveUiOption => activeUiOption;
    public static PhysioAwareFeedbackResult LastResult { get; private set; }

    public static void ApplyFeedbackForNextPhase(int blockNumber, int trialNumber, int nextPhaseNumber)
    {
        if (trialNumber != TargetTrialNumber)
        {
            ClearFeedbackOverride();
            return;
        }

        string resourcePath = GetResourcePathForPhase(nextPhaseNumber);
        if (string.IsNullOrEmpty(resourcePath))
        {
            return;
        }

        PhysioAwareFeedbackResult result;
        if (!PhysioAwareFeedbackParser.TryParseResource(resourcePath, out result))
        {
            ClearFeedbackOverride();
            Debug.LogWarning("Trial3 physio feedback test failed to parse " + resourcePath + ": " + result.Error);
            return;
        }

        string uiOption = GetUiOptionForPhase(nextPhaseNumber, result);
        if (string.IsNullOrEmpty(uiOption))
        {
            ClearFeedbackOverride();
            Debug.LogWarning("Trial3 physio feedback test did not map " + result.InterfaceCode + " to a UI option.");
            return;
        }

        feedbackOverrideActive = true;
        activeBlockNumber = blockNumber;
        activePhaseNumber = nextPhaseNumber;
        activeResourcePath = resourcePath;
        activeUiOption = uiOption;
        LastResult = result;

        DiskSelectorController.SetSelectionOverride(uiOption);
        Debug.Log(
            "Trial3 physio feedback test applied: Block " + blockNumber +
            ", " + Study2TrialPhaseInfo.GetLabel(nextPhaseNumber) +
            ", resource " + resourcePath +
            ", mode " + result.InterfaceModeKey +
            ", code " + result.InterfaceCode +
            ", UI " + uiOption + ".");
    }

    public static void ClearFeedbackOverride()
    {
        if (!feedbackOverrideActive && !DiskSelectorController.HasSelectionOverride)
        {
            return;
        }

        feedbackOverrideActive = false;
        activeBlockNumber = -1;
        activePhaseNumber = -1;
        activeResourcePath = string.Empty;
        activeUiOption = string.Empty;
        LastResult = null;
        DiskSelectorController.ClearSelectionOverride();
    }

    public static bool ShouldAllowHaloForTrial3Feedback(int trialNumber, int phaseNumber)
    {
        return feedbackOverrideActive &&
            trialNumber == TargetTrialNumber &&
            phaseNumber >= Study2TrialPhaseInfo.Episode1 &&
            phaseNumber <= Study2TrialPhaseInfo.Episode3 &&
            (activeUiOption == DiskSelectorController.BinaryHaloOption ||
             activeUiOption == DiskSelectorController.GradedHaloOption ||
             activeUiOption == DiskSelectorController.DirectionalPeripheralHaloOption);
    }

    public static bool ShouldForceTimelineDashboardForTrial3Summary()
    {
        return IsTimelineSummaryFeedbackActive() && HasActivePhaseStarted();
    }

    public static bool ShouldHoldTimelineDashboardUntilSummaryStart()
    {
        return IsTimelineSummaryFeedbackActive() && !HasActivePhaseStarted();
    }

    private static bool IsTimelineSummaryFeedbackActive()
    {
        return feedbackOverrideActive &&
            activePhaseNumber == Study2TrialPhaseInfo.Summary &&
            activeUiOption == DiskSelectorController.TimelineDashboardOption;
    }

    private static bool HasActivePhaseStarted()
    {
        int blockNumber;
        int trialNumber;
        int phaseNumber;
        double startTime;
        if (!Block1EpisodeSync.TryReadRoomState(out blockNumber, out trialNumber, out phaseNumber, out startTime))
        {
            return false;
        }

        return blockNumber == activeBlockNumber &&
            trialNumber == TargetTrialNumber &&
            phaseNumber == activePhaseNumber;
    }

    private static string GetResourcePathForPhase(int phaseNumber)
    {
        switch (phaseNumber)
        {
            case Study2TrialPhaseInfo.Episode1:
                return BinaryHaloResourcePath;
            case Study2TrialPhaseInfo.Episode2:
                return GradedHaloResourcePath;
            case Study2TrialPhaseInfo.Episode3:
                return DirectionalPeripheralHaloResourcePath;
            case Study2TrialPhaseInfo.Summary:
                return TimelineDashboardResourcePath;
            default:
                return string.Empty;
        }
    }

    private static string GetUiOptionForPhase(int phaseNumber, PhysioAwareFeedbackResult result)
    {
        if (phaseNumber == Study2TrialPhaseInfo.Summary)
        {
            return DiskSelectorController.TimelineDashboardOption;
        }

        switch (result.InterfaceCode)
        {
            case "A1":
                return DiskSelectorController.BinaryHaloOption;
            case "A2":
                return DiskSelectorController.GradedHaloOption;
            case "A4":
                return DiskSelectorController.DirectionalPeripheralHaloOption;
            default:
                return string.Empty;
        }
    }
}
