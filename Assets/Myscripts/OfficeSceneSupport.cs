public static class OfficeSceneSupport
{
    public const string OfficeLoggedIn = "OfficeLoggedIn";
    public const string OfficeLoggedInNoBot = "OfficeLoggedInNoBot";

    public static bool IsSupported(string sceneName)
    {
        return sceneName == OfficeLoggedIn || sceneName == OfficeLoggedInNoBot;
    }

    public static bool ShouldShowRuntimeUi(string sceneName)
    {
        if (sceneName == OfficeLoggedIn)
        {
            return true;
        }

        if (sceneName == OfficeLoggedInNoBot)
        {
            return LoginSession.HasRoute && LoginSession.Role == LoginUserRole.Leader;
        }

        return false;
    }
}
