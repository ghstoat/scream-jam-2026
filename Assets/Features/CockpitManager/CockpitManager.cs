using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum CockpitState
{
    Default,
    Alarm,
    Reset
}

public class CockpitManager : MonoBehaviour
{
    public CockpitState state {  get; private set; }

    [SerializeField] Transform cockpitTransform;
    private Coroutine shakeCoroutine;
    private Vector3 cockpitOriginalPosition;

    [SerializeField] Animator lightAnimator;

    [Header("Self Destruct")]
    [SerializeField] PlayerDialogue sixtySecondsLeft;
    [SerializeField] PlayerDialogue thirtySecondsLeft;
    [SerializeField] PlayerDialogue fifteenSecondsLeft;
    [SerializeField] PlayerDialogue tenSecondsLeft;
    [SerializeField] PlayerDialogue selfDestructImminent;

    [SerializeField] AudioSource selfDestructVoice;
    public AudioSource AlarmVoiceSource => selfDestructVoice;
    [SerializeField] float selfDestructDuration = 60;

    [Header("Alarm Voice")]
    private PlayerDialogue currentDialogue;
    private Queue<PlayerDialogue> dialogueQueue = new Queue<PlayerDialogue>();
    private Coroutine dialogueRoutine;
    [SerializeField] TextMeshProUGUI captionText;


    [Header("Systems In Cockpit")]
    [SerializeField] Light mainLight;
    [SerializeField] GameObject screen;
    [SerializeField] OxygenMeter oxygenMeter;
    [SerializeField] Heater heater;
    [SerializeField] Radio radio;
    [SerializeField] Vent vent;
    [SerializeField] FlashMachine flashMachine;
    [SerializeField] PowerSwitcher powerSwitcher;
    [SerializeField] Button ejectButton;

    public Light MainLight => mainLight;
    public GameObject Screen => screen;
    public OxygenMeter OxygenMeter => oxygenMeter;
    public Heater Heater => heater;
    public Radio Radio => radio;
    public Vent Vent => vent;
    public FlashMachine FlashMachine => flashMachine;
    public PowerSwitcher PowerSwitcher => powerSwitcher;
    public Button EjectButton => ejectButton;


    private const string DEFAULT = "DEFAULT";
    private const string ALARM = "ALARM";
    private const string RESET = "RESET";

    private Coroutine resetCoroutine;
    private Coroutine selfDestructRoutine;

