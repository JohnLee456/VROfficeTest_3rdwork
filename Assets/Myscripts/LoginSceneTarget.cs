using UnityEngine;
using UnityEngine.SceneManagement;

public static class LoginSceneTarget
{
    private const string DefaultSceneName = "OfficeLoggedIn";

    public static string SceneName
    {
        get
        {
            if (LoginSession.HasRoute && !string.IsNullOrWhiteSpace(LoginSession.SceneName))
            {
                return LoginSession.SceneName;
            }

            return DefaultSceneName;
        }
    }

    public static void Load()
    {
        string sceneName = SceneName;
        Debug.Log($"Loading login target scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
