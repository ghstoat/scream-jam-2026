using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Radio : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI outgoingDisplayText; 
    [SerializeField] TextMeshProUGUI incomingDisplayText;

    [SerializeField] LightBulb incomingLight;
    public LightBulb IncomingLight => incomingLight;

    [SerializeField] Button talkButton; 
    [SerializeField] Dial dial;

    [SerializeField] GameObject talkButtonPressed;

    [SerializeField] FillBar powerBar;
    [SerializeField] LightBulb powerLight;


    private const float estimateWPM = 200;

    private Queue<RadioDialogue> dialogueQueue = new Queue<RadioDialogue>();


    [Header("References")]
    [SerializeField] AudioSource dialogueSource;

    [SerializeField] TextMeshProUGUI captionText;

    [Header("Setting")]
    [SerializeField] float timeBetweenLines;
    [SerializeField] float timeBetweenDialogue;

    [SerializeField] float characterDelay = 0.04f;

    private Coroutine dialogueRoutine;

    private void Start()
    {
        RadioManager.Instance.OnRadioDialogueStart.AddListener(StartRadioDialogue);

        dialogueQueue.Clear();
        ClearCaptions();
    }


    private void Update()
    {
        UpdateIndicators();
        UpdateFrequency(dial.Value);



        if (dialogueRoutine == null && dialogueQueue.Count > 0) dialogueRoutine = StartCoroutine(DialogueRoutine());

    }

    private void UpdateIndicators()
    {

        if (RadioManager.Instance.incomingMessage != null)
        {
            incomingDisplayText.text = $"{RadioManager.Instance.incomingMessage.frequency:000.00}";

        }
        else
        {
            incomingDisplayText.text = "000.00";
        }

        switch (RadioManager.Instance.state)
        {
            case RadioState.Waiting:
                incomingLight.SetColor(Color.cornflowerBlue);
                incomingLight.SetState(LightStates.Blinking);
                talkButtonPressed.SetActive(false);
                break;
            case RadioState.Outgoing:
                incomingLight.SetColor(Color.green);
                incomingLight.SetState(LightStates.On);
                talkButtonPressed.SetActive(true);
                break;
            case RadioState.Incoming:  
                incomingLight.SetColor(Color.red);
                incomingLight.SetState(LightStates.On);
                talkButtonPressed.SetActive(true);
                break;
             default:
                incomingLight.SetState(LightStates.Off);
                talkButtonPressed.SetActive(false);
                break;

        }


        powerBar.SetTargetScaleYFromRatio(RadioManager.Instance.PowerRatio);
        if (RadioManager.Instance.CanUseRadio) powerLight.SetState(LightStates.On);
        else powerLight.SetState(LightStates.Off);
    }

    public void UpdateFrequency(float ratio)
    {
        ratio = 1 - Mathf.Clamp01(ratio);

        int minFrequency = RadioManager.Instance.MinFrequency;
        int maxFrequency = RadioManager.Instance.MaxFrequency;

        int frequency = (int)Mathf.Lerp(minFrequency, maxFrequency, ratio);
        RadioManager.Instance.SetFrequency(frequency);

        outgoingDisplayText.text = $"{frequency:000.00}";
    }

    public void TalkButtonPressed()
    {
        RadioManager.Instance.TalkButtonPressed();
    }

    private void StartRadioDialogue()
    {
        QueueDialogue(RadioManager.Instance.incomingMessage);
    }

    public void QueueDialogue(RadioDialogue dialogue)
    {
        dialogueQueue.Enqueue(dialogue);
    }
    public void ForceDialogue(RadioDialogue dialogue)
    {
        Queue<RadioDialogue> newQueue = new Queue<RadioDialogue>();
        newQueue.Enqueue(dialogue);

        foreach (RadioDialogue queuedDialogue in dialogueQueue)
        {
            newQueue.Enqueue(queuedDialogue);
        }

        dialogueQueue = newQueue;

        InterruptDialogue();
    }

    public void InterruptDialogue()
    {
        if (dialogueSource.isPlaying) dialogueSource.Stop();
        dialogueSource.clip = null;

        if (dialogueRoutine != null) StopCoroutine(dialogueRoutine);
        dialogueRoutine = null;

        ClearCaptions();

        RadioManager.Instance.OnGoToNextRadioDialogue();

    }


    private IEnumerator DialogueRoutine()
    {
        DialogueLine[] lines = dialogueQueue.Dequeue().GetLines();

        foreach (DialogueLine line in lines)
        {
            float lineDuration = timeBetweenLines;

            if (line.voiceline != null)
            {
                lineDuration += line.voiceline.length;
                dialogueSource.clip = line.voiceline;
                dialogueSource.Play();
            }
            else
            {
                lineDuration += EstimateDialogueLength(line.text);
            }

            yield return StartCoroutine(TypeCaptions(line.text));

            float typingDuration = captionText.textInfo.characterCount * characterDelay;

            float remainingDuration = lineDuration - typingDuration;

            if (remainingDuration > 0f)
            {
                yield return new WaitForSeconds(remainingDuration);
            }

            ClearCaptions();

        }

        yield return new WaitForSeconds(timeBetweenDialogue);

        RadioManager.Instance.OnGoToNextRadioDialogue();
        dialogueRoutine = null;
    }

    private IEnumerator TypeCaptions(string text)
    {
        captionText.gameObject.SetActive(true);
        captionText.text = text;
        captionText.maxVisibleCharacters = 0;

        captionText.ForceMeshUpdate();

        int characterCount = captionText.textInfo.characterCount;

        for (int i = 1; i <= characterCount; i++)
        {
            captionText.maxVisibleCharacters = i;

            yield return new WaitForSeconds(characterDelay);
        }
    }

    private void ClearCaptions()
    {
        captionText.maxVisibleCharacters = int.MaxValue;
        captionText.text = "";
        captionText.gameObject.SetActive(false);
    }

    private float EstimateDialogueLength(string sentence)
    {
        string[] words = sentence.Split(new char[] { ' ', '\t', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        int wordCount = words.Length;

        float wordsPerSecond = estimateWPM / 60f;
        float readTimeSeconds = wordCount / wordsPerSecond;

        return readTimeSeconds;
    }
}
