using UnityEngine;
using UnityEngine.SceneManagement;

public class OfficeLoggedInUiManagerBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureForCurrentScene()
    {
        EnsureManagers(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureManagers(scene);
    }

    private static void EnsureManagers(Scene scene)
    {
        if (!OfficeSceneSupport.ShouldShowRuntimeUi(scene.name))
        {
            return;
        }

        EnsureManager<GradedHaloDisplayManager>("Graded Halo Display Manager");
        EnsureManager<ProbabilityHaloDisplayManager>("Probability Halo Display Manager");
        EnsureManager<DirectionalPeripheralHaloDisplayManager>("Directional Peripheral Halo Display Manager");
        EnsureManager<RepeatAttemptDashboardManager>("Repeat Attempt Dashboard Manager");
        EnsureManager<TimelineDashboardManager>("Timeline Dashboard Manager");
        EnsureManager<ArousalDashboardManager>("Arousal Dashboard Manager");
    }

    private static void EnsureManager<T>(string objectName) where T : MonoBehaviour
    {
        if (Object.FindObjectOfType<T>() != null)
        {
            return;
        }

        GameObject manager = new GameObject(objectName);
        manager.AddComponent<T>();
    }
}
