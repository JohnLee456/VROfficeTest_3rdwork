using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-69)]
public class StaffEpisodePromptBoard : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private const string ControlledSceneName = OfficeSceneSupport.OfficeLoggedInNoBot;
    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, 0.08f, 1.05f);
    [SerializeField] private Vector2 panelSize = new Vector2(780f, 560f);
    [SerializeField] private float worldScale = 0.00155f;
    [SerializeField] private KeyCode toggleKey = KeyCode.N;
#if UNITY_EDITOR
    [SerializeField] private KeyCode editorDebugStartKey = KeyCode.B;
#endif

    private Camera cachedCamera;
    private GameObject boardRoot;
    private RectTransform panelRect;
    private Text titleText;
    private Text metaText;
    private Text contentText;
    private Text footerText;

    private int currentBlockNumber = 1;
    private int currentTrialNumber = 1;
    private int currentEpisodeNumber;
    private double episodeStartTime;
    private bool hasEpisodeStart;
    private bool phaseRunning;
    private bool boardVisible = true;
    private int lastRenderedPromptBlock = -1;
    private int lastRenderedPromptTrial = -1;
    private PromptPhase lastRenderedPromptPhase = (PromptPhase)(-1);
    private static Font cjkFont;
#if UNITY_EDITOR
    private int editorDebugTrialNumber = 1;
    private int editorDebugEpisodeNumber = Study2TrialPhaseInfo.Opening;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForScene()
    {
        TryCreateForCurrentScene();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateBootstrap()
    {
        if (FindObjectOfType<StaffEpisodePromptBoardBootstrap>() != null)
        {
            return;
        }

        GameObject bootstrap = new GameObject("Staff Episode Prompt Board Bootstrap");
        DontDestroyOnLoad(bootstrap);
        bootstrap.AddComponent<StaffEpisodePromptBoardBootstrap>();
    }

    public static void TryCreateForCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!ShouldCreateForScene(activeScene) || FindObjectOfType<StaffEpisodePromptBoard>() != null)
        {
            return;
        }

        GameObject manager = new GameObject("Staff Episode Prompt Board");
        manager.AddComponent<StaffEpisodePromptBoard>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Start()
    {
        if (!CanShowForCurrentUser())
        {
            enabled = false;
            return;
        }

        BuildBoard();
        TryApplyRoomState();
        RefreshBoard();
    }

    private void Update()
    {
        if (!CanShowForCurrentUser())
        {
            if (boardRoot != null)
            {
                boardRoot.SetActive(false);
            }

            return;
        }

        EnsureCameraAttachment();

        if (Input.GetKeyDown(toggleKey) || OfficeVrControllerInput.GetBDown())
        {
            boardVisible = !boardVisible;
            if (boardRoot != null)
            {
                boardRoot.SetActive(boardVisible);
            }
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(editorDebugStartKey))
        {
            SimulateEpisodeStartInEditor();
        }
#endif

        RefreshBoard();
    }

    public void OnEvent(EventData photonEvent)
    {
        int blockNumber;
        int trialNumber;
        int episodeNumber;
        double startTime;

        if (photonEvent.Code == Block1EpisodeSync.EpisodeReadyEventCode)
        {
            if (Block1EpisodeSync.TryParsePayload(photonEvent.CustomData, out blockNumber, out trialNumber, out episodeNumber, out startTime))
            {
                ApplyEpisodeReady(blockNumber, trialNumber, episodeNumber);
            }

            return;
        }

        if (photonEvent.Code == Block1EpisodeSync.EpisodeStartedEventCode &&
            Block1EpisodeSync.TryParsePayload(photonEvent.CustomData, out blockNumber, out trialNumber, out episodeNumber, out startTime))
        {
            ApplyEpisodeStart(blockNumber, trialNumber, episodeNumber, startTime);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(Block1EpisodeSync.BlockKey) ||
            propertiesThatChanged.ContainsKey(Block1EpisodeSync.TrialKey) ||
            propertiesThatChanged.ContainsKey(Block1EpisodeSync.EpisodeKey) ||
            propertiesThatChanged.ContainsKey(Block1EpisodeSync.EpisodeStartTimeKey) ||
            propertiesThatChanged.ContainsKey(Block1EpisodeSync.PromptBlockKey) ||
            propertiesThatChanged.ContainsKey(Block1EpisodeSync.PromptTrialKey) ||
            propertiesThatChanged.ContainsKey(Block1EpisodeSync.PromptEpisodeKey) ||
            propertiesThatChanged.ContainsKey(Block1EpisodeSync.PromptReadyTimeKey))
        {
            TryApplyRoomState();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!ShouldCreateForScene(scene))
        {
            DestroyBoard();
            return;
        }

        if (boardRoot == null)
        {
            BuildBoard();
        }

        TryApplyRoomState();
        RefreshBoard();
    }

    private void TryApplyRoomState()
    {
        int readyBlockNumber;
        int readyTrialNumber;
        int readyEpisodeNumber;
        double readyTime;
        bool hasReadyState = Block1EpisodeSync.TryReadPromptRoomState(out readyBlockNumber, out readyTrialNumber, out readyEpisodeNumber, out readyTime);

        int startBlockNumber;
        int startTrialNumber;
        int startEpisodeNumber;
        double startTime;
        bool hasStartState = Block1EpisodeSync.TryReadRoomState(out startBlockNumber, out startTrialNumber, out startEpisodeNumber, out startTime);

        if (hasReadyState && (!hasStartState || readyTime > startTime))
        {
            ApplyEpisodeReady(readyBlockNumber, readyTrialNumber, readyEpisodeNumber);
            return;
        }

        if (hasStartState)
        {
            ApplyEpisodeStart(startBlockNumber, startTrialNumber, startEpisodeNumber, startTime);
        }
    }

    private void ApplyEpisodeReady(int blockNumber, int trialNumber, int episodeNumber)
    {
        currentBlockNumber = Mathf.Clamp(blockNumber, 1, 3);
        currentTrialNumber = Mathf.Clamp(trialNumber, 1, 3);
        currentEpisodeNumber = Mathf.Clamp(episodeNumber, Study2TrialPhaseInfo.FirstPhaseNumber, Study2TrialPhaseInfo.LastPhaseNumber);
        episodeStartTime = 0d;
        hasEpisodeStart = true;
        phaseRunning = false;
        boardVisible = true;

        if (boardRoot != null)
        {
            boardRoot.SetActive(true);
        }

        RefreshBoard();
    }

    private void ApplyEpisodeStart(int blockNumber, int trialNumber, int episodeNumber, double startTime)
    {
        currentBlockNumber = Mathf.Clamp(blockNumber, 1, 3);
        currentTrialNumber = Mathf.Clamp(trialNumber, 1, 3);
        currentEpisodeNumber = Mathf.Clamp(episodeNumber, Study2TrialPhaseInfo.FirstPhaseNumber, Study2TrialPhaseInfo.LastPhaseNumber);
        episodeStartTime = startTime;
        hasEpisodeStart = true;
        phaseRunning = true;
        boardVisible = true;

        if (boardRoot != null)
        {
            boardRoot.SetActive(true);
        }

        RefreshBoard();
    }

#if UNITY_EDITOR
    private void SimulateEpisodeStartInEditor()
    {
        double startTime = PhotonNetwork.InRoom ? PhotonNetwork.Time : Time.time;
        ApplyEpisodeStart(1, editorDebugTrialNumber, editorDebugEpisodeNumber, startTime);
        Debug.Log("Editor debug prompt board phase start simulated: Trial " + editorDebugTrialNumber + ", " + Study2TrialPhaseInfo.GetLabel(editorDebugEpisodeNumber) + ".");

        editorDebugEpisodeNumber++;
        if (editorDebugEpisodeNumber > Study2TrialPhaseInfo.LastPhaseNumber)
        {
            editorDebugEpisodeNumber = Study2TrialPhaseInfo.Opening;
            editorDebugTrialNumber++;
        }

        if (editorDebugTrialNumber > 3)
        {
            editorDebugTrialNumber = 1;
        }
    }
