using System;
using System.Collections;
using System.Collections.Generic;
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
    public Queue<RadioDialogue> messages = new Queue<RadioDialogue>();
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

    public UnityEvent OnStartRadioDialogue;

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
        OnStartRadioDialogue.Invoke();
    }

    public void OnRadioDialogueEnd() //called when incoming dialogue is done
    {
        incomingMessage = null;
        state = RadioState.None; 
    }

}
