using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class SpeakingIntentionScenarioLoopController : MonoBehaviour
{
    private const string ControlledSceneName = "OfficeLoggedIn";
    private const float SegmentDuration = 30f;
    private const float TotalDuration = SegmentDuration * 3f;

    private SpeakingIntention dcy;
    private SpeakingIntention zjr;
    private SpeakingIntention zhz;
    private float loopStartTime;

    public static bool IsRunning { get; private set; }
    public static float CurrentLoopTime { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<SpeakingIntentionScenarioLoopController>() != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject("Speaking Intention Scenario Loop Controller");
        DontDestroyOnLoad(controllerObject);
        controllerObject.AddComponent<SpeakingIntentionScenarioLoopController>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        IsRunning = false;
    }

    private void Start()
    {
        if (!IsControlledScene(SceneManager.GetActiveScene()))
        {
            ClearTargets();
            return;
        }

        BindTargets(resetTimer: true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsControlledScene(scene))
        {
            ClearTargets();
            return;
        }

        BindTargets(resetTimer: true);
    }

    private void LateUpdate()
    {
        if (!IsControlledScene(SceneManager.GetActiveScene()))
        {
            ClearTargets();
            return;
        }

        if (!HasTargets())
        {
            BindTargets(resetTimer: true);
        }

        if (!HasTargets())
        {
            IsRunning = false;
            CurrentLoopTime = 0f;
            return;
        }

        IsRunning = true;
        ApplyScenarioValues();
    }

    private void BindTargets(bool resetTimer)
    {
        dcy = FindIntentionByObjectName("DCY");
        zjr = FindIntentionByObjectName("ZJR");
        zhz = FindIntentionByObjectName("ZHZ");

        foreach (ZJRSpeakingIntentionController legacyController in FindObjectsOfType<ZJRSpeakingIntentionController>(true))
        {
            legacyController.enabled = false;
        }

        if (resetTimer && HasTargets())
        {
            loopStartTime = Time.time;
            ApplyScenarioValues();
        }
    }

    private void ApplyScenarioValues()
    {
        float loopTime = Mathf.Repeat(Time.time - loopStartTime, TotalDuration);
        CurrentLoopTime = loopTime;
        float segmentTime = loopTime % SegmentDuration;

        float dcyValue;
        float zjrValue;
        float zhzValue;

        if (loopTime < SegmentDuration)
        {
            SpeakingIntentionScenarioOne.Evaluate(segmentTime, out dcyValue, out zjrValue, out zhzValue);
        }
        else if (loopTime < SegmentDuration * 2f)
        {
            SpeakingIntentionScenarioTwo.Evaluate(segmentTime, out dcyValue, out zjrValue, out zhzValue);
        }
        else
        {
            SpeakingIntentionScenarioThree.Evaluate(segmentTime, out dcyValue, out zjrValue, out zhzValue);
        }

        dcy.speaking_intention = Mathf.Clamp(dcyValue, 0f, 100f);
        zjr.speaking_intention = Mathf.Clamp(zjrValue, 0f, 100f);
        zhz.speaking_intention = Mathf.Clamp(zhzValue, 0f, 100f);
    }

    private void ClearTargets()
    {
        dcy = null;
        zjr = null;
        zhz = null;
        IsRunning = false;
        CurrentLoopTime = 0f;
    }

    private bool HasTargets()
    {
        return dcy != null && zjr != null && zhz != null;
    }

    private static bool IsControlledScene(Scene scene)
    {
        return scene.IsValid() && scene.name == ControlledSceneName;
    }

    private static SpeakingIntention FindIntentionByObjectName(string objectName)
    {
        SpeakingIntention[] intentions = FindObjectsOfType<SpeakingIntention>(true);
        foreach (SpeakingIntention intention in intentions)
        {
            if (IsSelfOrParentNamed(intention.transform, objectName))
            {
                return intention;
            }
        }

        return null;
    }

    private static bool IsSelfOrParentNamed(Transform transform, string objectName)
    {
        Transform current = transform;
        while (current != null)
        {
            if (current.name == objectName)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }
}
