using UnityEngine;

[RequireComponent(typeof(SpeakingIntention))]
public class ZJRSpeakingIntentionController : MonoBehaviour
{
    [SerializeField] private float increasePerSecond = 5f;
    [SerializeField] private float resetValue = 50f;
    [SerializeField] private float maxValue = 100f;

    private SpeakingIntention speakingIntention;
    private float timer;

    private void Awake()
    {
        speakingIntention = GetComponent<SpeakingIntention>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < 1f)
        {
            return;
        }

        timer -= 1f;
        speakingIntention.speaking_intention += increasePerSecond;

        if (speakingIntention.speaking_intention >= maxValue)
        {
            speakingIntention.speaking_intention = resetValue;
        }
    }
}
