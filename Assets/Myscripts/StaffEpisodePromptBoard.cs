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
    private const float EpisodeOneDuration = 60f;
    private const float LaterEpisodeDuration = 70f;

    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, 0.08f, 1.05f);
    [SerializeField] private Vector2 panelSize = new Vector2(720f, 500f);
    [SerializeField] private float worldScale = 0.00155f;
    [SerializeField] private KeyCode toggleKey = KeyCode.N;

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
    private bool boardVisible = true;
    private static Font cjkFont;

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

        RefreshBoard();
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != Block1EpisodeSync.EpisodeStartedEventCode)
        {
            return;
        }

        int blockNumber;
        int trialNumber;
        int episodeNumber;
        double startTime;
        if (Block1EpisodeSync.TryParsePayload(photonEvent.CustomData, out blockNumber, out trialNumber, out episodeNumber, out startTime))
        {
            ApplyEpisodeStart(blockNumber, trialNumber, episodeNumber, startTime);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(Block1EpisodeSync.BlockKey) ||
            propertiesThatChanged.ContainsKey(Block1EpisodeSync.TrialKey) ||
            propertiesThatChanged.ContainsKey(Block1EpisodeSync.EpisodeKey) ||
            propertiesThatChanged.ContainsKey(Block1EpisodeSync.EpisodeStartTimeKey))
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
        int blockNumber;
        int trialNumber;
        int episodeNumber;
        double startTime;
        if (Block1EpisodeSync.TryReadRoomState(out blockNumber, out trialNumber, out episodeNumber, out startTime))
        {
            ApplyEpisodeStart(blockNumber, trialNumber, episodeNumber, startTime);
        }
    }

    private void ApplyEpisodeStart(int blockNumber, int trialNumber, int episodeNumber, double startTime)
    {
        currentBlockNumber = Mathf.Clamp(blockNumber, 1, 3);
        currentTrialNumber = Mathf.Clamp(trialNumber, 1, 2);
        currentEpisodeNumber = Mathf.Clamp(episodeNumber, 1, 3);
        episodeStartTime = startTime;
        hasEpisodeStart = true;
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
        titleText.text = prompt.Title;
        metaText.text = prompt.Meta;
        contentText.text = AddPersonalRoleLine(prompt.Body);
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

        if (currentEpisodeNumber == 3 && GetElapsedSeconds() >= GetCurrentEpisodeDuration())
        {
            return FindPrompt(currentBlockNumber, currentTrialNumber, PromptPhase.Summary);
        }

        return FindPrompt(currentBlockNumber, currentTrialNumber, (PromptPhase)currentEpisodeNumber);
    }

    private string GetFooterText()
    {
        if (!hasEpisodeStart)
        {
            return "Waiting for leader to start Episode 1 / 等待 leader 开始 Episode 1";
        }

        float elapsed = GetElapsedSeconds();
        float duration = GetCurrentEpisodeDuration();
        int remaining = Mathf.CeilToInt(Mathf.Max(0f, duration - elapsed));
        string state = currentEpisodeNumber == 3 && elapsed >= duration ? "Summary / 总结" : "Episode " + currentEpisodeNumber;
        return "Block " + currentBlockNumber + " / Trial " + currentTrialNumber + " / " + state + "   Remaining: " + remaining + "s   Toggle: N or controller B";
    }

    private float GetElapsedSeconds()
    {
        if (!hasEpisodeStart)
        {
            return 0f;
        }

        return Mathf.Max(0f, (float)((PhotonNetwork.InRoom ? PhotonNetwork.Time : Time.time) - episodeStartTime));
    }

    private float GetCurrentEpisodeDuration()
    {
        return currentEpisodeNumber == 1 ? EpisodeOneDuration : LaterEpisodeDuration;
    }

    private string AddPersonalRoleLine(string body)
    {
        string label = GetCurrentParticipantLabel();
        return "Your avatar / 你的角色: " + LoginSession.AvatarName + " = " + label + "\n\n" + body;
    }

    private static string GetCurrentParticipantLabel()
    {
        if (LoginSession.AvatarName == "ZHZ")
        {
            return "A";
        }

        if (LoginSession.AvatarName == "DCY")
        {
            return "B";
        }

        if (LoginSession.AvatarName == "ZJR")
        {
            return "C";
        }

        return "Unknown";
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

        Image rule = CreateImage("Top Rule", panelRect, new Vector2(0f, 202f), new Vector2(610f, 3f), new Color(0.34f, 0.62f, 0.72f, 0.92f));
        rule.raycastTarget = false;

        titleText = CreateText("Title", panelRect, string.Empty, new Vector2(0f, 220f), new Vector2(640f, 40f), 24, FontStyle.Bold, TextAnchor.MiddleCenter, false);
        metaText = CreateText("Meta", panelRect, string.Empty, new Vector2(0f, 174f), new Vector2(640f, 42f), 16, FontStyle.Normal, TextAnchor.MiddleCenter, true);
        contentText = CreateText("Content", panelRect, string.Empty, new Vector2(0f, -12f), new Vector2(640f, 320f), 16, FontStyle.Normal, TextAnchor.UpperLeft, true);
        footerText = CreateText("Footer", panelRect, string.Empty, new Vector2(0f, -222f), new Vector2(640f, 28f), 14, FontStyle.Normal, TextAnchor.MiddleCenter, false);

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
        P(1, 1, PromptPhase.Opening, "B1-T1-O Opening / 开场", "Time 0-40s | Speaking Context | Island survival / 荒岛求生", "Topic / 主题: start the island survival discussion; discuss whether a water filter should be prioritized. / 开始荒岛求生讨论；讨论水过滤器是否应优先。\nMain task / 任务: let the leader open and state initial ranking criteria. / 让 leader 开启讨论并给出判断标准。\nStaff / 成员: briefly respond, do not take the leader's opening turn. / 简短回应，不抢 leader 开场。"),
        P(1, 1, PromptPhase.Episode1, "B1-T1-E1 Clear Single Entry / 单一进入请求", "Time 40-100s | Item: Knife / 刀 | Target: A", "Topic / 主题: leader explains the importance of the knife. / leader 正在解释 knife 的重要性。\nTarget assignment / 角色: A target; B/C normal participants. / A 是 target，B/C 正常参与。\nStaff action / 行动: A prepares a natural extra point, such as tool-making or food processing value; wait for invitation. B/C listen and give short responses. / A 准备补充观点并等待邀请；B/C 正常听和简短回应。\nExpected / 预期: inviting A is expected; no invitation is missed. / 邀请 A 为预期路径，未邀请为 missed。"),
        P(1, 1, PromptPhase.Episode2, "B1-T1-E2 Competing Entry / 竞争进入请求", "Time 100-170s | Item: Fishing item / 捕鱼工具 | Primary: B | Secondary: C", "Topic / 主题: two members have extra points about the fishing item. / 两名 members 都有关于 fishing item 的补充。\nTarget assignment / 角色: B higher speaking-intention target; C lower competitor; A normal. / B 是 primary target，C 是较低竞争者，A 正常参与。\nStaff action / 行动: B prepares the main point about long-term food; C prepares a secondary uncertainty point; A stays normal. / B 准备主要观点，C 准备次要观点，A 不竞争。\nExpected / 预期: inviting B is expected; inviting C is target mismatch; no invitation is missed. / 邀请 B 为预期，邀请 C 为 mismatch，未邀请为 missed。"),
        P(1, 1, PromptPhase.Episode3, "B1-T1-E3 Repeated Single Entry / 重复单一请求", "Time 170-240s | Item: Flare gun / 信号枪 | Target: C", "Topic / 主题: leader summarizes or explains the rescue value of the flare gun. / leader 总结或解释 flare gun 的求救价值。\nTarget assignment / 角色: C target; A/B normal participants. / C 是 target，A/B 正常参与。\nStaff action / 行动: C prepares a point about routes, rescue visibility, and whether flare gun is more valuable than stable water; A/B listen. / C 准备补充观点，A/B 正常听。\nExpected / 预期: inviting C is expected; no invitation is missed. / 邀请 C 为预期，未邀请为 missed。"),
        P(1, 1, PromptPhase.Summary, "B1-T1-S Summary / 总结", "Time 240-300s | Items: water filter, knife, fishing item, flare gun", "Topic / 主题: summarize the first four island survival items. / 总结前 4 个荒岛求生 items。\nStaff action / 行动: follow leader's wrap-up; only add a final point if invited. / 配合 leader 收尾，被邀请时再补充。\nGoal / 目标: close the trial without starting a new event. / 不再开启新事件。"),
        P(1, 2, PromptPhase.Opening, "B1-T2-O Opening / 开场", "Time 0-40s | Speaking Context | Island survival / 荒岛求生", "Topic / 主题: move to the later four island items; discuss first aid kit. / 进入后 4 个 items，讨论 first aid kit。\nMain task / 任务: let leader reorganize the discussion and start the new item subset. / 让 leader 重新组织并开始新 item subset。\nStaff / 成员: brief responses only; no formal event. / 简短回应，不触发正式事件。"),
        P(1, 2, PromptPhase.Episode1, "B1-T2-E1 Clear Single Entry / 单一进入请求", "Time 40-100s | Item: Rope / 绳子 | Target: B", "Topic / 主题: leader explains uses of rope; B prepares a supplement. / leader 解释 rope 的用途，B 准备补充。\nTarget assignment / 角色: B target; A/C normal. / B 是 target，A/C 正常参与。\nStaff action / 行动: B waits for invitation and discusses tying, climbing, shelter, or rescue uses; A/C listen. / B 等待邀请并围绕绳子用途表达，A/C 正常听。\nExpected / 预期: leader should hand the turn to B. / leader 应把话轮交给 B。"),
        P(1, 2, PromptPhase.Episode2, "B1-T2-E2 Competing Entry / 竞争进入请求", "Time 100-170s | Item: Lighter/matches / 火源 | Primary: C | Secondary: A", "Topic / 主题: two members can add points about fire source value. / 两名成员可补充火源价值。\nTarget assignment / 角色: C primary target; A secondary competitor; B normal. / C 是 primary，A 是 secondary，B 正常参与。\nStaff action / 行动: C prepares the main multi-function point: boiling water, warmth, rescue; A prepares reliability concerns; B does not compete. / C 准备主要观点，A 准备次要观点，B 不竞争。\nExpected / 预期: inviting C is expected; inviting A is mismatch. / 邀请 C 为预期，邀请 A 为 mismatch。"),
        P(1, 2, PromptPhase.Episode3, "B1-T2-E3 Repeated Single Entry / 重复单一请求", "Time 170-240s | Item: Tent/hammock / 帐篷或吊床 | Target: A", "Topic / 主题: leader advances or summarizes shelter. / leader 推进或总结 shelter item。\nTarget assignment / 角色: A target; B/C normal. / A 是 target，B/C 正常参与。\nStaff action / 行动: A waits for invitation and discusses cover, rest, insects, rain, and tradeoffs. / A 等待邀请后表达遮蔽、休息、防虫、防雨等观点。\nExpected / 预期: repeated single target event with A. / 重复单一 target 事件，target 为 A。"),
        P(1, 2, PromptPhase.Summary, "B1-T2-S Summary / 总结", "Time 240-300s | Items: first aid kit, rope, lighter/matches, tent/hammock", "Topic / 主题: summarize the later four island survival items. / 总结后 4 个荒岛求生 items。\nStaff action / 行动: follow leader's wrap-up; no new event. / 配合 leader 收尾，不开启新事件。"),

        P(2, 1, PromptPhase.Opening, "B2-T1-O Opening / 开场", "Time 0-40s | Listening Context | Desert survival / 沙漠求生", "Topic / 主题: leader introduces desert survival and hands cosmetic mirror discussion to members. / leader 引入沙漠求生，把 cosmetic mirror 交给 members 讨论。\nStaff / 成员: A/B start the discussion; C stays ordinary and does not grab the turn. / A/B 先讨论，C 普通参与不抢话。"),
        P(2, 1, PromptPhase.Episode1, "B2-T1-E1 Suppressed Entry / 被忽视进入", "Time 40-100s | Item: Coat / 外套 | Target: C", "Topic / 主题: A and B discuss coat; C has a point but has not had a chance. / A 和 B 正在讨论外套，C 有观点但未获机会。\nRole assignment / 角色: A/B active speakers; C target. / A/B 是 active speakers，C 是 target。\nStaff action / 行动: C prepares the shade and water-loss point; A/B maintain the two-person discussion. / C 准备遮阳和减少水分流失观点，A/B 维持双边讨论。\nExpected / 预期: leader invites C to join. / leader 邀请 C 加入。"),
        P(2, 1, PromptPhase.Episode2, "B2-T1-E2 Dominant Speaker / 主导者压制 target", "Time 100-170s | Item: Water / 水 | Target: B", "Topic / 主题: A speaks a lot about water; B has not had a chance. / A 在 water 上说得较多，B 尚未表达。\nRole assignment / 角色: A dominant speaker; B target; C ordinary responder. / A 是 dominant speaker，B 是 target，C 普通回应。\nStaff action / 行动: B prepares a balancing point; A continues more actively; C keeps responses short. / B 准备平衡观点，A 多说，C 简短回应。\nExpected / 预期: leader recognizes imbalance and invites B. / leader 识别失衡并邀请 B。"),
        P(2, 1, PromptPhase.Episode3, "B2-T1-E3 Repeated Suppressed Entry / 重复被忽视进入", "Time 170-240s | Item: Flashlight / 手电筒 | Target: A", "Topic / 主题: B/C discuss flashlight; A has an unexpressed point. / B/C 讨论 flashlight，A 有未表达观点。\nRole assignment / 角色: B/C active speakers; A target. / B/C 是 active speakers，A 是 target。\nStaff action / 行动: A prepares a point about night signaling or movement; B/C keep discussion going. / A 准备夜间求救或行动观点，B/C 继续讨论。\nExpected / 预期: leader invites A. / leader 邀请 A。"),
        P(2, 1, PromptPhase.Summary, "B2-T1-S Summary / 总结", "Time 240-300s | Items: mirror, coat, water, flashlight", "Topic / 主题: summarize the first four desert survival items. / 总结前 4 个沙漠求生 items。\nStaff action / 行动: accept leader's final confirmation or invitation; do not start a new event. / 接受 leader 最后确认或补充邀请，不开新事件。"),
        P(2, 2, PromptPhase.Opening, "B2-T2-O Opening / 开场", "Time 0-40s | Listening Context | Desert survival / 沙漠求生", "Topic / 主题: leader introduces later four desert items and lets members discuss parachute. / leader 引入后 4 个 desert items，让 members 讨论 parachute。\nStaff / 成员: begin a natural member discussion; no formal event. / 自然开启成员讨论，不触发正式事件。"),
        P(2, 2, PromptPhase.Episode1, "B2-T2-E1 Suppressed Entry / 被忽视进入", "Time 40-100s | Item: Jack knife / 折叠刀 | Target: B", "Topic / 主题: A and C discuss jack knife; B has a point but no chance yet. / A 和 C 讨论 jack knife，B 有观点但未获得机会。\nRole assignment / 角色: A/C active speakers; B target. / A/C 是 active speakers，B 是 target。\nStaff action / 行动: B prepares a supplement and waits for invitation; A/C maintain two-person discussion. / B 准备补充并等待邀请，A/C 维持讨论。"),
        P(2, 2, PromptPhase.Episode2, "B2-T2-E2 Dominant Speaker / 主导者压制 target", "Time 100-170s | Item: Sunglasses / 太阳镜 | Target: C", "Topic / 主题: B dominates the sunglasses discussion; C has not spoken. / B 主导 sunglasses 讨论，C 尚未表达。\nRole assignment / 角色: B dominant speaker; A secondary responder; C target. / B 主导，A 简短回应，C 是 target。\nStaff action / 行动: C prepares a point about crash-site strategy and signal/shade tradeoffs; B speaks more; A briefly responds. / C 准备 crash site 与信号/遮阳取舍观点，B 多说，A 简短回应。"),
        P(2, 2, PromptPhase.Episode3, "B2-T2-E3 Repeated Suppressed Entry / 重复被忽视进入", "Time 170-240s | Item: Map/compass / 地图或指南针 | Target: A", "Topic / 主题: B/C discuss map and compass; A has a point but no chance. / B/C 讨论 map/compass，A 有观点但未获机会。\nRole assignment / 角色: B/C active speakers; A target. / B/C 是 active speakers，A 是 target。\nStaff action / 行动: A prepares a point about whether to leave crash site; B/C maintain discussion. / A 准备是否离开 crash site 的观点，B/C 维持讨论。"),
        P(2, 2, PromptPhase.Summary, "B2-T2-S Summary / 总结", "Time 240-300s | Items: parachute, jack knife, sunglasses, map/compass", "Topic / 主题: summarize the later four desert survival items. / 总结后 4 个沙漠求生 items。\nStaff action / 行动: follow leader's wrap-up. / 配合 leader 收尾。"),

        P(3, 1, PromptPhase.Opening, "B3-T1-O Opening / 开场", "Time 0-40s | Silence Context | Mountain survival / 深山求生", "Topic / 主题: leader introduces mountain survival and starts discussing fire source. / leader 介绍深山求生并开始讨论火源。\nStaff / 成员: allow a natural pause after discussion; no formal event yet. / 允许自然停顿出现，暂不触发正式事件。"),
        P(3, 1, PromptPhase.Episode1, "B3-T1-E1 First Restart / 第一次重启", "Time 40-100s | Item: Shelter material / 遮蔽物 | Target: A", "Topic / 主题: discussion pauses; A is best suited to continue shelter. / 讨论短暂停顿，A 最适合继续 shelter item。\nRole assignment / 角色: A target; B/C waiting members. / A 是 target，B/C 等待。\nStaff action / 行动: A waits for leader invitation and continues with wind, rain, and warmth points; B/C do not restart first. / A 等待邀请后继续讨论，B/C 不抢先重启。"),
        P(3, 1, PromptPhase.Episode2, "B3-T1-E2 Competing Restart / 竞争重启", "Time 100-170s | Item: First-aid kit / 急救包 | Primary: B | Secondary: C", "Topic / 主题: after a pause, B and C can continue first aid, but B is primary. / 停顿后 B 和 C 都可接续 first-aid kit，但 B 是 primary。\nRole assignment / 角色: B primary; C secondary; A waiting. / B 是 primary，C 是 secondary，A 等待。\nStaff action / 行动: B prepares the more fitting point; C can continue but less strongly; A waits. / B 准备更贴合观点，C 次要接续，A 等待。\nExpected / 预期: inviting B is expected; C is mismatch. / 邀请 B 为预期，邀请 C 为 mismatch。"),
        P(3, 1, PromptPhase.Episode3, "B3-T1-E3 Repeated Restart / 重复重启", "Time 170-240s | Item: Signal flares / 信号弹 | Target: C", "Topic / 主题: another pause appears; C is best suited to continue signal flares. / 再次短暂停顿，C 最适合继续 signal flares。\nRole assignment / 角色: C target; A/B waiting. / C 是 target，A/B 等待。\nStaff action / 行动: C waits for invitation and continues the rescue visibility discussion; A/B do not restart first. / C 等待邀请后继续，A/B 不抢先。"),
        P(3, 1, PromptPhase.Summary, "B3-T1-S Summary / 总结", "Time 240-300s | Items: matches/lighter, shelter material, first-aid kit, signal flares", "Topic / 主题: summarize the first four mountain survival items. / 总结前 4 个深山求生 items。\nStaff action / 行动: follow leader's closing and final confirmation. / 跟随 leader 收尾和最后确认。"),
        P(3, 2, PromptPhase.Opening, "B3-T2-O Opening / 开场", "Time 0-40s | Silence Context | Mountain survival / 深山求生", "Topic / 主题: leader introduces later four mountain items and starts water discussion. / leader 介绍后 4 个 mountain items 并开始讨论水。\nStaff / 成员: enter discussion naturally and allow pauses. / 自然进入讨论并允许停顿。"),
        P(3, 2, PromptPhase.Episode1, "B3-T2-E1 First Restart / 第一次重启", "Time 40-100s | Item: Toolbox/axe/knife / 工具箱/手斧/刀 | Target: C", "Topic / 主题: current item pauses; C is best to continue tools. / 当前 item 暂停，C 最适合继续工具类讨论。\nRole assignment / 角色: C target; A/B waiting. / C 是 target，A/B 等待。\nStaff action / 行动: C waits for invitation and continues with cutting, repair, and shelter-building points. / C 等待邀请后从切割、修理、搭建等角度继续。"),
        P(3, 2, PromptPhase.Episode2, "B3-T2-E2 Competing Restart / 竞争重启", "Time 100-170s | Item: Extra clothing/blanket / 额外衣物或毯子 | Primary: A | Secondary: B", "Topic / 主题: after a pause, A and B can continue warmth item; A is more suitable first. / 停顿后 A 和 B 都可继续保暖 item，但 A 更适合先发言。\nRole assignment / 角色: A primary; B secondary; C waiting. / A 是 primary，B 是 secondary，C 等待。\nStaff action / 行动: A prepares the main warmth/exposure point; B prepares a secondary point; C waits. / A 准备主要保暖观点，B 次要，C 等待。"),
        P(3, 2, PromptPhase.Episode3, "B3-T2-E3 Repeated Restart / 重复重启", "Time 170-240s | Item: Chocolate/high-energy food / 巧克力或高能量食物 | Target: B", "Topic / 主题: another pause appears; B is best suited to continue energy supply. / 再次短暂停顿，B 最适合继续能量补给。\nRole assignment / 角色: B target; A/C waiting. / B 是 target，A/C 等待。\nStaff action / 行动: B waits for invitation and discusses energy, body heat, morale, and rationing; A/C do not restart first. / B 等待邀请后讨论能量、体温、士气和分配，A/C 不抢先。"),
        P(3, 2, PromptPhase.Summary, "B3-T2-S Summary / 总结", "Time 240-300s | Items: bottled water, tools, clothing/blanket, chocolate/food", "Topic / 主题: summarize the later four mountain survival items. / 总结后 4 个深山求生 items。\nStaff action / 行动: follow leader's wrap-up. / 跟随 leader 收尾。")
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
