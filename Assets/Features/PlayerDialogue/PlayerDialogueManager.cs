using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;



[Serializable]
public class DialogueLine
{
    public string text;
    public AudioClip voiceline;
}

public class PlayerDialogueManager : MonoBehaviour
{
    private const float estimateWPM = 40;

    public Queue<PlayerDialogue> dialogueQueue = new Queue<PlayerDialogue>();


    [Header("References")]
    [SerializeField] AudioSource dialogueSource;

    [SerializeField] TextMeshProUGUI captionText;

    [Header("Setting")]
    [SerializeField] float timeBetweenLines;
    [SerializeField] float timeBetweenDialogue;

    [SerializeField] float characterDelay = 0.04f;

    private Coroutine dialogueRoutine;

    public UnityEvent OnDialogueStart;
    public UnityEvent OnDialogueEnd;

    public static PlayerDialogueManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"!!! Multiple {this.name} found in scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        dialogueQueue.Clear();
        ClearCaptions();
    }

    private void Update()
    {
        if (dialogueRoutine == null && dialogueQueue.Count > 0) dialogueRoutine = StartCoroutine(DialogueRoutine());
    }

    public void QueueDialogue(PlayerDialogue dialogue)
    {
        dialogueQueue.Enqueue(dialogue);
    }

    public void ForceDialogue(PlayerDialogue dialogue)
    {
        Queue<PlayerDialogue> newQueue = new Queue<PlayerDialogue>();   
        newQueue.Enqueue(dialogue);

        foreach(PlayerDialogue queuedDialogue in dialogueQueue)
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

        OnDialogueEnd?.Invoke();

    }


    private IEnumerator DialogueRoutine()
    {
        DialogueLine[] lines = dialogueQueue.Dequeue().GetLines();

        OnDialogueStart?.Invoke();

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

        OnDialogueEnd?.Invoke();
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
