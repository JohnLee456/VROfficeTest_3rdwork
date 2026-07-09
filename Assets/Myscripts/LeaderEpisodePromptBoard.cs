using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-68)]
public class LeaderEpisodePromptBoard : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private const string ControlledSceneName = OfficeSceneSupport.OfficeLoggedInNoBot;
    private const int BoardSortingOrder = 930;
    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, 0.08f, 1.05f);
    [SerializeField] private Vector2 panelSize = new Vector2(780f, 560f);
    [SerializeField] private float worldScale = 0.00155f;
    [SerializeField] private KeyCode toggleKey = KeyCode.X;

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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForScene()
    {
        TryCreateForCurrentScene();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateBootstrap()
    {
        if (FindObjectOfType<LeaderEpisodePromptBoardBootstrap>() != null)
        {
            return;
        }

        GameObject bootstrap = new GameObject("Leader Episode Prompt Board Bootstrap");
        DontDestroyOnLoad(bootstrap);
        bootstrap.AddComponent<LeaderEpisodePromptBoardBootstrap>();
    }

    public static void TryCreateForCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!ShouldCreateForScene(activeScene) || FindObjectOfType<LeaderEpisodePromptBoard>() != null)
        {
            return;
        }

        GameObject manager = new GameObject("Leader Episode Prompt Board");
        manager.AddComponent<LeaderEpisodePromptBoard>();
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

        if (Input.GetKeyDown(toggleKey) || GetControllerToggleDown())
        {
            boardVisible = !boardVisible;
            if (boardRoot != null)
            {
                boardRoot.SetActive(boardVisible);
            }
        }

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

    private static bool GetControllerToggleDown()
    {
        return OfficeVrControllerInput.GetADown();
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
            return "Block " + currentBlockNumber + " / Trial " + currentTrialNumber + " / " + state + "   Ready / 等待 leader 按 Start   Toggle: X or controller A";
        }

        float elapsed = GetElapsedSeconds();
        float duration = GetCurrentEpisodeDuration();
        int remaining = Mathf.CeilToInt(Mathf.Max(0f, duration - elapsed));
        return "Block " + currentBlockNumber + " / Trial " + currentTrialNumber + " / " + state + "   Remaining: " + remaining + "s   Toggle: X or controller A";
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
        boardRoot = new GameObject("Leader Episode Prompt Board Canvas", typeof(RectTransform));

        Canvas canvas = boardRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = BoardSortingOrder;
        DashboardOverlayRendering.ConfigureCanvas(canvas, BoardSortingOrder);

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
            LoginSession.Role == LoginUserRole.Leader &&
            SceneManager.GetActiveScene().name == ControlledSceneName;
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
        P(1, 1, PromptPhase.Opening, "B1-T1-O Leader Episode 提示板 — Opening Phase（开场阶段）", "Leader Episode | Block 1 | Trial 1 | Opening Phase", "本 episode 需要做什么：\n介绍荒岛求生任务，并开始讨论是否应优先选择水过滤器。\n\n本 episode 的 items：\n- Water filter（水过滤器）\n\nLeader 讨论提示：\n- 饮用水是否是荒岛上最紧急的生存需求？\n- 如果小组找到不安全的淡水，水过滤器是否有帮助？\n- 如果只有海水可用，水过滤器是否仍然有用？\n- 它是否应排在工具、食物获取物品或求救物品之前？"),
        P(1, 1, PromptPhase.Episode1, "B1-T1-E1 Leader Episode 提示板 — Episode 1：单一成员进入请求", "Leader Episode | Block 1 | Trial 1 | Episode 1", "本 episode 需要做什么：\n解释或总结刀的重要性\n\n本 episode 的 items：\n- Knife（刀）\n\nLeader 讨论提示：\n- 刀是否可用于切割树枝、处理食物或制作简单工具？\n- 它是否有助于安全、急救或搭建庇护所？\n- 当它与绳子、捕鱼工具或庇护类物品配合使用时，价值是否更高？\n- 它应排在水过滤器之前还是之后？"),
        P(1, 1, PromptPhase.Episode2, "B1-T1-E2 Leader Episode 提示板 — Episode 2：竞争性进入请求", "Leader Episode | Block 1 | Trial 1 | Episode 2", "本 episode 需要做什么：\n讨论捕鱼工具。\n\n本 episode 的 items：\n- Fishing rod / fishing net（鱼竿/渔网）\n\nLeader 讨论提示：\n- 这个 item 能否解决长期食物需求？\n- 捕鱼是否需要技巧、时间和体力？\n- 在最初几天，食物是否不如水紧急？\n- 这个 item 是否足够可靠，可以排在较高位置？"),
        P(1, 1, PromptPhase.Episode3, "B1-T1-E3 Leader Episode 提示板 — Episode 3：重复单一进入请求", "Leader Episode | Block 1 | Trial 1 | Episode 3", "本 episode 需要做什么：\n解释或总结信号枪的求救价值\n\n本 episode 的 items：\n- Flare gun（信号枪）\n\nLeader 讨论提示：\n- 信号枪能否帮助向船只或飞机发出求救信号？\n- 它是否属于有限次数或一次性求救物品？\n- 它在夜间还是白天更有效？\n- 与水或工具等稳定生存物品相比，它的价值是更高还是更低？"),
        P(1, 1, PromptPhase.Summary, "B1-T1-S Leader Episode 提示板 — Summary Stage（总结阶段）", "Leader Episode | Block 1 | Trial 1 | Summary Stage", "本 episode 需要做什么：\n总结前四个荒岛求生 items，并形成阶段性排序或选择理由。\n\n本 episode 的 items：\n- Water filter（水过滤器）\n- Knife（刀）\n- Fishing rod / fishing net（鱼竿/渔网）\n- Flare gun（信号枪）\n\nLeader 讨论提示：\n- 哪个 item 应排在最高位置？\n- 哪些 items 支持即时生存？\n- 哪些 items 支持长期生存或求救？\n- 是否还有最后补充意见？"),
        P(1, 2, PromptPhase.Opening, "B1-T2-O Leader Episode 提示板 — Opening Phase（开场阶段）", "Leader Episode | Block 1 | Trial 2 | Opening Phase", "本 episode 需要做什么：\n讨论该 item 的必要性。\n\n本 episode 的 items：\n- First aid kit（急救包）\n\nLeader 讨论提示：\n- 该 item 能否解决当前生存阶段的关键问题？\n- 它与水、工具、火源、庇护或求救有什么关系？\n- 它是否应排在较高位置？"),
        P(1, 2, PromptPhase.Episode1, "B1-T2-E1 Leader Episode 提示板 — Episode 1：单一成员进入请求", "Leader Episode | Block 1 | Trial 2 | Episode 1", "本 episode 需要做什么：\n解释 Rope（绳子） 的用途。\n\n本 episode 的 items：\n- Rope（绳子）\n\nLeader 讨论提示：\n- 这个 item 是否具有多用途价值？\n- 它能否与其他 items 组合使用？\n- 它是否比单一用途 item 更值得优先选择？"),
        P(1, 2, PromptPhase.Episode2, "B1-T2-E2 Leader Episode 提示板 — Episode 2：竞争性进入请求", "Leader Episode | Block 1 | Trial 2 | Episode 2", "本 episode 需要做什么：\n讨论 Lighter / matches（打火机/火柴） 的重要性\n\n本 episode 的 items：\n- Lighter / matches（打火机/火柴）\n\nLeader 讨论提示：\n- 该 item 是否支持保暖、煮水、防护或求救？\n- 它在当前环境中是否可靠？\n- 它是否与其他 items 形成互补？"),
        P(1, 2, PromptPhase.Episode3, "B1-T2-E3 Leader Episode 提示板 — Episode 3：重复单一进入请求", "Leader Episode | Block 1 | Trial 2 | Episode 3", "本 episode 需要做什么：\n讨论 Tent / hammock（帐篷/吊床）。\n\n本 episode 的 items：\n- Tent / hammock（帐篷/吊床）\n\nLeader 讨论提示：\n- 该 item 是否支持庇护、休息或防护？\n- 它是否能提高长期生存质量？\n- 它与更紧急的水、火源或求救 items 相比如何？"),
        P(1, 2, PromptPhase.Summary, "B1-T2-S Leader Episode 提示板 — Summary Stage（总结阶段）", "Leader Episode | Block 1 | Trial 2 | Summary Stage", "本 episode 需要做什么：\n总结本 trial 的四个 items。\n\n本 episode 的 items：\n- First aid kit（急救包）\n- Rope（绳子）\n- Lighter / matches（打火机/火柴）\n- Tent / hammock（帐篷/吊床）\n\nLeader 讨论提示：\n- 哪个 item 最紧急？\n- 哪个 item 最灵活？\n- 哪个 item 支持长期生存？\n- 是否还有最后补充意见？"),
        P(1, 3, PromptPhase.Opening, "B1-T3-O Leader Episode 提示板 — Opening Phase（开场阶段）", "Leader Episode | Block 1 | Trial 3 | Opening Phase", "本 episode 需要做什么：\nLeader 介绍新的荒岛求生 item 集合，并开始讨论储水容器。\n\n本 episode 的 items：\n- Water container（储水容器）\n\nLeader 讨论提示：\n- 储水容器能否帮助储存雨水或过滤后的淡水？\n- 如果水源只是偶尔出现，储水是否重要？\n- 它是否需要与水过滤器或火源配合？"),
        P(1, 3, PromptPhase.Episode1, "B1-T3-E1 Leader Episode 提示板 — Episode 1：单一成员进入请求", "Leader Episode | Block 1 | Trial 3 | Episode 1", "本 episode 需要做什么：\n解释或总结砍刀的用途。\n\n本 episode 的 items：\n- Machete（砍刀）\n\nLeader 讨论提示：\n- 砍刀能否切割树枝、清理植被或准备庇护材料？\n- 它是否比小刀更有力量但精细度更低？\n- 它是否因为多用途而应排在较高位置？"),
        P(1, 3, PromptPhase.Episode2, "B1-T3-E2 Leader Episode 提示板 — Episode 2：竞争性进入请求", "Leader Episode | Block 1 | Trial 3 | Episode 2", "本 episode 需要做什么：\n讨论信号镜。\n\n本 episode 的 items：\n- Signal mirror（信号镜）\n\nLeader 讨论提示：\n- 信号镜能否在白天帮助吸引船只或飞机？\n- 与信号枪相比，它是否可以重复使用？\n- 它是否依赖阳光和能见度？"),
        P(1, 3, PromptPhase.Episode3, "B1-T3-E3 Leader Episode 提示板 — Episode 3：重复单一进入请求", "Leader Episode | Block 1 | Trial 3 | Episode 3", "本 episode 需要做什么：\n解释或总结蚊帐的防护价值。\n\n本 episode 的 items：\n- Mosquito net（蚊帐）\n\nLeader 讨论提示：\n- 蚊帐能否防虫并改善睡眠质量？\n- 防止叮咬是否能减少感染或不适？\n- 它是否不如水、火源或求救 items 紧急？"),
        P(1, 3, PromptPhase.Summary, "B1-T3-S Leader Episode 提示板 — Summary Stage（总结阶段）", "Leader Episode | Block 1 | Trial 3 | Summary Stage", "本 episode 需要做什么：\n总结本生理感知反馈 trial 中的四个 items。\n\n本 episode 的 items：\n- Water container（储水容器）\n- Machete（砍刀）\n- Signal mirror（信号镜）\n- Mosquito net（蚊帐）\n\nLeader 讨论提示：\n- 哪个 item 最能支持水资源管理？\n- 哪个 item 是最灵活的工具？\n- 哪个 item 最能支持求救？\n- 哪个 item 支持防护和休息？"),
        P(2, 1, PromptPhase.Opening, "B2-T1-O Leader Episode 提示板 — Opening Phase（开场阶段）", "Leader Episode | Block 2 | Trial 1 | Opening Phase", "本 episode 需要做什么：\n介绍沙漠求生任务，并把讨论交给成员。\n\n本 episode 的 items：\n- Cosmetic mirror（化妆镜/信号镜）\n\nLeader 讨论提示：\n- 该 item 是否能帮助小组在沙漠中生存或求救？\n- 它的价值是否取决于留在坠落点还是主动移动？\n- 它与水、遮阳、信号或工具类 items 相比如何？"),
        P(2, 1, PromptPhase.Episode1, "B2-T1-E1 Leader Episode 提示板 — Episode 1：双人讨论中的被压制进入请求", "Leader Episode | Block 2 | Trial 1 | Episode 1", "本 episode 需要做什么：\n观察小组成员讨论，并在适当的时候邀请小组成员发言。\n\n本 episode 的 items：\n- Top coat per person（每人一件外套）\n\nLeader 讨论提示：\n- 该 item 的主要用途是什么？\n- 它是否有明显限制或风险？\n- 它是否应排在水或求救信号类 items 之前？"),
        P(2, 1, PromptPhase.Episode2, "B2-T1-E2 Leader Episode 提示板 — Episode 2：主导发言者压制目标成员", "Leader Episode | Block 2 | Trial 1 | Episode 2", "本 episode 需要做什么：\n观察小组成员讨论，并在适当的时候邀请小组成员发言。\n\n本 episode 的 items：\n- Water per person（每人一份水）\n\nLeader 讨论提示：\n- 该 item 是否解决沙漠中的关键风险？\n- 如果小组选择移动或等待，它的价值是否变化？\n- 它与其他 items 的优先级关系如何？"),
        P(2, 1, PromptPhase.Episode3, "B2-T1-E3 Leader Episode 提示板 — Episode 3：重复被压制进入事件", "Leader Episode | Block 2 | Trial 1 | Episode 3", "本 episode 需要做什么：\n观察小组成员讨论，并在适当的时候邀请小组成员发言。\n\n本 episode 的 items：\n- Flashlight（手电筒）\n\nLeader 讨论提示：\n- 该 item 是否更适合短期生存还是长期生存？\n- 它是否依赖特定策略，例如移动、等待或求救？\n- 是否存在不确定性或使用限制？"),
        P(2, 1, PromptPhase.Summary, "B2-T1-S Leader Episode 提示板 — Summary Stage（总结阶段）", "Leader Episode | Block 2 | Trial 1 | Summary Stage", "本 episode 需要做什么：\n观察小组成员讨论与总结，并在适当的时候邀请小组成员发言。\n\n本 episode 的 items：\n- Cosmetic mirror（化妆镜/信号镜）\n- Top coat per person（每人一件外套）\n- Water per person（每人一份水）\n- Flashlight（手电筒）\n\nLeader 讨论提示：\n- 哪个 item 最支持即时生存？\n- 哪个 item 最支持求救或安全？\n- 哪个 item 的价值最依赖小组策略？\n- 是否还有最后补充意见？"),
        P(2, 2, PromptPhase.Opening, "B2-T2-O Leader Episode 提示板 — Opening Phase（开场阶段）", "Leader Episode | Block 2 | Trial 2 | Opening Phase", "本 episode 需要做什么：\n介绍沙漠求生任务，并把讨论交给成员。\n\n本 episode 的 items：\n- Parachute（降落伞）\n\nLeader 讨论提示：\n- 该 item 是否能帮助小组在沙漠中生存或求救？\n- 它的价值是否取决于留在坠落点还是主动移动？\n- 它与水、遮阳、信号或工具类 items 相比如何？"),
        P(2, 2, PromptPhase.Episode1, "B2-T2-E1 Leader Episode 提示板 — Episode 1：双人讨论中的被压制进入请求", "Leader Episode | Block 2 | Trial 2 | Episode 1", "本 episode 需要做什么：\n观察小组成员讨论，并在适当的时候邀请小组成员发言。\n\n本 episode 的 items：\n- Jack knife（折叠刀）\n\nLeader 讨论提示：\n- 该 item 的主要用途是什么？\n- 它是否有明显限制或风险？\n- 它是否应排在水或求救信号类 items 之前？"),
        P(2, 2, PromptPhase.Episode2, "B2-T2-E2 Leader Episode 提示板 — Episode 2：主导发言者压制目标成员", "Leader Episode | Block 2 | Trial 2 | Episode 2", "本 episode 需要做什么：\n观察小组成员讨论，并在适当的时候邀请小组成员发言。\n\n本 episode 的 items：\n- Sunglasses（太阳镜）\n\nLeader 讨论提示：\n- 该 item 是否解决沙漠中的关键风险？\n- 如果小组选择移动或等待，它的价值是否变化？\n- 它与其他 items 的优先级关系如何？"),
        P(2, 2, PromptPhase.Episode3, "B2-T2-E3 Leader Episode 提示板 — Episode 3：重复被压制进入事件", "Leader Episode | Block 2 | Trial 2 | Episode 3", "本 episode 需要做什么：\n观察小组成员讨论，并在适当的时候邀请小组成员发言。\n\n本 episode 的 items：\n- Map / compass（地图/指南针）\n\nLeader 讨论提示：\n- 该 item 是否更适合短期生存还是长期生存？\n- 它是否依赖特定策略，例如移动、等待或求救？\n- 是否存在不确定性或使用限制？"),
        P(2, 2, PromptPhase.Summary, "B2-T2-S Leader Episode 提示板 — Summary Stage（总结阶段）", "Leader Episode | Block 2 | Trial 2 | Summary Stage", "本 episode 需要做什么：\n观察小组成员讨论与总结，并在适当的时候邀请小组成员发言。\n\n本 episode 的 items：\n- Parachute（降落伞）\n- Jack knife（折叠刀）\n- Sunglasses（太阳镜）\n- Map / compass（地图/指南针）\n\nLeader 讨论提示：\n- 哪个 item 最支持即时生存？\n- 哪个 item 最支持求救或安全？\n- 哪个 item 的价值最依赖小组策略？\n- 是否还有最后补充意见？"),
        P(2, 3, PromptPhase.Opening, "B2-T3-O Leader Episode 提示板 — Opening Phase（开场阶段）", "Leader Episode | Block 2 | Trial 3 | Opening Phase", "本 episode 需要做什么：\n介绍沙漠求生任务，并把讨论交给成员。\n\n本 episode 的 items：\n- Plastic raincoat（塑料雨衣）\n\nLeader 讨论提示：\n- 该 item 是否能帮助小组在沙漠中生存或求救？\n- 它的价值是否取决于留在坠落点还是主动移动？\n- 它与水、遮阳、信号或工具类 items 相比如何？"),
        P(2, 3, PromptPhase.Episode1, "B2-T3-E1 Leader Episode 提示板 — Episode 1：双人讨论中的被压制进入请求", "Leader Episode | Block 2 | Trial 3 | Episode 1", "本 episode 需要做什么：\n观察小组成员讨论，并在适当的时候邀请小组成员发言。\n\n本 episode 的 items：\n- Pistol（手枪）\n\nLeader 讨论提示：\n- 该 item 的主要用途是什么？\n- 它是否有明显限制或风险？\n- 它是否应排在水或求救信号类 items 之前？"),
        P(2, 3, PromptPhase.Episode2, "B2-T3-E2 Leader Episode 提示板 — Episode 2：主导发言者压制目标成员", "Leader Episode | Block 2 | Trial 3 | Episode 2", "本 episode 需要做什么：\n观察小组成员讨论，并在适当的时候邀请小组成员发言。\n\n本 episode 的 items：\n- Alcohol bottle（酒精瓶）\n\nLeader 讨论提示：\n- 该 item 是否解决沙漠中的关键风险？\n- 如果小组选择移动或等待，它的价值是否变化？\n- 它与其他 items 的优先级关系如何？"),
        P(2, 3, PromptPhase.Episode3, "B2-T3-E3 Leader Episode 提示板 — Episode 3：重复被压制进入事件", "Leader Episode | Block 2 | Trial 3 | Episode 3", "本 episode 需要做什么：\n观察小组成员讨论，并在适当的时候邀请小组成员发言。\n\n本 episode 的 items：\n- Desert animals guidebook（沙漠动物指南）\n\nLeader 讨论提示：\n- 该 item 是否更适合短期生存还是长期生存？\n- 它是否依赖特定策略，例如移动、等待或求救？\n- 是否存在不确定性或使用限制？"),
        P(2, 3, PromptPhase.Summary, "B2-T3-S Leader Episode 提示板 — Summary Stage（总结阶段）", "Leader Episode | Block 2 | Trial 3 | Summary Stage", "本 episode 需要做什么：\n观察小组成员讨论，并在适当的时候邀请小组成员发言。\n\n本 episode 的 items：\n- Plastic raincoat（塑料雨衣）\n- Pistol（手枪）\n- Alcohol bottle（酒精瓶）\n- Desert animals guidebook（沙漠动物指南）\n\nLeader 讨论提示：\n- 哪个 item 最支持即时生存？\n- 哪个 item 最支持求救或安全？\n- 哪个 item 的价值最依赖小组策略？\n- 是否还有最后补充意见？"),
        P(3, 1, PromptPhase.Opening, "B3-T1-O Leader Episode 提示板 — Opening Phase（开场阶段）", "Leader Episode | Block 3 | Trial 1 | Opening Phase", "本 episode 需要做什么：\n介绍深山求生任务，并开始讨论火柴/打火机是否应被优先选择。\n\n本 episode 的 items：\n- Matches / lighter（火柴/打火机）\n\nLeader 讨论提示：\n- 火源是否有助于保暖、煮水、求救信号或夜间安全？\n- 在深山或寒冷环境中，火源是否比食物更紧急？\n- 火源是否需要干燥材料或庇护所配合？\n- 它是否应排在较高位置？"),
        P(3, 1, PromptPhase.Episode1, "B3-T1-E1 Leader Episode 提示板 — Episode 1：讨论过渡后的单一补充", "Leader Episode | Block 3 | Trial 1 | Episode 1", "本 episode 需要做什么：\n引导小组讨论塑料布/厚帆布，并在讨论自然收束时，邀请最适合补充观点的成员继续发言。\n\n本 episode 的 items：\n- Polythene sheeting / heavy canvas（塑料布/厚帆布）\n\nLeader 讨论提示：\n- 塑料布/厚帆布能否防风、防雨、防寒？\n- 它是否可作为临时庇护所、地垫或信号标记？\n- 它是否能与火源配合，提高生存质量？\n- 如果讨论开始变得停滞，谁最适合从 shelter 角度继续推进？"),
        P(3, 1, PromptPhase.Episode2, "B3-T1-E2 Leader Episode 提示板 — Episode 2：讨论过渡中的竞争性补充", "Leader Episode | Block 3 | Trial 1 | Episode 2", "本 episode 需要做什么：\n引导小组讨论急救包，并在两个成员都可能补充时，邀请更适合先发言的成员继续讨论。\n\n本 episode 的 items：\n- First-aid kit（急救包）\n\nLeader 讨论提示：\n- 急救包是否能处理割伤、摔伤、冻伤或轻伤？\n- 如果当前没有人受伤，它是否仍然值得高排序？\n- 它与火源、庇护、求救 items 相比优先级如何？\n- 如果有两位成员都可能补充，谁的观点更适合先听？"),
        P(3, 1, PromptPhase.Episode3, "B3-T1-E3 Leader Episode 提示板 — Episode 3：总结前的定向补充", "Leader Episode | Block 3 | Trial 1 | Episode 3", "本 episode 需要做什么：\n引导小组讨论信号弹，并在进入总结前邀请最适合补充 rescue 观点的成员发言。\n\n本 episode 的 items：\n- Signal flares（信号弹）\n\nLeader 讨论提示：\n- 信号弹能否有效吸引救援队注意？\n- 它在夜间还是白天更有用？\n- 它是否是有限使用 item？\n- 进入总结前，谁最适合补充 rescue 相关观点？"),
        P(3, 1, PromptPhase.Summary, "B3-T1-S Leader Episode 提示板 — Summary Stage（总结阶段）", "Leader Episode | Block 3 | Trial 1 | Summary Stage", "本 episode 需要做什么：\n总结本 trial 的四个深山求生 items，并形成阶段性排序或选择理由。\n\n本 episode 的 items：\n- Matches / lighter（火柴/打火机）\n- Polythene sheeting / heavy canvas（塑料布/厚帆布）\n- First-aid kit（急救包）\n- Signal flares（信号弹）\n\nLeader 讨论提示：\n- 哪个 item 最支持即时生存？\n- 哪个 item 最支持保暖或庇护？\n- 哪个 item 最支持求救？\n- 是否还有最后补充意见？"),
        P(3, 2, PromptPhase.Opening, "B3-T2-O Leader Episode 提示板 — Opening Phase（开场阶段）", "Leader Episode | Block 3 | Trial 2 | Opening Phase", "本 episode 需要做什么：\n介绍第二组深山求生 items，并开始讨论瓶装水的短期价值。\n\n本 episode 的 items：\n- Bottled water（瓶装水）\n\nLeader 讨论提示：\n- 深山环境中找水是容易还是困难？\n- 瓶装水是否对短期生存重要？\n- 它是否需要与火源或过滤方式配合？\n- 补水是否比保暖或庇护更紧急？"),
        P(3, 2, PromptPhase.Episode1, "B3-T2-E1 Leader Episode 提示板 — Episode 1：讨论过渡后的单一补充", "Leader Episode | Block 3 | Trial 2 | Episode 1", "本 episode 需要做什么：\n引导小组讨论工具箱/手斧/刀，并在讨论自然收束时，邀请最适合补充工具价值的成员继续发言。\n\n本 episode 的 items：\n- Toolbox / hand axe / knife（工具箱/手斧/刀）\n\nLeader 讨论提示：\n- 工具是否能帮助砍树枝、搭建庇护所或修理设备？\n- 工具是否比单一用途 item 更灵活？\n- 它们是否太重，不便携带？\n- 如果讨论开始停滞，谁最适合从工具使用角度继续推进？"),
        P(3, 2, PromptPhase.Episode2, "B3-T2-E2 Leader Episode 提示板 — Episode 2：讨论过渡中的竞争性补充", "Leader Episode | Block 3 | Trial 2 | Episode 2", "本 episode 需要做什么：\n引导小组讨论额外衣物/毯子，并在两个成员都可能补充时，邀请更适合先发言的成员继续讨论。\n\n本 episode 的 items：\n- Extra clothing / blanket（额外衣物/毯子）\n\nLeader 讨论提示：\n- 额外衣物或毯子能否防止失温？\n- 夜间保暖是否比白天行动更重要？\n- 它是否能与火源和庇护所配合？\n- 如果有两位成员都可能补充，谁的观点更适合先听？"),
        P(3, 2, PromptPhase.Episode3, "B3-T2-E3 Leader Episode 提示板 — Episode 3：总结前的定向补充", "Leader Episode | Block 3 | Trial 2 | Episode 3", "本 episode 需要做什么：\n引导小组讨论巧克力/高能量食物，并在进入总结前邀请最适合补充能量维持观点的成员发言。\n\n本 episode 的 items：\n- Chocolate / high-energy food（巧克力/高能量食物）\n\nLeader 讨论提示：\n- 高能量食物能否维持体力和体温？\n- 食物是否不如水、火源或庇护紧急？\n- 巧克力是否轻便且易于分配？\n- 进入总结前，谁最适合补充 energy 相关观点？"),
        P(3, 2, PromptPhase.Summary, "B3-T2-S Leader Episode 提示板 — Summary Stage（总结阶段）", "Leader Episode | Block 3 | Trial 2 | Summary Stage", "本 episode 需要做什么：\n总结本 trial 的四个深山求生 items，并形成阶段性排序或选择理由。\n\n本 episode 的 items：\n- Bottled water（瓶装水）\n- Toolbox / hand axe / knife（工具箱/手斧/刀）\n- Extra clothing / blanket（额外衣物/毯子）\n- Chocolate / high-energy food（巧克力/高能量食物）\n\nLeader 讨论提示：\n- 哪个 item 最支持补水？\n- 哪个 item 最灵活？\n- 哪个 item 最支持保暖？\n- 是否还有最后补充意见？"),
        P(3, 3, PromptPhase.Opening, "B3-T3-O Leader Episode 提示板 — Opening Phase（开场阶段）", "Leader Episode | Block 3 | Trial 3 | Opening Phase", "本 episode 需要做什么：\n介绍本 trial 的生理感知反馈条件，并开始讨论哨子的求救价值。\n\n本 episode 的 items：\n- Whistle（哨子）\n\nLeader 讨论提示：\n- 哨子能否在不消耗太多体力的情况下帮助求救？\n- 在雾、森林或低能见度环境中，它是否有用？\n- 声音在深山环境中传播是否足够远？\n- 它是否应排在视觉信号工具之前？"),
        P(3, 3, PromptPhase.Episode1, "B3-T3-E1 Leader Episode 提示板 — Episode 1：讨论过渡后的单一补充", "Leader Episode | Block 3 | Trial 3 | Episode 1", "本 episode 需要做什么：\n引导小组讨论睡袋，并在讨论自然收束时，邀请最适合补充保暖观点的成员继续发言。\n\n本 episode 的 items：\n- Sleeping bag（睡袋）\n\nLeader 讨论提示：\n- 睡袋能否在夜间防止失温？\n- 在深山环境中，保暖是否比食物更紧急？\n- 没有帐篷时，睡袋是否仍然有用？\n- 如果讨论开始停滞，谁最适合从保暖角度继续推进？"),
        P(3, 3, PromptPhase.Episode2, "B3-T3-E2 Leader Episode 提示板 — Episode 2：讨论过渡中的竞争性补充", "Leader Episode | Block 3 | Trial 3 | Episode 2", "本 episode 需要做什么：\n引导小组讨论金属杯/锅，并在两个成员都可能补充时，邀请更适合先发言的成员继续讨论。\n\n本 episode 的 items：\n- Metal cup / cooking pot（金属杯/锅）\n\nLeader 讨论提示：\n- 金属杯或锅能否用于煮水？\n- 它是否能帮助融雪或准备热饮？\n- 它是否只有在小组也有火源时才有用？\n- 如果有两位成员都可能补充，谁的观点更适合先听？"),
        P(3, 3, PromptPhase.Episode3, "B3-T3-E3 Leader Episode 提示板 — Episode 3：总结前的定向补充", "Leader Episode | Block 3 | Trial 3 | Episode 3", "本 episode 需要做什么：\n引导小组讨论头灯，并在进入总结前邀请最适合补充安全移动观点的成员发言。\n\n本 episode 的 items：\n- Headlamp（头灯）\n\nLeader 讨论提示：\n- 头灯能否帮助小组在低光环境中安全移动？\n- 免手持照明是否有助于急救或搭建庇护所？\n- 电池寿命是否是限制？\n- 进入总结前，谁最适合补充 visibility / movement 相关观点？"),
        P(3, 3, PromptPhase.Summary, "B3-T3-S Leader Episode 提示板 — Summary Stage（总结阶段）", "Leader Episode | Block 3 | Trial 3 | Summary Stage", "本 episode 需要做什么：\n总结本生理感知反馈 trial 的四个深山求生 items，并形成阶段性排序或选择理由。\n\n本 episode 的 items：\n- Whistle（哨子）\n- Sleeping bag（睡袋）\n- Metal cup / cooking pot（金属杯/锅）\n- Headlamp（头灯）\n\nLeader 讨论提示：\n- 哪个 item 最支持求救信号？\n- 哪个 item 最支持保暖和休息？\n- 哪个 item 最支持水处理？\n- 哪个 item 最支持安全移动或夜间操作？"),
    };

    private static PromptBoardContent P(int block, int trial, PromptPhase phase, string title, string meta, string body)
    {
        return new PromptBoardContent(block, trial, phase, title, meta, body);
    }
}

public class LeaderEpisodePromptBoardBootstrap : MonoBehaviour
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
        LeaderEpisodePromptBoard.TryCreateForCurrentScene();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextCheckTime)
        {
            return;
        }

        nextCheckTime = Time.unscaledTime + 0.5f;
        LeaderEpisodePromptBoard.TryCreateForCurrentScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LeaderEpisodePromptBoard.TryCreateForCurrentScene();
    }
}
