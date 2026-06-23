using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-88)]
public class Block3SpeakingIntentionController : MonoBehaviour
{
    private const string DefaultControlledSceneName = "OfficeLoggedInNoBot";
    private const float BackgroundProbability = 20f;

    [SerializeField] private string controlledSceneName = DefaultControlledSceneName;
    [SerializeField] private bool autoStartCueAtDocumentWindow = true;
    [SerializeField] private bool resetValuesWhenEpisodeEnds = true;

    private SpeakingIntention zhz;
    private SpeakingIntention dcy;
    private SpeakingIntention zjr;

    private readonly Dictionary<int, EpisodeSchedule[]> trials = new Dictionary<int, EpisodeSchedule[]>();
    private EpisodeSchedule currentEpisode;
    private int currentTrialNumber;
    private int currentEpisodeNumber;
    private float episodeElapsedSeconds;
    private bool trialRunning;
    private bool episodeRunning;
    private bool cueRunning;
    private bool manualCueActive;
    private bool paused;

    public int CurrentTrialNumber => currentTrialNumber;
    public int CurrentEpisodeNumber => currentEpisodeNumber;
    public bool IsEpisodeRunning => episodeRunning;
    public int EpisodeCount => trialRunning && trials.ContainsKey(currentTrialNumber) ? trials[currentTrialNumber].Length : 3;
    public float EpisodeRemainingSeconds => episodeRunning && currentEpisode != null ? Mathf.Max(0f, currentEpisode.EndSecond - (currentEpisode.StartSecond + episodeElapsedSeconds)) : 0f;

    public string GetEpisodeTitle(int trialNumber, int episodeNumber)
    {
        if (!trials.ContainsKey(trialNumber))
        {
            return string.Empty;
        }

        EpisodeSchedule[] episodes = trials[trialNumber];
        if (episodeNumber < 1 || episodeNumber > episodes.Length)
        {
            return string.Empty;
        }

        return "Episode " + episodeNumber + ": " + episodes[episodeNumber - 1].Name;
    }

    private void Awake()
    {
        BuildBlock3Schedules();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (IsControlledScene(SceneManager.GetActiveScene()))
        {
            BindTargets();
            ApplyBackgroundValues();
        }
    }

    private void Update()
    {
        if (!IsControlledScene(SceneManager.GetActiveScene()))
        {
            ClearRuntimeState();
            return;
        }

        if (!HasTargets())
        {
            BindTargets();
        }

        if (!episodeRunning || paused || currentEpisode == null || !HasTargets())
        {
            return;
        }

        episodeElapsedSeconds += Time.deltaTime;
        float absoluteTrialTime = currentEpisode.StartSecond + episodeElapsedSeconds;
        bool cueShouldRun = manualCueActive || (autoStartCueAtDocumentWindow && currentEpisode.IsInCueWindow(absoluteTrialTime));

        if (cueShouldRun && !cueRunning)
        {
            BeginCue("WindowStarted");
        }
        else if (!cueShouldRun && cueRunning)
        {
            EndCueInternal("WindowEnded");
        }

        ApplyEpisodeValues(absoluteTrialTime, manualCueActive);
    }

    public void StartTrial(int trialNumber)
    {
        if (!CanRunHere())
        {
            return;
        }

        if (!trials.ContainsKey(trialNumber))
        {
            Debug.LogWarning("Block3 trial " + trialNumber + " is not configured.");
            return;
        }

        BindTargets();
        currentTrialNumber = trialNumber;
        currentEpisodeNumber = 0;
        currentEpisode = null;
        trialRunning = true;
        episodeRunning = false;
        cueRunning = false;
        manualCueActive = false;
        paused = false;
        ApplyBackgroundValues();
        Debug.Log("Block3 Silence Context started: Trial " + currentTrialNumber);
    }

