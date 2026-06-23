using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(100)]
public class BotSpeakingGestureLoopController : MonoBehaviour
{
    private const float TotalDuration = 90f;
    private const string ControlledSceneName = OfficeSceneSupport.OfficeLoggedIn;

    [SerializeField] private float transitionSpeed = 8f;
    [SerializeField] private float gestureSpeed = 3.5f;
    [SerializeField] private Vector3 silentLeftLocalPosition = new Vector3(-0.42f, -0.34f, 0.08f);
    [SerializeField] private Vector3 silentRightLocalPosition = new Vector3(0.42f, -0.34f, 0.08f);
    [SerializeField] private Vector3 speakingLeftLocalPosition = new Vector3(-0.2f, 0.2f, 0.24f);
    [SerializeField] private Vector3 speakingRightLocalPosition = new Vector3(0.2f, 0.08f, 0.22f);

    private BotHands dcyHands;
    private BotHands zjrHands;
    private BotHands zhzHands;
    private float fallbackStartTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<BotSpeakingGestureLoopController>() != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject("Bot Speaking Gesture Loop Controller");
        DontDestroyOnLoad(controllerObject);
        controllerObject.AddComponent<BotSpeakingGestureLoopController>();
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
        if (!ShouldControlCurrentScene())
        {
            ClearTargets();
            return;
        }

        BindTargets(resetTimer: true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != ControlledSceneName)
        {
            ClearTargets();
            return;
        }

        BindTargets(resetTimer: true);
    }

    private void LateUpdate()
    {
        if (!ShouldControlCurrentScene())
        {
            ClearTargets();
            return;
        }

        if (!HasTargets())
        {
            BindTargets(resetTimer: false);
        }

        if (!HasTargets())
        {
            return;
        }

        float loopTime = GetLoopTime();
        bool dcySpeaking = loopTime < 60f;
        bool zjrSpeaking = loopTime >= 30f && loopTime < 60f;
        bool zhzSpeaking = false;

        dcyHands.UpdatePose(dcySpeaking, loopTime, 0.0f, this);
        zjrHands.UpdatePose(zjrSpeaking, loopTime, 0.7f, this);
        zhzHands.UpdatePose(zhzSpeaking, loopTime, 1.4f, this);
    }

    private void BindTargets(bool resetTimer)
    {
        dcyHands = BotHands.TryCreate("DCY");
        zjrHands = BotHands.TryCreate("ZJR");
        zhzHands = BotHands.TryCreate("ZHZ");

        if (resetTimer && HasTargets())
        {
            fallbackStartTime = Time.time;
        }
    }

    private void ClearTargets()
    {
        dcyHands = default;
        zjrHands = default;
        zhzHands = default;
        fallbackStartTime = 0f;
    }

    private bool ShouldControlCurrentScene()
    {
        return SceneManager.GetActiveScene().name == ControlledSceneName;
    }

    private float GetLoopTime()
    {
        if (SpeakingIntentionScenarioLoopController.IsRunning)
        {
            return SpeakingIntentionScenarioLoopController.CurrentLoopTime;
        }

        return Mathf.Repeat(Time.time - fallbackStartTime, TotalDuration);
    }

    private bool HasTargets()
    {
        return dcyHands.IsValid && zjrHands.IsValid && zhzHands.IsValid;
    }

    private Vector3 GetTargetPosition(bool isLeftHand, bool isSpeaking, float loopTime, float phase)
    {
        Vector3 basePosition = isSpeaking
            ? (isLeftHand ? speakingLeftLocalPosition : speakingRightLocalPosition)
            : (isLeftHand ? silentLeftLocalPosition : silentRightLocalPosition);

        if (!isSpeaking)
        {
            return basePosition;
        }

        float verticalWave = Mathf.Sin(loopTime * gestureSpeed + phase);
        float secondaryWave = Mathf.Cos(loopTime * (gestureSpeed * 0.67f) + phase * 1.31f);
        float heightAmplitude = isLeftHand ? 0.075f : 0.045f;
        float forwardAmplitude = isLeftHand ? 0.025f : 0.04f;

        basePosition.y += verticalWave * heightAmplitude;
        basePosition.z += secondaryWave * forwardAmplitude;
        basePosition.x += (isLeftHand ? -1f : 1f) * secondaryWave * 0.02f;
        return basePosition;
    }

    private Quaternion GetTargetRotation(Quaternion baseRotation, bool isLeftHand, bool isSpeaking, float loopTime, float phase)
    {
        if (!isSpeaking)
        {
            return baseRotation * Quaternion.Euler(isLeftHand ? 12f : -12f, 0f, isLeftHand ? -18f : 18f);
        }

        float wave = Mathf.Sin(loopTime * gestureSpeed + phase);
        float secondaryWave = Mathf.Cos(loopTime * (gestureSpeed * 0.71f) + phase);
        float facingEachOtherYaw = isLeftHand ? 68f : -68f;
        float backOutRoll = isLeftHand ? -36f : 36f;
        float pitch = -4f + secondaryWave * 7f;
        float roll = backOutRoll + wave * (isLeftHand ? 9f : 13f);
        return baseRotation * Quaternion.Euler(pitch, facingEachOtherYaw, roll);
    }

    private struct BotHands
    {
        private readonly Transform leftHand;
        private readonly Transform rightHand;
        private readonly Quaternion leftBaseRotation;
        private readonly Quaternion rightBaseRotation;

        public bool IsValid
        {
            get { return leftHand != null && rightHand != null; }
        }

        private BotHands(Transform leftHand, Transform rightHand)
        {
            this.leftHand = leftHand;
            this.rightHand = rightHand;
            leftBaseRotation = leftHand != null ? leftHand.localRotation : Quaternion.identity;
            rightBaseRotation = rightHand != null ? rightHand.localRotation : Quaternion.identity;
        }

        public static BotHands TryCreate(string botName)
        {
            GameObject bot = GameObject.Find(botName);
            if (bot == null)
            {
                return default;
            }

            Transform[] children = bot.GetComponentsInChildren<Transform>(true);
            Transform left = null;
            Transform right = null;

            foreach (Transform child in children)
            {
                if (left == null && child.name.StartsWith("LeftHand_"))
                {
                    left = child;
                }
                else if (right == null && child.name.StartsWith("RightHand_"))
                {
                    right = child;
                }

                if (left != null && right != null)
                {
                    break;
                }
            }

            return new BotHands(left, right);
        }

        public void UpdatePose(bool isSpeaking, float loopTime, float phase, BotSpeakingGestureLoopController controller)
        {
            if (!IsValid)
            {
                return;
            }

            float t = 1f - Mathf.Exp(-controller.transitionSpeed * Time.deltaTime);

            Vector3 leftTarget = controller.GetTargetPosition(true, isSpeaking, loopTime, phase);
            Vector3 rightTarget = controller.GetTargetPosition(false, isSpeaking, loopTime, phase + 0.9f);
            Quaternion leftRotation = controller.GetTargetRotation(leftBaseRotation, true, isSpeaking, loopTime, phase);
            Quaternion rightRotation = controller.GetTargetRotation(rightBaseRotation, false, isSpeaking, loopTime, phase + 0.9f);

            leftHand.localPosition = Vector3.Lerp(leftHand.localPosition, leftTarget, t);
            rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, rightTarget, t);
            leftHand.localRotation = Quaternion.Slerp(leftHand.localRotation, leftRotation, t);
            rightHand.localRotation = Quaternion.Slerp(rightHand.localRotation, rightRotation, t);
        }
    }
}
