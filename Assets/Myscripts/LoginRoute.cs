public sealed class LoginRoute
{
    public LoginRoute(string accountId, LoginUserRole role, string avatarName, string sceneName, string scenePath)
    {
        AccountId = accountId;
        Role = role;
        AvatarName = avatarName;
        SceneName = sceneName;
        ScenePath = scenePath;
    }

    public string AccountId { get; }
    public LoginUserRole Role { get; }
    public string AvatarName { get; }
    public string SceneName { get; }
    public string ScenePath { get; }
}