    public void StartEpisode(int episodeNumber)
    {
        if (!CanRunHere())
        {
            return;
        }

        if (!trialRunning)
        {
            StartTrial(1);
        }

        EpisodeSchedule[] episodes = trials[currentTrialNumber];
        if (episodeNumber < 1 || episodeNumber > episodes.Length)
        {
            Debug.LogWarning("Block3 episode " + episodeNumber + " is not configured.");
            return;
        }

        if (episodeRunning)
        {
            EndEpisode();
        }

        currentEpisodeNumber = episodeNumber;
        currentEpisode = episodes[episodeNumber - 1];
        episodeElapsedSeconds = 0f;
        episodeRunning = true;
        cueRunning = false;
        manualCueActive = false;
        paused = false;

        ApplyEpisodeValues(currentEpisode.StartSecond, false);
        Debug.Log("Block3 Trial " + currentTrialNumber + " Episode " + currentEpisodeNumber + " started: " + currentEpisode.Name);
    }

    public void EndEpisode()
    {
        if (!episodeRunning)
        {
            return;
        }

        if (cueRunning)
        {
            EndCueInternal("EpisodeEnded");
        }

        Debug.Log("Block3 Trial " + currentTrialNumber + " Episode " + currentEpisodeNumber + " ended at " + episodeElapsedSeconds.ToString("0.0") + "s.");
        episodeRunning = false;
        manualCueActive = false;
        currentEpisode = null;

        if (resetValuesWhenEpisodeEnds)
        {
            ApplyBackgroundValues();
        }
    }

    public void Pause()
    {
        paused = true;
    }

    public void Resume()
    {
        paused = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsControlledScene(scene))
        {
            ClearRuntimeState();
            return;
        }

