using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public static class LoginSession
{
    public static bool HasRoute { get; private set; }
    public static string AccountId { get; private set; }
    public static LoginUserRole Role { get; private set; } = LoginUserRole.Unknown;
    public static string AvatarName { get; private set; }
    public static string SceneName { get; private set; }
    public static string ScenePath { get; private set; }

    public static void Apply(LoginRoute route)
    {
        HasRoute = route != null;
        AccountId = route != null ? route.AccountId : string.Empty;
        Role = route != null ? route.Role : LoginUserRole.Unknown;
        AvatarName = route != null ? route.AvatarName : string.Empty;
        SceneName = route != null ? route.SceneName : string.Empty;
        ScenePath = route != null ? route.ScenePath : string.Empty;

        if (!HasRoute)
        {
            return;
        }

        PhotonNetwork.NickName = AvatarName;
        PhotonNetwork.AuthValues = new AuthenticationValues(AccountId);

        if (PhotonNetwork.LocalPlayer != null)
        {
            PhotonNetwork.LocalPlayer.NickName = AvatarName;
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
            {
                { "accountId", AccountId },
                { "role", Role.ToString() },
                { "avatar", AvatarName }
            });
        }

        PlayerPrefs.SetString("LoginAccountId", AccountId);
        PlayerPrefs.SetString("LoginRole", Role.ToString());
        PlayerPrefs.SetString("LoginAvatarName", AvatarName);
        PlayerPrefs.SetString("LoginSceneName", SceneName);
        PlayerPrefs.Save();
    }
}
