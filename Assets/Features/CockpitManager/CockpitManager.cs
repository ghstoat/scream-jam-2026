using System.Collections;
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

    [SerializeField] Animator lightAnimator;

    [Header("Self Destruct")]
    [SerializeField] AudioClip sixtySecondsLeft;
    [SerializeField] AudioClip thirtySecondsLeft;
    [SerializeField] AudioClip fifteenSecondsLeft;
    [SerializeField] AudioClip tenSecondsLeft;
    [SerializeField] AudioClip selfDestructImminent;

    [SerializeField] AudioSource selfDestructVoice;
    [SerializeField] float selfDestructDuration = 60;

    [Header("Systems In Cockpit")]
    [SerializeField] OxygenMeter oxygenMeter;
    [SerializeField] Heater heater;
    [SerializeField] Radio radio;
    [SerializeField] Vent vent;
    [SerializeField] FlashMachine flashMachine;
    [SerializeField] PowerSwitcher powerSwitcher;
    [SerializeField] Button ejectButton;

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

    public void StartSelfDestructRoutine() 
    {
        if (selfDestructRoutine != null) StopCoroutine(selfDestructRoutine);
        selfDestructRoutine = StartCoroutine(SelfDestructRoutine());
    }

    private IEnumerator SelfDestructRoutine()
    {
        float timer = selfDestructDuration;

        selfDestructVoice.clip = thirtySecondsLeft;
        selfDestructVoice.Play();

        int stage = 0;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            switch (stage)
            {
                case 0:
                    if (timer < 30)
                    {
                        stage++;
                        selfDestructVoice.clip = thirtySecondsLeft;
                    }
                    break;
                case 1:
                    if (timer < 15)
                    {
                        stage++;
                        selfDestructVoice.clip = fifteenSecondsLeft;
                    }
                    break;
                case 2:
                    if (timer < 10)
                    {
                        stage++;
                        selfDestructVoice.clip = tenSecondsLeft;
                    }
                    break;
                default:
                    if (!selfDestructVoice.isPlaying)
                    {
                        selfDestructVoice.clip = selfDestructImminent;
                        selfDestructVoice.Play();
                    }
                    break;

            }
            yield return null;

        }

        HealthManager.Instance.Die(Death.SelfDestruct);
    }

}