    public static CockpitManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"!!! Multiple {this.name} found in scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        state = CockpitState.Default;
    }

    private void Start()
    {
        Init(); 
        
        currentDialogue = null;
        dialogueQueue.Clear();
        ClearCaptions();
    }



    public void SetState(CockpitState newState)
    {
        if (newState == state) return;

        state = newState;   

        switch (state)
        {
            case CockpitState.Alarm:
                EnterAlarm(); break;  
            case CockpitState.Reset:
                EnterReset(); break;
            default: EnterDefault(); break;

        }
    }

    private void Update()
    {
        if (dialogueRoutine == null && dialogueQueue.Count > 0) dialogueRoutine = StartCoroutine(DialogueRoutine());

    }

    private void Init()
    {
        ejectButton.Lock(true);
    }
    private void ResetAllTriggers(Animator animator)
    {
        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(param.name);
            }
        }
    }

    private void Trigger(Animator animator, string trigger)
    {
        ResetAllTriggers(animator);
        animator.SetTrigger(trigger);
    }

    private void EnterDefault()
    {
        StopResetCoroutine();
        Trigger(lightAnimator, DEFAULT);

    }

    private void EnterAlarm()
    {
        StopResetCoroutine();
        Trigger(lightAnimator, ALARM);
    }
    private void EnterReset()
    {
        StopResetCoroutine();
        Trigger(lightAnimator, RESET);
        resetCoroutine = StartCoroutine(ResetRoutine());
    }
    private IEnumerator ResetRoutine()
    {
        while (!lightAnimator.GetCurrentAnimatorStateInfo(0).IsName("RESET"))
        {
            yield return null;
        }

        AnimatorStateInfo stateInfo = lightAnimator.GetCurrentAnimatorStateInfo(0);

        float duration = stateInfo.length;

        float speed = Mathf.Abs(stateInfo.speed * lightAnimator.speed);

        if (speed > 0f)  duration /= speed;

        yield return new WaitForSeconds(duration);

        if (state == CockpitState.Reset) SetState(CockpitState.Default);

        resetCoroutine = null;
    }

    private void StopResetCoroutine()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }
    }

    public void ShakeCockpit(float shakeMagnitude, float shakeDuration)
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeRoutine(shakeMagnitude, shakeDuration));
    }

    private IEnumerator ShakeRoutine(float shakeMagnitude, float shakeDuration)
    {
        cockpitOriginalPosition = cockpitTransform.localPosition;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            Vector3 randomOffset = Random.insideUnitSphere * shakeMagnitude;
            float fade = (shakeDuration - elapsed) /shakeDuration;

            cockpitTransform.localPosition = cockpitOriginalPosition + randomOffset * fade;

            elapsed += Time.deltaTime;
            yield return null;
        }

        cockpitTransform.localPosition = cockpitOriginalPosition;
        shakeCoroutine = null;
    }



    #region selfdestruct
    public void TryToEject()
    {
        ejectButton.SetInteractable(false);

        ShakeCockpit(.8f, 2f);
    }

    public void StartSelfDestructRoutine() 
    {
        if (selfDestructRoutine != null) StopCoroutine(selfDestructRoutine);
        selfDestructRoutine = StartCoroutine(SelfDestructRoutine());
    }

    private void StopSelfDestructRoutine()
    {
        if (selfDestructRoutine != null) StopCoroutine(selfDestructRoutine);
        selfDestructRoutine = null;
    }

    private IEnumerator SelfDestructRoutine()
    {
        float timer = selfDestructDuration;

        QueueDialogue(sixtySecondsLeft);

        int stage = 0;

        while (timer > 0)
        {
            if (state != CockpitState.Alarm)
            {
                StopSelfDestructRoutine();
                yield break;
            }

            timer -= Time.deltaTime;

            switch (stage)
            {
                case 0:
                    if (timer < 30)
                    {
                        stage++;
                        QueueDialogue(thirtySecondsLeft);
                    }
                    break;
                case 1:
                    if (timer < 15)
                    {
                        stage++;
                        QueueDialogue(fifteenSecondsLeft);
                    }
                    break;
                case 2:
                    if (timer < 10)
                    {
                        stage++;
                        QueueDialogue(tenSecondsLeft);
                    }
                    break;
                default:
                    if (!selfDestructVoice.isPlaying)
                    {
                        QueueDialogue(selfDestructImminent);
                    }
                    break;

            }




            yield return null;

        }

        HealthManager.Instance.Die(Death.SelfDestruct);
    }
    #endregion


    #region alarm voice

    public void QueueDialogue(PlayerDialogue dialogue)
    {
        if (currentDialogue != null)
        {
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
        if (currentDialogue != null)
        {
            if (currentDialogue == dialogue) return;
        }


        Queue<PlayerDialogue> newQueue = new Queue<PlayerDialogue>();
        newQueue.Enqueue(dialogue);

        foreach (PlayerDialogue queuedDialogue in dialogueQueue)
        {
            newQueue.Enqueue(queuedDialogue);
        }

        dialogueQueue = newQueue;

        InterruptDialogue();
    }

    public void InterruptDialogue()
    {

        if (selfDestructVoice.isPlaying) selfDestructVoice.Stop();
        selfDestructVoice.clip = null;

        if (dialogueRoutine != null) StopCoroutine(dialogueRoutine);
        dialogueRoutine = null;

        ClearCaptions();

        bool ended = false;
        if (currentDialogue != null) ended = true;

        bool prio = false;
        if (currentDialogue != null) prio = currentDialogue.priority;

        currentDialogue = null;

    }
    private IEnumerator DialogueRoutine()
    {

        captionText.maxVisibleCharacters = 0;

        currentDialogue = dialogueQueue.Dequeue();
        DialogueLine[] lines = currentDialogue.GetLines();

        foreach (DialogueLine line in lines)
        {
            float lineDuration = 0.4f;

            if (line.voiceline != null)
            {
                lineDuration += line.voiceline.length;
                selfDestructVoice.clip = line.voiceline;
                selfDestructVoice.Play();
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

                yield return new WaitForSeconds(0.04f);
            }


            float typingDuration = captionText.textInfo.characterCount * 0.04f;

            float remainingDuration = lineDuration - typingDuration;

            if (remainingDuration > 0f)
            {
                yield return new WaitForSeconds(remainingDuration);
            }


            ClearCaptions();

        }

        yield return new WaitForSeconds(0.04f);

        bool prio = currentDialogue.priority;

        currentDialogue = null;

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

        float wordsPerSecond = 200 / 60f;
        float readTimeSeconds = wordCount / wordsPerSecond;

        return readTimeSeconds;
    }



    #endregion
}