        BindTargets();
        ApplyBackgroundValues();
    }

    private bool CanRunHere()
    {
        if (!IsControlledScene(SceneManager.GetActiveScene()))
        {
            Debug.LogWarning("Block3SpeakingIntentionController only runs in " + controlledSceneName + ".");
            return false;
        }

        BindTargets();
        if (!HasTargets())
        {
            Debug.LogWarning("Block3SpeakingIntentionController could not find ZHZ, DCY, and ZJR SpeakingIntention targets.");
            return false;
        }

        return true;
    }

    private void BindTargets()
    {
        zhz = FindIntentionByObjectName("ZHZ");
        dcy = FindIntentionByObjectName("DCY");
        zjr = FindIntentionByObjectName("ZJR");

        foreach (ZJRSpeakingIntentionController legacyController in FindObjectsOfType<ZJRSpeakingIntentionController>(true))
        {
            legacyController.enabled = false;
        }
    }

    private bool HasTargets()
    {
        return zhz != null && dcy != null && zjr != null;
    }

    private bool IsControlledScene(Scene scene)
    {
        return scene.IsValid() && scene.name == controlledSceneName;
    }

    private void BeginCue(string reason)
    {
        cueRunning = true;
        Debug.Log("Block3 cue started: Trial " + currentTrialNumber + ", Episode " + currentEpisodeNumber + ", target " + currentEpisode.PrimaryTarget + ", reason " + reason + ".");
    }

    private void EndCueInternal(string reason)
    {
        if (!cueRunning)
        {
            return;
        }

        cueRunning = false;
        Debug.Log("Block3 cue ended: Trial " + currentTrialNumber + ", Episode " + currentEpisodeNumber + ", reason " + reason + ".");
    }

    private void ApplyEpisodeValues(float absoluteTrialTime, bool forceCuePeak)
    {
        if (currentEpisode == null || !HasTargets())
        {
            return;
        }

        zhz.speaking_intention = Mathf.Clamp(GetMemberValue(currentEpisode.Zhz, absoluteTrialTime, forceCuePeak), 0f, 100f);
        dcy.speaking_intention = Mathf.Clamp(GetMemberValue(currentEpisode.Dcy, absoluteTrialTime, forceCuePeak), 0f, 100f);
        zjr.speaking_intention = Mathf.Clamp(GetMemberValue(currentEpisode.Zjr, absoluteTrialTime, forceCuePeak), 0f, 100f);
    }

    private static float GetMemberValue(MemberSchedule schedule, float absoluteTrialTime, bool forceCuePeak)
    {
        if (forceCuePeak && schedule.CuePeakValue > 0f)
        {
            return schedule.CuePeakValue;
        }

        for (int i = 0; i < schedule.Segments.Length; i++)
        {
            ProbabilitySegment segment = schedule.Segments[i];
            if (segment.Contains(absoluteTrialTime))
            {
                return segment.Evaluate(absoluteTrialTime);
            }
        }

        return schedule.BackgroundValue;
    }

    private void ApplyBackgroundValues()
    {
        if (!HasTargets())
        {
            return;
        }

        zhz.speaking_intention = BackgroundProbability;
        dcy.speaking_intention = BackgroundProbability;
        zjr.speaking_intention = BackgroundProbability;
    }

    private void ClearRuntimeState()
    {
        zhz = null;
        dcy = null;
        zjr = null;
        trialRunning = false;
        episodeRunning = false;
        cueRunning = false;
        manualCueActive = false;
        paused = false;
        currentEpisode = null;
    }

    private void BuildBlock3Schedules()
    {
        trials.Clear();
        trials.Add(1, new[]
        {
            new EpisodeSchedule("Clear First Restart After Silence", 40f, 100f, 58f, 63f, "ZHZ", "", SingleRestart(54f, 58f, 58f, 63f, 63f, 66f, 88f), MemberSchedule.Background(), MemberSchedule.Background()),
            new EpisodeSchedule("Competing Restart Attempts", 100f, 170f, 128f, 133f, "DCY", "ZJR", MemberSchedule.Background(), PrimaryCompetition(), SecondaryCompetition()),
            new EpisodeSchedule("Repeated Restart Event", 170f, 240f, 200f, 205f, "ZJR", "", MemberSchedule.Background(), MemberSchedule.Background(), SingleRestart(196f, 200f, 200f, 205f, 205f, 208f, 86f))
        });

        trials.Add(2, new[]
        {
            new EpisodeSchedule("Clear First Restart After Silence", 40f, 100f, 58f, 63f, "ZJR", "", MemberSchedule.Background(), MemberSchedule.Background(), SingleRestart(54f, 58f, 58f, 63f, 63f, 66f, 88f)),
            new EpisodeSchedule("Competing Restart Attempts", 100f, 170f, 128f, 133f, "ZHZ", "DCY", PrimaryCompetition(), SecondaryCompetition(), MemberSchedule.Background()),
            new EpisodeSchedule("Repeated Restart Event", 170f, 240f, 200f, 205f, "DCY", "", MemberSchedule.Background(), SingleRestart(196f, 200f, 200f, 205f, 205f, 208f, 86f), MemberSchedule.Background())
        });
    }

    private static MemberSchedule SingleRestart(float riseStart, float riseEnd, float cueStart, float cueEnd, float fallStart, float fallEnd, float peak)
    {
        return MemberSchedule.WithSegments(peak, new[]
        {
            ProbabilitySegment.Linear(riseStart, riseEnd, 20f, 74f),
            ProbabilitySegment.Peaked(cueStart, cueEnd, 74f, peak, 74f),
            ProbabilitySegment.Linear(fallStart, fallEnd, 74f, 20f)
        });
    }

    private static MemberSchedule PrimaryCompetition()
    {
        return MemberSchedule.WithSegments(92f, new[]
        {
            ProbabilitySegment.Linear(124f, 128f, 25f, 78f),
            ProbabilitySegment.Peaked(128f, 133f, 78f, 92f, 78f),
            ProbabilitySegment.Linear(133f, 136f, 78f, 25f)
        });
    }

    private static MemberSchedule SecondaryCompetition()
    {
        return MemberSchedule.WithSegments(80f, new[]
        {
            ProbabilitySegment.Linear(125f, 128f, 20f, 72f),
            ProbabilitySegment.Peaked(128f, 133f, 72f, 80f, 71f),
            ProbabilitySegment.Linear(133f, 136f, 71f, 20f)
        });
    }

    private static SpeakingIntention FindIntentionByObjectName(string objectName)
    {
        SpeakingIntention[] intentions = FindObjectsOfType<SpeakingIntention>(true);
        for (int i = 0; i < intentions.Length; i++)
        {
            if (IsSelfOrParentNamed(intentions[i].transform, objectName))
            {
                return intentions[i];
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

    private sealed class EpisodeSchedule
    {
        public readonly string Name;
        public readonly float StartSecond;
        public readonly float EndSecond;
        public readonly float CueStartSecond;
        public readonly float CueEndSecond;
        public readonly string PrimaryTarget;
        public readonly string SecondaryCandidate;
        public readonly MemberSchedule Zhz;
        public readonly MemberSchedule Dcy;
        public readonly MemberSchedule Zjr;

        public EpisodeSchedule(string name, float startSecond, float endSecond, float cueStartSecond, float cueEndSecond, string primaryTarget, string secondaryCandidate, MemberSchedule zhz, MemberSchedule dcy, MemberSchedule zjr)
        {
            Name = name;
            StartSecond = startSecond;
            EndSecond = endSecond;
            CueStartSecond = cueStartSecond;
            CueEndSecond = cueEndSecond;
            PrimaryTarget = primaryTarget;
            SecondaryCandidate = secondaryCandidate;
            Zhz = zhz;
            Dcy = dcy;
            Zjr = zjr;
        }

        public bool IsInCueWindow(float absoluteTrialTime)
        {
            return absoluteTrialTime >= CueStartSecond && absoluteTrialTime < CueEndSecond;
        }
    }

    private readonly struct MemberSchedule
    {
        public readonly float BackgroundValue;
        public readonly float CuePeakValue;
        public readonly ProbabilitySegment[] Segments;

        private MemberSchedule(float backgroundValue, float cuePeakValue, ProbabilitySegment[] segments)
        {
            BackgroundValue = backgroundValue;
            CuePeakValue = cuePeakValue;
            Segments = segments;
        }

        public static MemberSchedule Background()
        {
            return new MemberSchedule(BackgroundProbability, 0f, new ProbabilitySegment[0]);
        }

        public static MemberSchedule WithSegments(float cuePeakValue, ProbabilitySegment[] segments)
        {
            return new MemberSchedule(BackgroundProbability, cuePeakValue, segments);
        }
    }

    private readonly struct ProbabilitySegment
    {
        private readonly float startSecond;
        private readonly float endSecond;
        private readonly float startValue;
        private readonly float endValue;
        private readonly float peakValue;
        private readonly bool hasPeak;

        private ProbabilitySegment(float startSecond, float endSecond, float startValue, float peakValue, float endValue, bool hasPeak)
        {
            this.startSecond = startSecond;
            this.endSecond = endSecond;
            this.startValue = startValue;
            this.peakValue = peakValue;
            this.endValue = endValue;
            this.hasPeak = hasPeak;
        }

        public static ProbabilitySegment Linear(float startSecond, float endSecond, float startValue, float endValue)
        {
            return new ProbabilitySegment(startSecond, endSecond, startValue, 0f, endValue, false);
        }

        public static ProbabilitySegment Peaked(float startSecond, float endSecond, float startValue, float peakValue, float endValue)
        {
            return new ProbabilitySegment(startSecond, endSecond, startValue, peakValue, endValue, true);
        }

        public bool Contains(float absoluteTrialTime)
        {
            return absoluteTrialTime >= startSecond && absoluteTrialTime <= endSecond;
        }

        public float Evaluate(float absoluteTrialTime)
        {
            if (!hasPeak)
            {
                return Mathf.Lerp(startValue, endValue, Mathf.InverseLerp(startSecond, endSecond, absoluteTrialTime));
            }

            float middleSecond = (startSecond + endSecond) * 0.5f;
            if (absoluteTrialTime <= middleSecond)
            {
                return Mathf.Lerp(startValue, peakValue, Mathf.InverseLerp(startSecond, middleSecond, absoluteTrialTime));
            }

            return Mathf.Lerp(peakValue, endValue, Mathf.InverseLerp(middleSecond, endSecond, absoluteTrialTime));
        }
    }
}
