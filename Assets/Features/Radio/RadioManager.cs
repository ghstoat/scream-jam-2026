using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public enum RadioState
{
    None,
    Waiting,
    Outgoing,
    Incoming,
}

public class RadioManager : PoweredSystem
{
    private Queue<RadioDialogue> messages = new Queue<RadioDialogue>();
    public RadioState state { get; private set; } = RadioState.None;


    public int frequency { get; private set; }
    public RadioDialogue incomingMessage {  get; private set; } = null;

    private bool dialogueStarted = false;

    [SerializeField] int requiredPowerToUseRadio;
    public bool CanUseRadio => currentPower >= requiredPowerToUseRadio;

    [SerializeField] int minFrequency = 50;
    [SerializeField] int maxFrequency = 250;

    public int MinFrequency => minFrequency;
    public int MaxFrequency => maxFrequency;

    public UnityEvent OnRadioDialogueStart;
    public UnityEvent OnRadioDialogueEnd;


    [Header("Player dialogue")]
    [SerializeField] PlayerDialogue radioEmpty;
    [SerializeField] PlayerDialogue radioNoPower;
    [SerializeField] PlayerDialogue wrongFrequency;


    public static RadioManager Instance { get; private set; }
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
        messages.Clear();
        state = RadioState.None;
        incomingMessage = null;

        dialogueStarted = false;

        PlayerDialogueManager.Instance.OnDialogueStart.AddListener(OnPlayerDialogueStart);

        PlayerDialogueManager.Instance.OnDialogueEnd.AddListener(OnPlayerDialogueEnd);
    }

    public void ReceiveMessage(RadioDialogue message)
    {
        if (message.frequency > maxFrequency || message.frequency < minFrequency)
        {
            Debug.LogWarning($"!!! Incoming message has a frequency out of range! {message.frequency}");
        }

        message.frequency = Mathf.Clamp(message.frequency, minFrequency, maxFrequency); 

        messages.Enqueue(message);
    }

    public void ReceiveMessage(Conversation conversation)
    {
        RadioDialogue[] messages = conversation.Dialogue;

        foreach (RadioDialogue message in messages)
        {
            ReceiveMessage(message);
        }
    }

    public void Update()
    {
        if (state == RadioState.None && messages.Count > 0)
        {
            incomingMessage = messages.Dequeue();
            state = RadioState.Waiting;
        }
    }

    public void SetFrequency(int frequency)
    {
        this.frequency = Mathf.Clamp(frequency, minFrequency, maxFrequency);
    }


    public void TalkButtonPressed()
    {
        if (state == RadioState.None)
        {
            PlayerDialogueManager.Instance.ForceDialogue(radioEmpty);
        }
        else if (!CanUseRadio)
        {
            PlayerDialogueManager.Instance.ForceDialogue(radioNoPower);
        }

        else if (frequency != incomingMessage.frequency)
        {
            PlayerDialogueManager.Instance.ForceDialogue(wrongFrequency);
        }


        if (state == RadioState.Waiting && frequency == incomingMessage.frequency && CanUseRadio)
        {
            StartOutgoing();
        }
    }

    private void StartOutgoing()
    {
        state = RadioState.Outgoing;
        dialogueStarted = false;
        PlayerDialogueManager.Instance.ForceDialogue(incomingMessage.playerDialogue);

        PlayerDialogueManager.Instance.ignoreNonPriority = true;

    }
    private void OnPlayerDialogueStart()
    {
        if (state == RadioState.Outgoing) dialogueStarted = true;
    }
    private void OnPlayerDialogueEnd()
    {
        if (state == RadioState.Outgoing && dialogueStarted) StartIncoming();
    }

    private void StartIncoming()
    {
        state = RadioState.Incoming;
        dialogueStarted = false;
        OnRadioDialogueStart.Invoke();
    }

    public void OnGoToNextRadioDialogue() //called when incoming dialogue is done
    {
        if (messages.Count > 0)
        {
            incomingMessage = messages.Dequeue();
            StartOutgoing();
            return;
        }

        incomingMessage = null;
        state = RadioState.None;
        PlayerDialogueManager.Instance.ignoreNonPriority = false;

        OnRadioDialogueEnd?.Invoke();
    }

}
