using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-90)]
public class Block1SpeakingIntentionController : MonoBehaviour
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
    public bool IsTrialRunning => trialRunning;
    public bool IsEpisodeRunning => episodeRunning;
    public bool IsPaused => paused;
    public int EpisodeCount => trialRunning && trials.ContainsKey(currentTrialNumber) ? trials[currentTrialNumber].Length : 3;
    public float EpisodeElapsedSeconds => episodeElapsedSeconds;
    public float EpisodeRemainingSeconds => episodeRunning && currentEpisode != null ? Mathf.Max(0f, currentEpisode.EndSecond - (currentEpisode.StartSecond + episodeElapsedSeconds)) : 0f;
    public string CurrentEpisodeTitle => currentEpisode != null ? "Episode " + currentEpisodeNumber + ": " + currentEpisode.Name : string.Empty;

    public string CurrentStatus
    {
        get
        {
            if (!IsControlledScene(SceneManager.GetActiveScene()))
            {
                return "Inactive scene";
            }

            if (!trialRunning)
            {
                return "Block1 ready";
            }

            if (!episodeRunning)
            {
                return "Trial " + currentTrialNumber + " ready";
            }

            return "Trial " + currentTrialNumber + " / Episode " + currentEpisodeNumber + " / " + Mathf.FloorToInt(episodeElapsedSeconds) + "s";
        }
    }

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
        BuildBlock1Schedules();
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

    public void StartTrial1()
    {
        StartTrial(1);
    }

    public void StartTrial2()
    {
        StartTrial(2);
    }

    public void StartTrial(int trialNumber)
    {
        if (!CanRunHere())
        {
            return;
        }

        if (!trials.ContainsKey(trialNumber))
        {
            Debug.LogWarning("Block1 trial " + trialNumber + " is not configured.");
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
        Debug.Log("Block1 Speaking Context started: Trial " + currentTrialNumber);
    }

    public void StartEpisode1()
    {
        StartEpisode(1);
    }

    public void StartEpisode2()
    {
        StartEpisode(2);
    }

    public void StartEpisode3()
    {
        StartEpisode(3);
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
            Debug.LogWarning("Block1 episode " + episodeNumber + " is not configured.");
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
        Debug.Log("Block1 Trial " + currentTrialNumber + " Episode " + currentEpisodeNumber + " started: " + currentEpisode.Name);
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

        Debug.Log("Block1 Trial " + currentTrialNumber + " Episode " + currentEpisodeNumber + " ended at " + episodeElapsedSeconds.ToString("0.0") + "s.");
        episodeRunning = false;
        manualCueActive = false;
        currentEpisode = null;

        if (resetValuesWhenEpisodeEnds)
        {
            ApplyBackgroundValues();
        }
    }

    public void StartCue()
    {
        if (!episodeRunning || currentEpisode == null)
        {
            return;
        }

        manualCueActive = true;
        if (!cueRunning)
        {
            BeginCue("ManualStarted");
        }

        ApplyEpisodeValues(currentEpisode.StartSecond + episodeElapsedSeconds, true);
    }

    public void EndCue()
    {
        if (!cueRunning && !manualCueActive)
        {
            return;
        }

        manualCueActive = false;
        EndCueInternal("ManualEnded");
    }

    public void Pause()
    {
        paused = true;
    }

    public void Resume()
    {
        paused = false;
    }

    public void InviteZHZ()
    {
        InviteMember("ZHZ");
    }

    public void InviteDCY()
    {
        InviteMember("DCY");
    }

    public void InviteZJR()
    {
        InviteMember("ZJR");
    }

    public void InviteMember(string memberName)
    {
        if (!episodeRunning || currentEpisode == null)
        {
            return;
        }

        string result = "TargetMismatch";
        if (memberName == currentEpisode.PrimaryTarget)
        {
            result = "TargetMatch";
            if (cueRunning)
            {
                manualCueActive = false;
                EndCueInternal("InvitedTarget");
            }
        }
        else if (!string.IsNullOrEmpty(currentEpisode.SecondaryCandidate) && memberName == currentEpisode.SecondaryCandidate)
        {
            result = "SecondaryCandidateInvited";
        }

        float absoluteTrialTime = currentEpisode.StartSecond + episodeElapsedSeconds;
        Debug.Log("Block1 invite recorded: " + memberName + " at trial " + absoluteTrialTime.ToString("0.0") + "s, " + result + ".");
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
            Debug.LogWarning("Block1SpeakingIntentionController only runs in " + controlledSceneName + ".");
            return false;
        }

        BindTargets();
        if (!HasTargets())
        {
            Debug.LogWarning("Block1SpeakingIntentionController could not find ZHZ, DCY, and ZJR SpeakingIntention targets.");
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
        Debug.Log("Block1 cue started: Trial " + currentTrialNumber + ", Episode " + currentEpisodeNumber + ", target " + currentEpisode.PrimaryTarget + ", reason " + reason + ".");
    }

    private void EndCueInternal(string reason)
    {
        if (!cueRunning)
        {
            return;
        }

        cueRunning = false;
        Debug.Log("Block1 cue ended: Trial " + currentTrialNumber + ", Episode " + currentEpisodeNumber + ", reason " + reason + ".");
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

    private void BuildBlock1Schedules()
    {
        trials.Clear();
        trials.Add(1, new[]
        {
            new EpisodeSchedule(
                "Clear Single Entry Request",
                40f,
                100f,
                60f,
                65f,
                "ZHZ",
                "",
                MemberSchedule.WithSegments(88f, new[]
                {
                    ProbabilitySegment.Linear(56f, 60f, 20f, 74f),
                    ProbabilitySegment.Peaked(60f, 65f, 74f, 88f, 74f),
                    ProbabilitySegment.Linear(65f, 68f, 74f, 20f)
                }),
                MemberSchedule.Background(),
                MemberSchedule.Background()),
            new EpisodeSchedule(
                "Competing Entry Requests",
                100f,
                170f,
                130f,
                135f,
                "DCY",
                "ZJR",
                MemberSchedule.Background(),
                MemberSchedule.WithSegments(92f, new[]
                {
                    ProbabilitySegment.Linear(126f, 130f, 25f, 78f),
                    ProbabilitySegment.Peaked(130f, 135f, 78f, 92f, 78f),
                    ProbabilitySegment.Linear(135f, 138f, 78f, 25f)
                }),
                MemberSchedule.WithSegments(80f, new[]
                {
                    ProbabilitySegment.Linear(127f, 130f, 20f, 72f),
                    ProbabilitySegment.Peaked(130f, 135f, 72f, 80f, 71f),
                    ProbabilitySegment.Linear(135f, 138f, 71f, 20f)
                })),
            new EpisodeSchedule(
                "Repeated Single Entry Request",
                170f,
                240f,
                200f,
                205f,
                "ZJR",
                "",
                MemberSchedule.Background(),
                MemberSchedule.Background(),
                MemberSchedule.WithSegments(86f, new[]
                {
                    ProbabilitySegment.Linear(196f, 200f, 20f, 74f),
                    ProbabilitySegment.Peaked(200f, 205f, 74f, 86f, 74f),
                    ProbabilitySegment.Linear(205f, 208f, 74f, 20f)
                }))
        });

        trials.Add(2, new[]
        {
            new EpisodeSchedule(
                "Clear Single Entry Request",
                40f,
                100f,
                60f,
                65f,
                "DCY",
                "",
                MemberSchedule.Background(),
                MemberSchedule.WithSegments(88f, new[]
                {
                    ProbabilitySegment.Linear(56f, 60f, 20f, 74f),
                    ProbabilitySegment.Peaked(60f, 65f, 74f, 88f, 74f),
                    ProbabilitySegment.Linear(65f, 68f, 74f, 20f)
                }),
                MemberSchedule.Background()),
            new EpisodeSchedule(
                "Competing Entry Requests",
                100f,
                170f,
                130f,
                135f,
                "ZJR",
                "ZHZ",
                MemberSchedule.WithSegments(80f, new[]
                {
                    ProbabilitySegment.Linear(127f, 130f, 20f, 72f),
                    ProbabilitySegment.Peaked(130f, 135f, 72f, 80f, 71f),
                    ProbabilitySegment.Linear(135f, 138f, 71f, 20f)
                }),
                MemberSchedule.Background(),
                MemberSchedule.WithSegments(92f, new[]
                {
                    ProbabilitySegment.Linear(126f, 130f, 25f, 78f),
                    ProbabilitySegment.Peaked(130f, 135f, 78f, 92f, 78f),
                    ProbabilitySegment.Linear(135f, 138f, 78f, 25f)
                })),
            new EpisodeSchedule(
                "Repeated Single Entry Request",
                170f,
                240f,
                200f,
                205f,
                "ZHZ",
                "",
                MemberSchedule.WithSegments(86f, new[]
                {
                    ProbabilitySegment.Linear(196f, 200f, 20f, 74f),
                    ProbabilitySegment.Peaked(200f, 205f, 74f, 86f, 74f),
                    ProbabilitySegment.Linear(205f, 208f, 74f, 20f)
                }),
                MemberSchedule.Background(),
                MemberSchedule.Background())
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
