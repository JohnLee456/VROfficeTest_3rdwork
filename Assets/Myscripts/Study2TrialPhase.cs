public static class Study2TrialPhaseInfo
{
    public const int Opening = 0;
    public const int Episode1 = 1;
    public const int Episode2 = 2;
    public const int Episode3 = 3;
    public const int Summary = 4;

    public const int FirstPhaseNumber = Opening;
    public const int LastPhaseNumber = Summary;
    public const int PhaseCount = 5;

    public static string GetLabel(int phaseNumber)
    {
        switch (phaseNumber)
        {
            case Opening:
                return "Opening Phase";
            case Episode1:
                return "Episode 1";
            case Episode2:
                return "Episode 2";
            case Episode3:
                return "Episode 3";
            case Summary:
                return "Summary Stage";
            default:
                return "Phase " + phaseNumber;
        }
    }

    public static string GetShortCode(int phaseNumber)
    {
        switch (phaseNumber)
        {
            case Opening:
                return "O";
            case Episode1:
                return "E1";
            case Episode2:
                return "E2";
            case Episode3:
                return "E3";
            case Summary:
                return "S";
            default:
                return "P" + phaseNumber;
        }
    }

    public static float GetStartSecond(int phaseNumber)
    {
        switch (phaseNumber)
        {
            case Opening:
                return 0f;
            case Episode1:
                return 40f;
            case Episode2:
                return 100f;
            case Episode3:
                return 170f;
            case Summary:
                return 240f;
            default:
                return 0f;
        }
    }

    public static float GetEndSecond(int phaseNumber)
    {
        switch (phaseNumber)
        {
            case Opening:
                return 40f;
            case Episode1:
                return 100f;
            case Episode2:
                return 170f;
            case Episode3:
                return 240f;
            case Summary:
                return 300f;
            default:
                return 0f;
        }
    }

    public static float GetDuration(int phaseNumber)
    {
        return GetEndSecond(phaseNumber) - GetStartSecond(phaseNumber);
    }
}