#endif

    private void RefreshBoard()
    {
        if (titleText == null || metaText == null || contentText == null || footerText == null)
        {
            return;
        }

        PromptBoardContent prompt = GetCurrentPrompt();
        RefreshRuntimeFontIfPromptChanged(prompt);
        titleText.text = prompt.Title;
        metaText.text = prompt.Meta;
        contentText.text = prompt.Body;
        footerText.text = GetFooterText();
        ApplyReadableWhiteText(titleText, 0.08f);
        ApplyReadableWhiteText(metaText, 0.06f);
        ApplyReadableWhiteText(contentText, 0.045f);
        ApplyReadableWhiteText(footerText, 0.05f);
    }

    private PromptBoardContent GetCurrentPrompt()
    {
        if (!hasEpisodeStart)
        {
            return FindPrompt(1, 1, PromptPhase.Opening);
        }

        return FindPrompt(currentBlockNumber, currentTrialNumber, (PromptPhase)currentEpisodeNumber);
    }

    private string GetFooterText()
    {
        if (!hasEpisodeStart)
        {
            return "Waiting for leader to start Opening Phase / 等待 leader 开始开场阶段";
        }

        string state = Study2TrialPhaseInfo.GetLabel(currentEpisodeNumber);
        if (!phaseRunning)
        {
            return "Block " + currentBlockNumber + " / Trial " + currentTrialNumber + " / " + state + "   Ready / 等待 leader 按 Start   Toggle: N or controller B";
        }

        float elapsed = GetElapsedSeconds();
        float duration = GetCurrentEpisodeDuration();
        int remaining = Mathf.CeilToInt(Mathf.Max(0f, duration - elapsed));
        return "Block " + currentBlockNumber + " / Trial " + currentTrialNumber + " / " + state + "   Remaining: " + remaining + "s   Toggle: N or controller B";
    }

    private float GetElapsedSeconds()
    {
        if (!hasEpisodeStart || !phaseRunning)
        {
            return 0f;
        }

        return Mathf.Max(0f, (float)((PhotonNetwork.InRoom ? PhotonNetwork.Time : Time.time) - episodeStartTime));
    }

    private float GetCurrentEpisodeDuration()
    {
        return Study2TrialPhaseInfo.GetDuration(currentEpisodeNumber);
    }

    private void BuildBoard()
    {
        if (boardRoot != null)
        {
            return;
        }

        cachedCamera = Camera.main;
        boardRoot = new GameObject("Staff Episode Prompt Board Canvas", typeof(RectTransform));

        Canvas canvas = boardRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 880;
        DashboardOverlayRendering.ConfigureCanvas(canvas, 880);

        boardRoot.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 12f;

        RectTransform rootRect = boardRoot.GetComponent<RectTransform>();
        rootRect.sizeDelta = panelSize;

        GameObject panelObject = CreateRect("Panel", boardRoot.transform);
        panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;

        Image panel = panelObject.AddComponent<Image>();
        panel.color = new Color(0.045f, 0.052f, 0.064f, 0.96f);
        panel.raycastTarget = false;
        DashboardOverlayRendering.ApplyToGraphic(panel);

        Image rule = CreateImage("Top Rule", panelRect, new Vector2(0f, 232f), new Vector2(680f, 3f), new Color(0.34f, 0.62f, 0.72f, 0.92f));
        rule.raycastTarget = false;

        titleText = CreateText("Title", panelRect, string.Empty, new Vector2(0f, 250f), new Vector2(700f, 40f), 24, FontStyle.Bold, TextAnchor.MiddleCenter, false);
        metaText = CreateText("Meta", panelRect, string.Empty, new Vector2(0f, 204f), new Vector2(700f, 42f), 15, FontStyle.Normal, TextAnchor.MiddleCenter, true);
        contentText = CreateText("Content", panelRect, string.Empty, new Vector2(0f, -15f), new Vector2(700f, 390f), 12, FontStyle.Normal, TextAnchor.UpperLeft, true);
        contentText.lineSpacing = 0.92f;
        contentText.resizeTextForBestFit = false;
        footerText = CreateText("Footer", panelRect, string.Empty, new Vector2(0f, -254f), new Vector2(700f, 28f), 13, FontStyle.Normal, TextAnchor.MiddleCenter, false);

        EnsureCameraAttachment();
        DashboardOverlayRendering.ApplyToRoot(boardRoot);
        boardRoot.SetActive(boardVisible);
    }

    private void EnsureCameraAttachment()
    {
        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        if (boardRoot == null || cachedCamera == null)
        {
            return;
        }

        if (boardRoot.transform.parent != cachedCamera.transform)
        {
            boardRoot.transform.SetParent(cachedCamera.transform, false);
        }

        boardRoot.transform.localPosition = cameraLocalPosition;
        boardRoot.transform.localRotation = Quaternion.identity;
        boardRoot.transform.localScale = Vector3.one * worldScale;

        Canvas canvas = boardRoot.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = cachedCamera;
        }
    }

    private void DestroyBoard()
    {
        if (boardRoot != null)
        {
            Destroy(boardRoot);
            boardRoot = null;
        }

        enabled = false;
    }

    private static PromptBoardContent FindPrompt(int block, int trial, PromptPhase phase)
    {
        for (int i = 0; i < Prompts.Length; i++)
        {
            PromptBoardContent prompt = Prompts[i];
            if (prompt.Block == block && prompt.Trial == trial && prompt.Phase == phase)
            {
                return prompt;
            }
        }

        return Prompts[0];
    }

    private void RefreshRuntimeFontIfPromptChanged(PromptBoardContent prompt)
    {
        if (prompt.Block == lastRenderedPromptBlock &&
            prompt.Trial == lastRenderedPromptTrial &&
            prompt.Phase == lastRenderedPromptPhase)
        {
            return;
        }

        lastRenderedPromptBlock = prompt.Block;
        lastRenderedPromptTrial = prompt.Trial;
        lastRenderedPromptPhase = prompt.Phase;
        cjkFont = null;
    }

    private static bool ShouldCreateForScene(Scene scene)
    {
        return scene.IsValid() && scene.name == ControlledSceneName && CanShowForCurrentUser();
    }

    private static bool CanShowForCurrentUser()
    {
        return LoginSession.HasRoute &&
            LoginSession.Role == LoginUserRole.Member &&
            IsMemberAvatar(LoginSession.AvatarName) &&
            SceneManager.GetActiveScene().name == ControlledSceneName;
    }

    private static bool IsMemberAvatar(string avatarName)
    {
        return avatarName == "ZHZ" || avatarName == "DCY" || avatarName == "ZJR";
    }

    private static Image CreateImage(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject imageObject = CreateRect(objectName, parent);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        DashboardOverlayRendering.ApplyToGraphic(image);
        return image;
    }

    private static Text CreateText(string objectName, Transform parent, string text, Vector2 anchoredPosition, Vector2 size, int fontSize, FontStyle fontStyle, TextAnchor alignment, bool wrap)
    {
        GameObject textObject = CreateRect(objectName, parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text label = textObject.AddComponent<Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = alignment;
        label.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.supportRichText = false;
        label.raycastTarget = false;
        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.72f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);
        ApplyReadableWhiteText(label, 0.06f);
        DashboardOverlayRendering.ApplyToGraphic(label);
        return label;
    }

    private static void ApplyReadableWhiteText(Text label, float outlineWidth)
    {
        if (label == null)
        {
            return;
        }

        EnsureReadableFont(label);
        label.color = Color.white;
        DashboardOverlayRendering.ApplyToGraphic(label);
    }

    private static void EnsureReadableFont(Text label)
    {
        Font font = GetCjkFont();
        if (font != null && label.font != font)
        {
            label.font = font;
        }
    }

    private static Font GetCjkFont()
    {
        if (cjkFont != null)
        {
            return cjkFont;
        }

        cjkFont = Font.CreateDynamicFontFromOSFont(
            new[]
            {
                "Microsoft YaHei UI",
                "Microsoft YaHei",
                "SimHei",
                "SimSun",
                "NSimSun",
                "Noto Sans CJK SC",
                "Noto Sans CJK JP",
                "Noto Sans CJK",
                "Droid Sans Fallback",
                "Yu Gothic",
                "Meiryo",
                "Arial Unicode MS"
            },
            18);

        if (cjkFont == null)
        {
            cjkFont = TryLoadFontFile(
                "C:/Windows/Fonts/msyh.ttc",
                "C:/Windows/Fonts/simhei.ttf",
                "C:/Windows/Fonts/simsun.ttc",
                "/system/fonts/NotoSansCJK-Regular.ttc",
                "/system/fonts/DroidSansFallback.ttf");
        }

        if (cjkFont == null)
        {
            cjkFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        if (cjkFont != null)
        {
            cjkFont.name = "Runtime CJK Prompt Board Font";
        }

        return cjkFont;
    }

    private static Font TryLoadFontFile(params string[] paths)
    {
        for (int i = 0; i < paths.Length; i++)
        {
            if (!System.IO.File.Exists(paths[i]))
            {
                continue;
            }

            Font font = new Font(paths[i]);
            if (font != null)
            {
                return font;
            }
        }

        return null;
    }

    private static GameObject CreateRect(string objectName, Transform parent)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private enum PromptPhase
    {
        Opening = 0,
        Episode1 = 1,
        Episode2 = 2,
        Episode3 = 3,
        Summary = 4
    }

    private readonly struct PromptBoardContent
    {
        public readonly int Block;
        public readonly int Trial;
        public readonly PromptPhase Phase;
        public readonly string Title;
        public readonly string Meta;
        public readonly string Body;

        public PromptBoardContent(int block, int trial, PromptPhase phase, string title, string meta, string body)
        {
            Block = block;
            Trial = trial;
            Phase = phase;
            Title = title;
            Meta = meta;
            Body = body;
        }
    }

    private static readonly PromptBoardContent[] Prompts =
    {
        P(1, 1, PromptPhase.Opening, "B1-T1-O Staff Episode 提示板 — Opening Phase（开场阶段）", "Staff Episode | Block 1 | Trial 1 | Opening Phase", "时间：0–40s\n讨论情境：Speaking Context（leader 主导发言情境）\n场景：荒岛求生\n\n本 episode 的 items：\n- Water filter（水过滤器）\n\nEpisode 任务：\n自然开始本 trial。Leader 介绍荒岛求生任务，并开始讨论是否应优先选择水过滤器。\n\nDiscussion focus：\n- 饮用水是否是荒岛上最紧急的生存需求？\n- 如果小组找到不安全的淡水，水过滤器是否有帮助？\n- 如果只有海水可用，水过滤器是否仍然有用？\n- 它是否应排在工具、食物获取物品或求救物品之前？\n\nStaff 角色任务：\nMember ZHZ：简短回应 leader，帮助进入讨论。\nMember DCY：简短回应，或自然地表示同意/不同意。\nMember ZJR：简短回应并确认理解任务。\n\n注意：\n- 本阶段不要触发正式 speaking-intention event。\n- 不要争抢发言权。"),
        P(1, 1, PromptPhase.Episode1, "B1-T1-E1 Staff Episode 提示板 — Episode 1：单一成员进入请求", "Staff Episode | Block 1 | Trial 1 | Episode 1", "时间：40–100s\n讨论情境：Speaking Context（leader 主导发言情境）\n场景：荒岛求生\n\n本 episode 的 items：\n- Knife（刀）\n\nEpisode 任务：\nLeader 正在解释或总结刀的重要性。Member ZHZ 有一个有价值的补充观点，应被视为主要目标成员。\n\nDiscussion focus：\n- 刀是否可用于切割树枝、处理食物或制作简单工具？\n- 它是否有助于安全、急救或搭建庇护所？\n- 当它与绳子、捕鱼工具或庇护类物品配合使用时，价值是否更高？\n- 它应排在水过滤器之前还是之后？\n\nStaff 角色任务：\nMember ZHZ：目标成员。准备一个清晰补充观点，例如“刀具有间接价值，因为它可以帮助制作其他工具”。等待 leader 邀请；被邀请后自然发言。\nMember DCY：普通参与者。倾听即可，如有需要只做简短回应。\nMember ZJR：普通参与者。倾听即可，如有需要只做简短回应。\n\n注意：\n- Member ZHZ 不应打断 leader。\n- 如果 leader 邀请 ZHZ，则该事件成功。"),
        P(1, 1, PromptPhase.Episode2, "B1-T1-E2 Staff Episode 提示板 — Episode 2：竞争性进入请求", "Staff Episode | Block 1 | Trial 1 | Episode 2", "时间：100–170s\n讨论情境：Speaking Context（leader 主导发言情境）\n场景：荒岛求生\n\n本 episode 的 items：\n- Fishing rod / fishing net（鱼竿/渔网）\n\nEpisode 任务：\nLeader 正在讨论捕鱼工具。两名成员都有可能补充观点，但 Member DCY 是更强的目标成员，Member ZJR 是较低优先级的竞争成员。\n\nDiscussion focus：\n- 这个 item 能否解决长期食物需求？\n- 捕鱼是否需要技巧、时间和体力？\n- 在最初几天，食物是否不如水紧急？\n- 这个 item 是否足够可靠，可以排在较高位置？\n\nStaff 角色任务：\nMember ZHZ：普通参与者。倾听，不加入竞争。\nMember DCY：主要目标成员。准备主要观点，例如“食物在最初几天可能不是最紧急需求，但捕鱼工具对长期生存很重要”。被邀请后自然发言。\nMember ZJR：次要竞争成员。准备较弱或更有条件性的观点，例如“如果附近鱼很少，这个 item 的不确定性较高”。被邀请后自然发言，但不要表现得比 DCY 更强。\n\n注意：\n- 预期路径：leader 邀请 DCY。\n- 如果 leader 先邀请 ZJR，则记录为 target mismatch。\n- 如果 DCY 和 ZJR 都没有被邀请，则记录为 missed。"),
        P(1, 1, PromptPhase.Episode3, "B1-T1-E3 Staff Episode 提示板 — Episode 3：重复单一进入请求", "Staff Episode | Block 1 | Trial 1 | Episode 3", "时间：170–240s\n讨论情境：Speaking Context（leader 主导发言情境）\n场景：荒岛求生\n\n本 episode 的 items：\n- Flare gun（信号枪）\n\nEpisode 任务：\nLeader 正在解释或总结信号枪的求救价值。Member ZJR 是主要目标成员。\n\nDiscussion focus：\n- 信号枪能否帮助向船只或飞机发出求救信号？\n- 它是否属于有限次数或一次性求救物品？\n- 它在夜间还是白天更有效？\n- 与水或工具等稳定生存物品相比，它的价值是更高还是更低？\n\nStaff 角色任务：\nMember ZHZ：普通参与者。倾听即可，只做简短回应。\nMember DCY：普通参与者。倾听即可，只做简短回应。\nMember ZJR：目标成员。准备一个补充观点，例如“如果岛屿靠近航线或航道，信号枪非常有价值，但它不如水过滤器稳定”。等待 leader 邀请。\n\n注意：\n- Member ZJR 不应打断。\n- 如果 leader 邀请 ZJR，则该事件成功。"),
        P(1, 1, PromptPhase.Summary, "B1-T1-S Staff Episode 提示板 — Summary Stage（总结阶段）", "Staff Episode | Block 1 | Trial 1 | Summary Stage", "时间：240–300s\n讨论情境：Speaking Context（leader 主导发言情境）\n场景：荒岛求生\n\n本 episode 的 items：\n- Water filter（水过滤器）\n- Knife（刀）\n- Fishing rod / fishing net（鱼竿/渔网）\n- Flare gun（信号枪）\n\nEpisode 任务：\n帮助 leader 总结前四个荒岛求生 items，并形成阶段性排序或选择理由。\n\nDiscussion focus：\n- 哪个 item 应排在最高位置？\n- 哪些 items 支持即时生存？\n- 哪些 items 支持长期生存或求救？\n- 是否还有最后补充意见？\n\nStaff 角色任务：\nMember ZHZ：如被邀请，可确认讨论结果或简短补充。\nMember DCY：如被邀请，可确认讨论结果或简短补充。\nMember ZJR：如被邀请，可确认讨论结果或简短补充。\n\n注意：\n- 不要开启新的主要 speaking-intention event。\n- 如 leader 发出最后补充邀请，可自然回应。"),
        P(1, 2, PromptPhase.Opening, "B1-T2-O Staff Episode 提示板 — Opening Phase（开场阶段）", "Staff Episode | Block 1 | Trial 2 | Opening Phase", "时间：0–40s\n讨论情境：Speaking Context（leader 主导发言情境）\n场景：荒岛求生\n\n本 episode 的 items：\n- First aid kit（急救包）\n\nEpisode 任务：\n开始讨论该 item 的必要性。\n\nDiscussion focus：\n- 该 item 能否解决当前生存阶段的关键问题？\n- 它与水、工具、火源、庇护或求救有什么关系？\n- 它是否应排在较高位置？\n\nStaff 角色任务：\nMember ZHZ：简短回应 leader，帮助进入讨论。\nMember DCY：简短回应，或自然地表示同意/不同意。\nMember ZJR：简短回应并确认理解任务。\n\n注意：\n- 不要触发正式 speaking-intention event。\n- 不要争抢发言权。"),
        P(1, 2, PromptPhase.Episode1, "B1-T2-E1 Staff Episode 提示板 — Episode 1：单一成员进入请求", "Staff Episode | Block 1 | Trial 2 | Episode 1", "时间：40–100s\n讨论情境：Speaking Context（leader 主导发言情境）\n场景：荒岛求生\n\n本 episode 的 items：\n- Rope（绳子）\n\nEpisode 任务：\nLeader 正在解释 Rope（绳子） 的用途。Member DCY 是具有有效补充观点的目标成员。\n\nDiscussion focus：\n- 这个 item 是否具有多用途价值？\n- 它能否与其他 items 组合使用？\n- 它是否比单一用途 item 更值得优先选择？\n\nStaff 角色任务：\nMember ZHZ：普通参与者。倾听即可，只做简短回应。\nMember DCY：目标成员。准备相关补充观点。等待 leader 邀请。\nMember ZJR：普通参与者。倾听即可，只做简短回应。\n\n注意：\n- Member DCY 不应打断。\n- 如果 leader 邀请 DCY，则该事件成功。"),
        P(1, 2, PromptPhase.Episode2, "B1-T2-E2 Staff Episode 提示板 — Episode 2：竞争性进入请求", "Staff Episode | Block 1 | Trial 2 | Episode 2", "时间：100–170s\n讨论情境：Speaking Context（leader 主导发言情境）\n场景：荒岛求生\n\n本 episode 的 items：\n- Lighter / matches（打火机/火柴）\n\nEpisode 任务：\n两名成员都对 Lighter / matches（打火机/火柴） 的重要性有观点。Member ZJR 是主要目标成员，Member ZHZ 是次要竞争成员。\n\nDiscussion focus：\n- 该 item 是否支持保暖、煮水、防护或求救？\n- 它在当前环境中是否可靠？\n- 它是否与其他 items 形成互补？\n\nStaff 角色任务：\nMember ZHZ：次要竞争成员。准备较弱或有条件性的观点，不要表现得比主要目标成员更强。\nMember DCY：普通参与者。倾听，不参与竞争。\nMember ZJR：主要目标成员。准备主要观点。\n\n注意：\n- 预期路径：leader 邀请 ZJR。\n- 如果 leader 先邀请次要竞争成员，则记录为 target mismatch。"),
        P(1, 2, PromptPhase.Episode3, "B1-T2-E3 Staff Episode 提示板 — Episode 3：重复单一进入请求", "Staff Episode | Block 1 | Trial 2 | Episode 3", "时间：170–240s\n讨论情境：Speaking Context（leader 主导发言情境）\n场景：荒岛求生\n\n本 episode 的 items：\n- Tent / hammock（帐篷/吊床）\n\nEpisode 任务：\nLeader 正在讨论 Tent / hammock（帐篷/吊床）。Member ZHZ 是具有补充观点的主要目标成员。\n\nDiscussion focus：\n- 该 item 是否支持庇护、休息或防护？\n- 它是否能提高长期生存质量？\n- 它与更紧急的水、火源或求救 items 相比如何？\n\nStaff 角色任务：\nMember ZHZ：目标成员。准备相关补充观点。等待 leader 邀请。\nMember DCY：普通参与者。倾听即可，只做简短回应。\nMember ZJR：普通参与者。倾听即可，只做简短回应。\n\n注意：\n- Member ZHZ 不应打断。\n- 如果 leader 邀请 ZHZ，则该事件成功。"),
        P(1, 2, PromptPhase.Summary, "B1-T2-S Staff Episode 提示板 — Summary Stage（总结阶段）", "Staff Episode | Block 1 | Trial 2 | Summary Stage", "时间：240–300s\n讨论情境：Speaking Context（leader 主导发言情境）\n场景：荒岛求生\n\n本 episode 的 items：\n- First aid kit（急救包）\n- Rope（绳子）\n- Lighter / matches（打火机/火柴）\n- Tent / hammock（帐篷/吊床）\n\nEpisode 任务：\n帮助 leader 总结本 trial 的四个 items。\n\nDiscussion focus：\n- 哪个 item 最紧急？\n- 哪个 item 最灵活？\n- 哪个 item 支持长期生存？\n- 是否还有最后补充意见？\n\nStaff 角色任务：\nMember ZHZ：如被邀请，可确认讨论结果或简短补充。\nMember DCY：如被邀请，可确认讨论结果或简短补充。\nMember ZJR：如被邀请，可确认讨论结果或简短补充。\n\n注意：\n- 不要开启新的主要 speaking-intention event。\n- 如 leader 发出最后补充邀请，可自然回应。"),
        P(1, 3, PromptPhase.Opening, "B1-T3-O Staff Episode 提示板 — Opening Phase（开场阶段）", "Staff Episode | Block 1 | Trial 3 | Opening Phase", "时间：0–40s\n讨论情境：Speaking Context（leader 主导发言情境） — 生理感知反馈\n场景：荒岛求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Water container（储水容器）\n\nEpisode 任务：\nLeader 介绍新的荒岛求生 item 集合，并开始讨论储水容器。\n\nDiscussion focus：\n- 储水容器能否帮助储存雨水或过滤后的淡水？\n- 如果水源只是偶尔出现，储水是否重要？\n- 它是否需要与水过滤器或火源配合？\n\nStaff 角色任务：\nMember ZHZ：简短回应 leader，帮助进入讨论。\nMember DCY：简短回应，或自然地表示同意/不同意。\nMember ZJR：简短回应并确认理解任务。\n\n注意：\n- 不要触发正式 speaking-intention event。\n- 不要争抢发言权。"),
        P(1, 3, PromptPhase.Episode1, "B1-T3-E1 Staff Episode 提示板 — Episode 1：单一成员进入请求", "Staff Episode | Block 1 | Trial 3 | Episode 1", "时间：40–100s\n讨论情境：Speaking Context（leader 主导发言情境） — 生理感知反馈\n场景：荒岛求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Machete（砍刀）\n\nEpisode 任务：\nLeader 正在解释或总结砍刀的用途。Member ZJR 是主要目标成员。\n\nDiscussion focus：\n- 砍刀能否切割树枝、清理植被或准备庇护材料？\n- 它是否比小刀更有力量但精细度更低？\n- 它是否因为多用途而应排在较高位置？\n\nStaff 角色任务：\nMember ZHZ：普通参与者。倾听即可，只做简短回应。\nMember DCY：普通参与者。倾听即可，只做简短回应。\nMember ZJR：目标成员。准备相关补充观点。等待 leader 邀请。\n\n注意：\n- Member ZJR 不应打断。\n- 如果 leader 邀请 ZJR，则该事件成功。"),
        P(1, 3, PromptPhase.Episode2, "B1-T3-E2 Staff Episode 提示板 — Episode 2：竞争性进入请求", "Staff Episode | Block 1 | Trial 3 | Episode 2", "时间：100–170s\n讨论情境：Speaking Context（leader 主导发言情境） — 生理感知反馈\n场景：荒岛求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Signal mirror（信号镜）\n\nEpisode 任务：\nLeader 正在讨论信号镜。Member ZHZ 是主要目标成员，Member DCY 是次要竞争成员。\n\nDiscussion focus：\n- 信号镜能否在白天帮助吸引船只或飞机？\n- 与信号枪相比，它是否可以重复使用？\n- 它是否依赖阳光和能见度？\n\nStaff 角色任务：\nMember ZHZ：主要目标成员。准备主要观点。\nMember DCY：次要竞争成员。准备较弱或有条件性的观点，不要表现得比主要目标成员更强。\nMember ZJR：普通参与者。倾听，不加入竞争。\n\n注意：\n- 预期路径：leader 邀请 ZHZ。\n- 如果 leader 先邀请次要竞争成员，则记录为 target mismatch。"),
        P(1, 3, PromptPhase.Episode3, "B1-T3-E3 Staff Episode 提示板 — Episode 3：重复单一进入请求", "Staff Episode | Block 1 | Trial 3 | Episode 3", "时间：170–240s\n讨论情境：Speaking Context（leader 主导发言情境） — 生理感知反馈\n场景：荒岛求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Mosquito net（蚊帐）\n\nEpisode 任务：\nLeader 正在解释或总结蚊帐的防护价值。Member DCY 是主要目标成员。\n\nDiscussion focus：\n- 蚊帐能否防虫并改善睡眠质量？\n- 防止叮咬是否能减少感染或不适？\n- 它是否不如水、火源或求救 items 紧急？\n\nStaff 角色任务：\nMember ZHZ：普通参与者。倾听即可，只做简短回应。\nMember DCY：目标成员。准备相关补充观点。等待 leader 邀请。\nMember ZJR：普通参与者。倾听即可，只做简短回应。\n\n注意：\n- Member DCY 不应打断。\n- 如果 leader 邀请 DCY，则该事件成功。"),
        P(1, 3, PromptPhase.Summary, "B1-T3-S Staff Episode 提示板 — Summary Stage（总结阶段）", "Staff Episode | Block 1 | Trial 3 | Summary Stage", "时间：240–300s\n讨论情境：Speaking Context（leader 主导发言情境） — 生理感知反馈\n场景：荒岛求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Water container（储水容器）\n- Machete（砍刀）\n- Signal mirror（信号镜）\n- Mosquito net（蚊帐）\n\nEpisode 任务：\n帮助 leader 总结本生理感知反馈 trial 中的四个 items。\n\nDiscussion focus：\n- 哪个 item 最能支持水资源管理？\n- 哪个 item 是最灵活的工具？\n- 哪个 item 最能支持求救？\n- 哪个 item 支持防护和休息？\n\nStaff 角色任务：\nMember ZHZ：如被邀请，可确认讨论结果或简短补充。\nMember DCY：如被邀请，可确认讨论结果或简短补充。\nMember ZJR：如被邀请，可确认讨论结果或简短补充。\n\n注意：\n- 不要开启新的主要 speaking-intention event。\n- 如 leader 发出最后补充邀请，可自然回应。"),
        P(2, 1, PromptPhase.Opening, "B2-T1-O Staff Episode 提示板 — Opening Phase（开场阶段）", "Staff Episode | Block 2 | Trial 1 | Opening Phase", "时间：0–40s\n讨论情境：Listening Context（leader 观察协调情境）\n场景：沙漠求生\n\n本 episode 的 items：\n- Cosmetic mirror（化妆镜/信号镜）\n\nEpisode 任务：\nLeader 介绍沙漠求生任务，并把讨论交给成员。Member ZHZ 和 Member DCY 开始讨论 Cosmetic mirror（化妆镜/信号镜）。\n\nDiscussion focus：\n- 该 item 是否能帮助小组在沙漠中生存或求救？\n- 它的价值是否取决于留在坠落点还是主动移动？\n- 它与水、遮阳、信号或工具类 items 相比如何？\n\nStaff 角色任务：\nMember ZHZ：active speaker。开始讨论该 item。\nMember DCY：active speaker。回应 ZHZ 并继续讨论。\nMember ZJR：普通参与者。保持参与感，但不要成为目标成员。\n\n注意：\n- 不要触发正式 speaking-intention event。"),
        P(2, 1, PromptPhase.Episode1, "B2-T1-E1 Staff Episode 提示板 — Episode 1：双人讨论中的被压制进入请求", "Staff Episode | Block 2 | Trial 1 | Episode 1", "时间：40–100s\n讨论情境：Listening Context（leader 观察协调情境）\n场景：沙漠求生\n\n本 episode 的 items：\n- Top coat per person（每人一件外套）\n\nEpisode 任务：\nMember ZHZ 和 Member DCY 正在讨论 Top coat per person（每人一件外套）。Member ZJR 有补充观点，但还没有获得发言机会。\n\nDiscussion focus：\n- 该 item 的主要用途是什么？\n- 它是否有明显限制或风险？\n- 它是否应排在水或求救信号类 items 之前？\n\nStaff 角色任务：\nMember ZHZ：active speaker。与 DCY 维持双人讨论。\nMember DCY：active speaker。回应 ZHZ 并继续讨论。\nMember ZJR：目标成员。准备一个补充观点，等待 leader 邀请。\n\n注意：\n- Member ZJR 不应打断。\n- 如果 leader 邀请 ZJR，则该事件成功。"),
        P(2, 1, PromptPhase.Episode2, "B2-T1-E2 Staff Episode 提示板 — Episode 2：主导发言者压制目标成员", "Staff Episode | Block 2 | Trial 1 | Episode 2", "时间：100–170s\n讨论情境：Listening Context（leader 观察协调情境）\n场景：沙漠求生\n\n本 episode 的 items：\n- Water per person（每人一份水）\n\nEpisode 任务：\nMember ZHZ 围绕 Water per person（每人一份水） 说得比其他人更多。Member DCY 还没有机会表达重要观点。\n\nDiscussion focus：\n- 该 item 是否解决沙漠中的关键风险？\n- 如果小组选择移动或等待，它的价值是否变化？\n- 它与其他 items 的优先级关系如何？\n\nStaff 角色任务：\nMember ZHZ：主导发言者。给出多个理由，但不要过度表演。\nMember DCY：目标成员。准备一个重要补充观点，等待 leader 邀请。\nMember ZJR：次要回应者。简短回应 ZHZ，不要成为目标成员。\n\n注意：\n- Member DCY 是目标成员。\n- 如果 leader 邀请 DCY，则该事件成功。"),
        P(2, 1, PromptPhase.Episode3, "B2-T1-E3 Staff Episode 提示板 — Episode 3：重复被压制进入事件", "Staff Episode | Block 2 | Trial 1 | Episode 3", "时间：170–240s\n讨论情境：Listening Context（leader 观察协调情境）\n场景：沙漠求生\n\n本 episode 的 items：\n- Flashlight（手电筒）\n\nEpisode 任务：\nMember DCY 和 Member ZJR 正在讨论 Flashlight（手电筒）。Member ZHZ 有补充观点，但还没有获得发言机会。\n\nDiscussion focus：\n- 该 item 是否更适合短期生存还是长期生存？\n- 它是否依赖特定策略，例如移动、等待或求救？\n- 是否存在不确定性或使用限制？\n\nStaff 角色任务：\nMember ZHZ：目标成员。准备一个补充观点，等待 leader 邀请。\nMember DCY：active speaker。与 ZJR 讨论该 item。\nMember ZJR：active speaker。回应 DCY 并维持双人讨论。\n\n注意：\n- Member ZHZ 不应打断。\n- 如果 leader 邀请 ZHZ，则该事件成功。"),
        P(2, 1, PromptPhase.Summary, "B2-T1-S Staff Episode 提示板 — Summary Stage（总结阶段）", "Staff Episode | Block 2 | Trial 1 | Summary Stage", "时间：240–300s\n讨论情境：Listening Context（leader 观察协调情境）\n场景：沙漠求生\n\n本 episode 的 items：\n- Cosmetic mirror（化妆镜/信号镜）\n- Top coat per person（每人一件外套）\n- Water per person（每人一份水）\n- Flashlight（手电筒）\n\nEpisode 任务：\n帮助 leader 总结本 trial 的四个 沙漠求生 items。\n\nDiscussion focus：\n- 哪个 item 最支持即时生存？\n- 哪个 item 最支持求救或安全？\n- 哪个 item 的价值最依赖小组策略？\n- 是否还有最后补充意见？\n\nStaff 角色任务：\nMember ZHZ：如被邀请，可确认讨论结果或简短补充。\nMember DCY：如被邀请，可确认讨论结果或简短补充。\nMember ZJR：如被邀请，可确认讨论结果或简短补充。\n\n注意：\n- 不要开启新的主要 speaking-intention event。\n- 如 leader 发出最后补充邀请，可自然回应。"),
        P(2, 2, PromptPhase.Opening, "B2-T2-O Staff Episode 提示板 — Opening Phase（开场阶段）", "Staff Episode | Block 2 | Trial 2 | Opening Phase", "时间：0–40s\n讨论情境：Listening Context（leader 观察协调情境）\n场景：沙漠求生\n\n本 episode 的 items：\n- Parachute（降落伞）\n\nEpisode 任务：\nLeader 介绍沙漠求生任务，并把讨论交给成员。Member ZHZ 和 Member DCY 开始讨论 Parachute（降落伞）。\n\nDiscussion focus：\n- 该 item 是否能帮助小组在沙漠中生存或求救？\n- 它的价值是否取决于留在坠落点还是主动移动？\n- 它与水、遮阳、信号或工具类 items 相比如何？\n\nStaff 角色任务：\nMember ZHZ：active speaker。开始讨论该 item。\nMember DCY：active speaker。回应 ZHZ 并继续讨论。\nMember ZJR：普通参与者。保持参与感，但不要成为目标成员。\n\n注意：\n- 不要触发正式 speaking-intention event。"),
        P(2, 2, PromptPhase.Episode1, "B2-T2-E1 Staff Episode 提示板 — Episode 1：双人讨论中的被压制进入请求", "Staff Episode | Block 2 | Trial 2 | Episode 1", "时间：40–100s\n讨论情境：Listening Context（leader 观察协调情境）\n场景：沙漠求生\n\n本 episode 的 items：\n- Jack knife（折叠刀）\n\nEpisode 任务：\nMember ZHZ 和 Member DCY 正在讨论 Jack knife（折叠刀）。Member ZJR 有补充观点，但还没有获得发言机会。\n\nDiscussion focus：\n- 该 item 的主要用途是什么？\n- 它是否有明显限制或风险？\n- 它是否应排在水或求救信号类 items 之前？\n\nStaff 角色任务：\nMember ZHZ：active speaker。与 DCY 维持双人讨论。\nMember DCY：active speaker。回应 ZHZ 并继续讨论。\nMember ZJR：目标成员。准备一个补充观点，等待 leader 邀请。\n\n注意：\n- Member ZJR 不应打断。\n- 如果 leader 邀请 ZJR，则该事件成功。"),
        P(2, 2, PromptPhase.Episode2, "B2-T2-E2 Staff Episode 提示板 — Episode 2：主导发言者压制目标成员", "Staff Episode | Block 2 | Trial 2 | Episode 2", "时间：100–170s\n讨论情境：Listening Context（leader 观察协调情境）\n场景：沙漠求生\n\n本 episode 的 items：\n- Sunglasses（太阳镜）\n\nEpisode 任务：\nMember ZHZ 围绕 Sunglasses（太阳镜） 说得比其他人更多。Member DCY 还没有机会表达重要观点。\n\nDiscussion focus：\n- 该 item 是否解决沙漠中的关键风险？\n- 如果小组选择移动或等待，它的价值是否变化？\n- 它与其他 items 的优先级关系如何？\n\nStaff 角色任务：\nMember ZHZ：主导发言者。给出多个理由，但不要过度表演。\nMember DCY：目标成员。准备一个重要补充观点，等待 leader 邀请。\nMember ZJR：次要回应者。简短回应 ZHZ，不要成为目标成员。\n\n注意：\n- Member DCY 是目标成员。\n- 如果 leader 邀请 DCY，则该事件成功。"),
        P(2, 2, PromptPhase.Episode3, "B2-T2-E3 Staff Episode 提示板 — Episode 3：重复被压制进入事件", "Staff Episode | Block 2 | Trial 2 | Episode 3", "时间：170–240s\n讨论情境：Listening Context（leader 观察协调情境）\n场景：沙漠求生\n\n本 episode 的 items：\n- Map / compass（地图/指南针）\n\nEpisode 任务：\nMember DCY 和 Member ZJR 正在讨论 Map / compass（地图/指南针）。Member ZHZ 有补充观点，但还没有获得发言机会。\n\nDiscussion focus：\n- 该 item 是否更适合短期生存还是长期生存？\n- 它是否依赖特定策略，例如移动、等待或求救？\n- 是否存在不确定性或使用限制？\n\nStaff 角色任务：\nMember ZHZ：目标成员。准备一个补充观点，等待 leader 邀请。\nMember DCY：active speaker。与 ZJR 讨论该 item。\nMember ZJR：active speaker。回应 DCY 并维持双人讨论。\n\n注意：\n- Member ZHZ 不应打断。\n- 如果 leader 邀请 ZHZ，则该事件成功。"),
        P(2, 2, PromptPhase.Summary, "B2-T2-S Staff Episode 提示板 — Summary Stage（总结阶段）", "Staff Episode | Block 2 | Trial 2 | Summary Stage", "时间：240–300s\n讨论情境：Listening Context（leader 观察协调情境）\n场景：沙漠求生\n\n本 episode 的 items：\n- Parachute（降落伞）\n- Jack knife（折叠刀）\n- Sunglasses（太阳镜）\n- Map / compass（地图/指南针）\n\nEpisode 任务：\n帮助 leader 总结本 trial 的四个 沙漠求生 items。\n\nDiscussion focus：\n- 哪个 item 最支持即时生存？\n- 哪个 item 最支持求救或安全？\n- 哪个 item 的价值最依赖小组策略？\n- 是否还有最后补充意见？\n\nStaff 角色任务：\nMember ZHZ：如被邀请，可确认讨论结果或简短补充。\nMember DCY：如被邀请，可确认讨论结果或简短补充。\nMember ZJR：如被邀请，可确认讨论结果或简短补充。\n\n注意：\n- 不要开启新的主要 speaking-intention event。\n- 如 leader 发出最后补充邀请，可自然回应。"),
        P(2, 3, PromptPhase.Opening, "B2-T3-O Staff Episode 提示板 — Opening Phase（开场阶段）", "Staff Episode | Block 2 | Trial 3 | Opening Phase", "时间：0–40s\n讨论情境：Listening Context（leader 观察协调情境） — 生理感知反馈\n场景：沙漠求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Plastic raincoat（塑料雨衣）\n\nEpisode 任务：\nLeader 介绍沙漠求生任务，并把讨论交给成员。Member ZHZ 和 Member DCY 开始讨论 Plastic raincoat（塑料雨衣）。\n\nDiscussion focus：\n- 该 item 是否能帮助小组在沙漠中生存或求救？\n- 它的价值是否取决于留在坠落点还是主动移动？\n- 它与水、遮阳、信号或工具类 items 相比如何？\n\nStaff 角色任务：\nMember ZHZ：active speaker。开始讨论该 item。\nMember DCY：active speaker。回应 ZHZ 并继续讨论。\nMember ZJR：普通参与者。保持参与感，但不要成为目标成员。\n\n注意：\n- 不要触发正式 speaking-intention event。"),
        P(2, 3, PromptPhase.Episode1, "B2-T3-E1 Staff Episode 提示板 — Episode 1：双人讨论中的被压制进入请求", "Staff Episode | Block 2 | Trial 3 | Episode 1", "时间：40–100s\n讨论情境：Listening Context（leader 观察协调情境） — 生理感知反馈\n场景：沙漠求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Pistol（手枪）\n\nEpisode 任务：\nMember ZHZ 和 Member DCY 正在讨论 Pistol（手枪）。Member ZJR 有补充观点，但还没有获得发言机会。\n\nDiscussion focus：\n- 该 item 的主要用途是什么？\n- 它是否有明显限制或风险？\n- 它是否应排在水或求救信号类 items 之前？\n\nStaff 角色任务：\nMember ZHZ：active speaker。与 DCY 维持双人讨论。\nMember DCY：active speaker。回应 ZHZ 并继续讨论。\nMember ZJR：目标成员。准备一个补充观点，等待 leader 邀请。\n\n注意：\n- Member ZJR 不应打断。\n- 如果 leader 邀请 ZJR，则该事件成功。"),
        P(2, 3, PromptPhase.Episode2, "B2-T3-E2 Staff Episode 提示板 — Episode 2：主导发言者压制目标成员", "Staff Episode | Block 2 | Trial 3 | Episode 2", "时间：100–170s\n讨论情境：Listening Context（leader 观察协调情境） — 生理感知反馈\n场景：沙漠求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Alcohol bottle（酒精瓶）\n\nEpisode 任务：\nMember ZHZ 围绕 Alcohol bottle（酒精瓶） 说得比其他人更多。Member DCY 还没有机会表达重要观点。\n\nDiscussion focus：\n- 该 item 是否解决沙漠中的关键风险？\n- 如果小组选择移动或等待，它的价值是否变化？\n- 它与其他 items 的优先级关系如何？\n\nStaff 角色任务：\nMember ZHZ：主导发言者。给出多个理由，但不要过度表演。\nMember DCY：目标成员。准备一个重要补充观点，等待 leader 邀请。\nMember ZJR：次要回应者。简短回应 ZHZ，不要成为目标成员。\n\n注意：\n- Member DCY 是目标成员。\n- 如果 leader 邀请 DCY，则该事件成功。"),
        P(2, 3, PromptPhase.Episode3, "B2-T3-E3 Staff Episode 提示板 — Episode 3：重复被压制进入事件", "Staff Episode | Block 2 | Trial 3 | Episode 3", "时间：170–240s\n讨论情境：Listening Context（leader 观察协调情境） — 生理感知反馈\n场景：沙漠求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Desert animals guidebook（沙漠动物指南）\n\nEpisode 任务：\nMember DCY 和 Member ZJR 正在讨论 Desert animals guidebook（沙漠动物指南）。Member ZHZ 有补充观点，但还没有获得发言机会。\n\nDiscussion focus：\n- 该 item 是否更适合短期生存还是长期生存？\n- 它是否依赖特定策略，例如移动、等待或求救？\n- 是否存在不确定性或使用限制？\n\nStaff 角色任务：\nMember ZHZ：目标成员。准备一个补充观点，等待 leader 邀请。\nMember DCY：active speaker。与 ZJR 讨论该 item。\nMember ZJR：active speaker。回应 DCY 并维持双人讨论。\n\n注意：\n- Member ZHZ 不应打断。\n- 如果 leader 邀请 ZHZ，则该事件成功。"),
        P(2, 3, PromptPhase.Summary, "B2-T3-S Staff Episode 提示板 — Summary Stage（总结阶段）", "Staff Episode | Block 2 | Trial 3 | Summary Stage", "时间：240–300s\n讨论情境：Listening Context（leader 观察协调情境） — 生理感知反馈\n场景：沙漠求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Plastic raincoat（塑料雨衣）\n- Pistol（手枪）\n- Alcohol bottle（酒精瓶）\n- Desert animals guidebook（沙漠动物指南）\n\nEpisode 任务：\n帮助 leader 总结本 trial 的四个 沙漠求生 items。\n\nDiscussion focus：\n- 哪个 item 最支持即时生存？\n- 哪个 item 最支持求救或安全？\n- 哪个 item 的价值最依赖小组策略？\n- 是否还有最后补充意见？\n\nStaff 角色任务：\nMember ZHZ：如被邀请，可确认讨论结果或简短补充。\nMember DCY：如被邀请，可确认讨论结果或简短补充。\nMember ZJR：如被邀请，可确认讨论结果或简短补充。\n\n注意：\n- 不要开启新的主要 speaking-intention event。\n- 如 leader 发出最后补充邀请，可自然回应。"),
        P(3, 1, PromptPhase.Opening, "B3-T1-O Staff Episode 提示板 — Opening Phase（开场阶段）", "Staff Episode | Block 3 | Trial 1 | Opening Phase", "时间：0–40s\n讨论情境：Silence Context（讨论过渡与重启情境）\n场景：深山求生\n\n本 episode 的 items：\n- Matches / lighter（火柴/打火机）\n\nEpisode 任务：\n自然开始深山求生讨论。三位 staff 围绕火柴/打火机的生存价值进行简短讨论，为后续 episode 建立讨论节奏。\n\nDiscussion focus：\n- 火源是否有助于保暖、煮水、求救信号或夜间安全？\n- 在深山或寒冷环境中，火源是否比食物更紧急？\n- 火源是否需要干燥材料或庇护所配合？\n- 它是否应排在较高位置？\n\nStaff 角色任务：\nMember ZHZ：普通参与者。自然加入讨论，可给出一个简短理由。\nMember DCY：普通参与者。自然回应 ZHZ 或 leader，可补充一个不同角度。\nMember ZJR：普通参与者。自然确认任务理解，或补充一个简短观点。\n\n注意：\n- 不要触发正式 speaking-intention event。\n- 不需要表演尴尬沉默；保持自然讨论即可。"),
        P(3, 1, PromptPhase.Episode1, "B3-T1-E1 Staff Episode 提示板 — Episode 1：讨论过渡后的单一补充", "Staff Episode | Block 3 | Trial 1 | Episode 1", "时间：40–100s\n讨论情境：Silence Context（讨论过渡与重启情境）\n场景：深山求生\n\n本 episode 的 items：\n- Polythene sheeting / heavy canvas（塑料布/厚帆布）\n\nEpisode 任务：\n围绕塑料布/厚帆布进行正常讨论。当讨论从“火源”自然转向“庇护”时，Member ZHZ 准备一个关键补充点，但不主动抢话，等待 leader 是否邀请。\n\nDiscussion focus：\n- 塑料布/厚帆布能否防风、防雨、防寒？\n- 它是否可作为临时庇护所、地垫或信号标记？\n- 它是否能与火源配合，提高生存质量？\n- 它是否比主动移动寻找救援更有价值？\n\nStaff 角色任务：\nMember ZHZ：目标成员。准备关于 shelter 价值的补充观点，例如“它可以和火源组合，减少热量流失”。不要主动打断，等待 leader 邀请。\nMember DCY：普通回应者。可以简短回应当前讨论，但不要抢先给出 ZHZ 的核心观点。\nMember ZJR：普通回应者。可以提出简单疑问或确认观点，帮助讨论自然推进。\n\n注意：\n- 不要全体保持沉默；应该让讨论自然收束到“还需要谁补充”的状态。\n- 如果 leader 邀请 ZHZ，则该事件成功。"),
        P(3, 1, PromptPhase.Episode2, "B3-T1-E2 Staff Episode 提示板 — Episode 2：讨论过渡中的竞争性补充", "Staff Episode | Block 3 | Trial 1 | Episode 2", "时间：100–170s\n讨论情境：Silence Context（讨论过渡与重启情境）\n场景：深山求生\n\n本 episode 的 items：\n- First-aid kit（急救包）\n\nEpisode 任务：\n围绕急救包进行正常讨论。讨论中 Member DCY 和 Member ZJR 都可以补充，但 Member DCY 的观点更适合先推进当前讨论，Member ZJR 作为次要候选。\n\nDiscussion focus：\n- 急救包是否能处理割伤、摔伤、冻伤或轻伤？\n- 如果当前没有人受伤，它是否仍然值得高排序？\n- 它与火源、庇护、求救 items 相比优先级如何？\n- 它是否能提升团队移动或等待救援时的安全性？\n\nStaff 角色任务：\nMember ZHZ：普通参与者。可提出简短背景观点，但不要成为本轮补充重点。\nMember DCY：主要目标成员。准备更直接的延续观点，例如“急救包不一定最紧急，但能降低受伤后的风险”。等待 leader 邀请。\nMember ZJR：次要候选成员。准备较弱或条件性的观点，例如“如果没有人受伤，它可能低于火源和庇护”。不要表现得比 DCY 更强。\n\n注意：\n- 预期路径：leader 邀请 DCY。\n- 如果 leader 先邀请 ZJR，则记录为 target mismatch。\n- 讨论应自然推进，不需要刻意停顿等待 cue。"),
        P(3, 1, PromptPhase.Episode3, "B3-T1-E3 Staff Episode 提示板 — Episode 3：总结前的定向补充", "Staff Episode | Block 3 | Trial 1 | Episode 3", "时间：170–240s\n讨论情境：Silence Context（讨论过渡与重启情境）\n场景：深山求生\n\n本 episode 的 items：\n- Signal flares（信号弹）\n\nEpisode 任务：\n围绕信号弹进行讨论。讨论接近总结阶段时，Member ZJR 准备一个 rescue 相关补充观点，等待 leader 是否邀请其帮助收束该 item。\n\nDiscussion focus：\n- 信号弹能否有效吸引救援队注意？\n- 它在夜间还是白天更有用？\n- 它是否是有限使用 item？\n- 是否应在救援可能接近时再使用？\n\nStaff 角色任务：\nMember ZHZ：普通参与者。可简短回应，不主动开启新的大段观点。\nMember DCY：普通参与者。可帮助讨论进入总结，但不抢 ZJR 的核心补充点。\nMember ZJR：目标成员。准备关于 rescue timing 的补充观点，例如“信号弹应保留到可能被看到的时候使用”。等待 leader 邀请。\n\n注意：\n- 不要制造长时间沉默；通过“观点自然结束、准备进入总结”的方式形成重启机会。\n- 如果 leader 邀请 ZJR，则该事件成功。"),
        P(3, 1, PromptPhase.Summary, "B3-T1-S Staff Episode 提示板 — Summary Stage（总结阶段）", "Staff Episode | Block 3 | Trial 1 | Summary Stage", "时间：240–300s\n讨论情境：Silence Context（讨论过渡与重启情境）\n场景：深山求生\n\n本 episode 的 items：\n- Matches / lighter（火柴/打火机）\n- Polythene sheeting / heavy canvas（塑料布/厚帆布）\n- First-aid kit（急救包）\n- Signal flares（信号弹）\n\nEpisode 任务：\n帮助 leader 总结本 trial 的四个深山求生 items，不再开启新的主要 speaking-intention event。\n\nDiscussion focus：\n- 哪个 item 最支持即时生存？\n- 哪个 item 最支持保暖或庇护？\n- 哪个 item 最支持求救？\n- 是否还有最后补充意见？\n\nStaff 角色任务：\nMember ZHZ：如被邀请，可确认讨论结果或简短补充。\nMember DCY：如被邀请，可确认讨论结果或简短补充。\nMember ZJR：如被邀请，可确认讨论结果或简短补充。\n\n注意：\n- 不要开启新的主要 speaking-intention event。\n- 如 leader 发出最后补充邀请，可自然回应。"),
        P(3, 2, PromptPhase.Opening, "B3-T2-O Staff Episode 提示板 — Opening Phase（开场阶段）", "Staff Episode | Block 3 | Trial 2 | Opening Phase", "时间：0–40s\n讨论情境：Silence Context（讨论过渡与重启情境）\n场景：深山求生\n\n本 episode 的 items：\n- Bottled water（瓶装水）\n\nEpisode 任务：\n自然开始第二组深山求生讨论。三位 staff 围绕瓶装水的短期价值进行简短讨论，为后续 episode 建立讨论节奏。\n\nDiscussion focus：\n- 深山环境中找水是容易还是困难？\n- 瓶装水是否对短期生存重要？\n- 它是否需要与火源或过滤方式配合？\n- 补水是否比保暖或庇护更紧急？\n\nStaff 角色任务：\nMember ZHZ：普通参与者。自然加入讨论，可给出一个简短理由。\nMember DCY：普通参与者。自然回应 ZHZ 或 leader，可补充一个不同角度。\nMember ZJR：普通参与者。自然确认任务理解，或补充一个简短观点。\n\n注意：\n- 不要触发正式 speaking-intention event。\n- 不需要表演尴尬沉默；保持自然讨论即可。"),
        P(3, 2, PromptPhase.Episode1, "B3-T2-E1 Staff Episode 提示板 — Episode 1：讨论过渡后的单一补充", "Staff Episode | Block 3 | Trial 2 | Episode 1", "时间：40–100s\n讨论情境：Silence Context（讨论过渡与重启情境）\n场景：深山求生\n\n本 episode 的 items：\n- Toolbox / hand axe / knife（工具箱/手斧/刀）\n\nEpisode 任务：\n围绕工具箱/手斧/刀进行正常讨论。当讨论从“瓶装水”自然转向“工具使用”时，Member ZJR 准备一个关键补充点，但不主动抢话，等待 leader 是否邀请。\n\nDiscussion focus：\n- 工具是否能帮助砍树枝、搭建庇护所或修理设备？\n- 工具是否比单一用途 item 更灵活？\n- 它们是否太重，不便携带？\n- 它们能否与绳子、帆布和火源配合？\n\nStaff 角色任务：\nMember ZHZ：普通回应者。可以简短回应当前讨论，但不要抢先给出 ZJR 的核心观点。\nMember DCY：普通回应者。可以提出简单疑问或确认观点，帮助讨论自然推进。\nMember ZJR：目标成员。准备关于工具灵活价值的补充观点。不要主动打断，等待 leader 邀请。\n\n注意：\n- 不要全体保持沉默；应该让讨论自然收束到“还需要谁补充”的状态。\n- 如果 leader 邀请 ZJR，则该事件成功。"),
        P(3, 2, PromptPhase.Episode2, "B3-T2-E2 Staff Episode 提示板 — Episode 2：讨论过渡中的竞争性补充", "Staff Episode | Block 3 | Trial 2 | Episode 2", "时间：100–170s\n讨论情境：Silence Context（讨论过渡与重启情境）\n场景：深山求生\n\n本 episode 的 items：\n- Extra clothing / blanket（额外衣物/毯子）\n\nEpisode 任务：\n围绕额外衣物/毯子进行正常讨论。讨论中 Member ZHZ 和 Member DCY 都可以补充，但 Member ZHZ 的观点更适合先推进当前讨论，Member DCY 作为次要候选。\n\nDiscussion focus：\n- 额外衣物或毯子能否防止失温？\n- 夜间保暖是否比白天行动更重要？\n- 它是否能与火源和庇护所配合？\n- 它是否应排在食物或工具之前？\n\nStaff 角色任务：\nMember ZHZ：主要目标成员。准备更直接的延续观点，例如“防止失温是深山环境中的关键风险”。等待 leader 邀请。\nMember DCY：次要候选成员。准备较弱或更有条件性的观点，例如“如果已有火源，毯子的优先级可能略低”。不要表现得比 ZHZ 更强。\nMember ZJR：普通参与者。可提出简短背景观点，但不要成为本轮补充重点。\n\n注意：\n- 预期路径：leader 邀请 ZHZ。\n- 如果 leader 先邀请 DCY，则记录为 target mismatch。\n- 讨论应自然推进，不需要刻意停顿等待 cue。"),
        P(3, 2, PromptPhase.Episode3, "B3-T2-E3 Staff Episode 提示板 — Episode 3：总结前的定向补充", "Staff Episode | Block 3 | Trial 2 | Episode 3", "时间：170–240s\n讨论情境：Silence Context（讨论过渡与重启情境）\n场景：深山求生\n\n本 episode 的 items：\n- Chocolate / high-energy food（巧克力/高能量食物）\n\nEpisode 任务：\n围绕巧克力/高能量食物进行讨论。讨论接近总结阶段时，Member DCY 准备一个 energy 相关补充观点，等待 leader 是否邀请其帮助收束该 item。\n\nDiscussion focus：\n- 高能量食物能否维持体力和体温？\n- 食物是否不如水、火源或庇护紧急？\n- 巧克力是否轻便且易于分配？\n- 如果小组需要行走或等待救援，它的重要性是否不同？\n\nStaff 角色任务：\nMember ZHZ：普通参与者。可简短回应，不主动开启新的大段观点。\nMember DCY：目标成员。准备关于能量维持的补充观点，例如“如果需要移动，轻便高热量食物有价值”。等待 leader 邀请。\nMember ZJR：普通参与者。可帮助讨论进入总结，但不抢 DCY 的核心补充点。\n\n注意：\n- 不要制造长时间沉默；通过“观点自然结束、准备进入总结”的方式形成重启机会。\n- 如果 leader 邀请 DCY，则该事件成功。"),
        P(3, 2, PromptPhase.Summary, "B3-T2-S Staff Episode 提示板 — Summary Stage（总结阶段）", "Staff Episode | Block 3 | Trial 2 | Summary Stage", "时间：240–300s\n讨论情境：Silence Context（讨论过渡与重启情境）\n场景：深山求生\n\n本 episode 的 items：\n- Bottled water（瓶装水）\n- Toolbox / hand axe / knife（工具箱/手斧/刀）\n- Extra clothing / blanket（额外衣物/毯子）\n- Chocolate / high-energy food（巧克力/高能量食物）\n\nEpisode 任务：\n帮助 leader 总结本 trial 的四个深山求生 items，不再开启新的主要 speaking-intention event。\n\nDiscussion focus：\n- 哪个 item 最支持补水？\n- 哪个 item 最灵活？\n- 哪个 item 最支持保暖？\n- 是否还有最后补充意见？\n\nStaff 角色任务：\nMember ZHZ：如被邀请，可确认讨论结果或简短补充。\nMember DCY：如被邀请，可确认讨论结果或简短补充。\nMember ZJR：如被邀请，可确认讨论结果或简短补充。\n\n注意：\n- 不要开启新的主要 speaking-intention event。\n- 如 leader 发出最后补充邀请，可自然回应。"),
        P(3, 3, PromptPhase.Opening, "B3-T3-O Staff Episode 提示板 — Opening Phase（开场阶段）", "Staff Episode | Block 3 | Trial 3 | Opening Phase", "时间：0–40s\n讨论情境：Silence Context（讨论过渡与重启情境）— 生理感知反馈\n场景：深山求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Whistle（哨子）\n\nEpisode 任务：\n自然开始生理感知反馈 trial。三位 staff 围绕哨子的求救价值进行简短讨论，为后续 episode 建立讨论节奏。\n\nDiscussion focus：\n- 哨子能否在不消耗太多体力的情况下帮助求救？\n- 在雾、森林或低能见度环境中，它是否有用？\n- 声音在深山环境中传播是否足够远？\n- 它是否应排在视觉信号工具之前？\n\nStaff 角色任务：\nMember ZHZ：普通参与者。自然加入讨论，可给出一个简短理由。\nMember DCY：普通参与者。自然回应 ZHZ 或 leader，可补充一个不同角度。\nMember ZJR：普通参与者。自然确认任务理解，或补充一个简短观点。\n\n注意：\n- 不要触发正式 speaking-intention event。\n- 不需要表演尴尬沉默；保持自然讨论即可。\n- 反馈形式可能变化，但 staff 行为应保持自然。"),
        P(3, 3, PromptPhase.Episode1, "B3-T3-E1 Staff Episode 提示板 — Episode 1：讨论过渡后的单一补充", "Staff Episode | Block 3 | Trial 3 | Episode 1", "时间：40–100s\n讨论情境：Silence Context（讨论过渡与重启情境）— 生理感知反馈\n场景：深山求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Sleeping bag（睡袋）\n\nEpisode 任务：\n围绕睡袋进行正常讨论。当讨论从“求救信号”自然转向“保暖与休息”时，Member ZJR 准备一个关键补充点，但不主动抢话，等待 leader 是否邀请。\n\nDiscussion focus：\n- 睡袋能否在夜间防止失温？\n- 在深山环境中，保暖是否比食物更紧急？\n- 没有帐篷时，睡袋是否仍然有用？\n- 它是否笨重但对休息和生存很重要？\n\nStaff 角色任务：\nMember ZHZ：普通回应者。可以简短回应当前讨论，但不要抢先给出 ZJR 的核心观点。\nMember DCY：普通回应者。可以提出简单疑问或确认观点，帮助讨论自然推进。\nMember ZJR：目标成员。准备关于保暖和防止失温的补充观点。不要主动打断，等待 leader 邀请。\n\n注意：\n- 不要全体保持沉默；应该让讨论自然收束到“还需要谁补充”的状态。\n- 如果 leader 邀请 ZJR，则该事件成功。"),
        P(3, 3, PromptPhase.Episode2, "B3-T3-E2 Staff Episode 提示板 — Episode 2：讨论过渡中的竞争性补充", "Staff Episode | Block 3 | Trial 3 | Episode 2", "时间：100–170s\n讨论情境：Silence Context（讨论过渡与重启情境）— 生理感知反馈\n场景：深山求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Metal cup / cooking pot（金属杯/锅）\n\nEpisode 任务：\n围绕金属杯/锅进行正常讨论。讨论中 Member ZHZ 和 Member DCY 都可以补充，但 Member ZHZ 的观点更适合先推进当前讨论，Member DCY 作为次要候选。\n\nDiscussion focus：\n- 金属杯或锅能否用于煮水？\n- 它是否能帮助融雪或准备热饮？\n- 它是否只有在小组也有火源时才有用？\n- 水处理功能是否让它比食物更重要？\n\nStaff 角色任务：\nMember ZHZ：主要目标成员。准备更直接的延续观点，例如“如果有火源，金属容器能把雪或不安全的水变成可饮用水”。等待 leader 邀请。\nMember DCY：次要候选成员。准备较弱或更有条件性的观点，例如“如果没有火源，它的价值会下降”。不要表现得比 ZHZ 更强。\nMember ZJR：普通参与者。可提出简短背景观点，但不要成为本轮补充重点。\n\n注意：\n- 预期路径：leader 邀请 ZHZ。\n- 如果 leader 先邀请 DCY，则记录为 target mismatch。\n- 讨论应自然推进，不需要刻意停顿等待 cue。"),
        P(3, 3, PromptPhase.Episode3, "B3-T3-E3 Staff Episode 提示板 — Episode 3：总结前的定向补充", "Staff Episode | Block 3 | Trial 3 | Episode 3", "时间：170–240s\n讨论情境：Silence Context（讨论过渡与重启情境）— 生理感知反馈\n场景：深山求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Headlamp（头灯）\n\nEpisode 任务：\n围绕头灯进行讨论。讨论接近总结阶段时，Member DCY 准备一个 visibility / movement 相关补充观点，等待 leader 是否邀请其帮助收束该 item。\n\nDiscussion focus：\n- 头灯能否帮助小组在低光环境中安全移动？\n- 免手持照明是否有助于急救或搭建庇护所？\n- 电池寿命是否是限制？\n- 即使有头灯，小组是否仍应避免夜间移动？\n\nStaff 角色任务：\nMember ZHZ：普通参与者。可简短回应，不主动开启新的大段观点。\nMember DCY：目标成员。准备关于安全移动或免手持工作的补充观点。等待 leader 邀请。\nMember ZJR：普通参与者。可帮助讨论进入总结，但不抢 DCY 的核心补充点。\n\n注意：\n- 不要制造长时间沉默；通过“观点自然结束、准备进入总结”的方式形成重启机会。\n- 如果 leader 邀请 DCY，则该事件成功。"),
        P(3, 3, PromptPhase.Summary, "B3-T3-S Staff Episode 提示板 — Summary Stage（总结阶段）", "Staff Episode | Block 3 | Trial 3 | Summary Stage", "时间：240–300s\n讨论情境：Silence Context（讨论过渡与重启情境）— 生理感知反馈\n场景：深山求生\n反馈条件：生理感知反馈 trial\n\n本 episode 的 items：\n- Whistle（哨子）\n- Sleeping bag（睡袋）\n- Metal cup / cooking pot（金属杯/锅）\n- Headlamp（头灯）\n\nEpisode 任务：\n帮助 leader 总结本生理感知反馈 trial 的四个深山求生 items，不再开启新的主要 speaking-intention event。\n\nDiscussion focus：\n- 哪个 item 最支持求救信号？\n- 哪个 item 最支持保暖和休息？\n- 哪个 item 最支持水处理？\n- 哪个 item 最支持安全移动或夜间操作？\n\nStaff 角色任务：\nMember ZHZ：如被邀请，可确认讨论结果或简短补充。\nMember DCY：如被邀请，可确认讨论结果或简短补充。\nMember ZJR：如被邀请，可确认讨论结果或简短补充。\n\n注意：\n- 不要开启新的主要 speaking-intention event。\n- 如 leader 发出最后补充邀请，可自然回应。"),
    };

    private static PromptBoardContent P(int block, int trial, PromptPhase phase, string title, string meta, string body)
    {
        return new PromptBoardContent(block, trial, phase, title, meta, body);
    }
}

public class StaffEpisodePromptBoardBootstrap : MonoBehaviour
{
    private float nextCheckTime;

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
        StaffEpisodePromptBoard.TryCreateForCurrentScene();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextCheckTime)
        {
            return;
        }

        nextCheckTime = Time.unscaledTime + 0.5f;
        StaffEpisodePromptBoard.TryCreateForCurrentScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StaffEpisodePromptBoard.TryCreateForCurrentScene();
    }
}
