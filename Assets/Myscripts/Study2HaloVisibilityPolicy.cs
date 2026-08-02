public static class Study2HaloVisibilityPolicy
{
    public static bool ShouldSuppressHaloForCurrentPhase()
    {
        int trialNumber;
        int episodeNumber;

        if (!TryGetCurrentOrReadyPhase(out trialNumber, out episodeNumber))
        {
            return false;
        }

        return (trialNumber == 2 || trialNumber == 3) &&
            episodeNumber >= Study2TrialPhaseInfo.Episode1 &&
            episodeNumber <= Study2TrialPhaseInfo.Episode3;
    }

    private static bool TryGetCurrentOrReadyPhase(out int trialNumber, out int episodeNumber)
    {
        double readyTime;
        bool hasReadyState = Block1EpisodeSync.TryReadPromptRoomState(out _, out trialNumber, out episodeNumber, out readyTime);

        int startedTrialNumber;
        int startedEpisodeNumber;
        double startedTime;
        bool hasStartedState = Block1EpisodeSync.TryReadRoomState(out _, out startedTrialNumber, out startedEpisodeNumber, out startedTime);

        if (hasReadyState && (!hasStartedState || readyTime > startedTime))
        {
            return true;
        }

        if (hasStartedState)
        {
            trialNumber = startedTrialNumber;
            episodeNumber = startedEpisodeNumber;
            return true;
        }

        return false;
    }
}
