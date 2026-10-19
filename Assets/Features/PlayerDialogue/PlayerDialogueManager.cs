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
    private const float estimateWPM = 200;

    private Queue<PlayerDialogue> dialogueQueue = new Queue<PlayerDialogue>();

    private PlayerDialogue currentDialogue;


    [Header("References")]
    [SerializeField] AudioSource dialogueSource;

    [SerializeField] TextMeshProUGUI captionText;

    [Header("Setting")]
    public bool ignoreNonPriority;

    [SerializeField] float timeBetweenLines;
    [SerializeField] float timeBetweenDialogue;

    [SerializeField] float characterDelay = 0.04f;

    private Coroutine dialogueRoutine;

    public UnityEvent OnDialogueStart;
    public UnityEvent OnDialogueEnd;
    public UnityEvent OnPriorityDialogueEnd;

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
        currentDialogue = null;
        dialogueQueue.Clear();
        ClearCaptions();
    }

    private void Update()
    {
        if (dialogueRoutine == null && dialogueQueue.Count > 0) dialogueRoutine = StartCoroutine(DialogueRoutine());
    }

    public void QueueDialogue(PlayerDialogue dialogue)
    {
        if (ignoreNonPriority && !dialogue.priority) return;

        if (currentDialogue != null)
        {
            if (currentDialogue.priority) return;
            if (currentDialogue == dialogue) return;
        }

        foreach (PlayerDialogue p in dialogueQueue)
        {
            if (p == dialogue) return;
        }

        dialogueQueue.Enqueue(dialogue);
    }

    public void ForceDialogue(PlayerDialogue dialogue)
    {
        if (ignoreNonPriority && !dialogue.priority) return;

        if (currentDialogue != null)
        {
            if (currentDialogue.priority) return;
            if (currentDialogue == dialogue) return;
        }


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

        bool ended = false;
        if (currentDialogue != null) ended = true;

        bool prio = false;
        if (currentDialogue != null) prio = currentDialogue.priority;

        currentDialogue = null;

        if (ended) OnDialogueEnd?.Invoke();
        if (prio) OnPriorityDialogueEnd?.Invoke();
    }


    private IEnumerator DialogueRoutine()
    {

        captionText.maxVisibleCharacters = 0;

        currentDialogue = dialogueQueue.Dequeue();
        DialogueLine[] lines = currentDialogue.GetLines();

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

            captionText.gameObject.SetActive(true);
            captionText.text = line.text;
            captionText.maxVisibleCharacters = 0;

            captionText.ForceMeshUpdate();

            int characterCount = captionText.textInfo.characterCount;

            for (int i = 1; i <= characterCount; i++)
            {

                captionText.maxVisibleCharacters = i;

                yield return new WaitForSeconds(characterDelay);
            }


            float typingDuration = captionText.textInfo.characterCount * characterDelay;

            float remainingDuration = lineDuration - typingDuration;

            if (remainingDuration > 0f)
            {
                yield return new WaitForSeconds(remainingDuration);
            }


            ClearCaptions();
       
        }

        yield return new WaitForSeconds(timeBetweenDialogue);

        bool prio = currentDialogue.priority;

        currentDialogue = null;
        OnDialogueEnd?.Invoke();

        if (prio) OnPriorityDialogueEnd?.Invoke();

        dialogueRoutine = null;
    }


    private void ClearCaptions()
    {
        captionText.text = "";
        captionText.maxVisibleCharacters = 0;
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
