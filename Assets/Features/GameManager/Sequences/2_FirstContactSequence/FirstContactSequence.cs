using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "2_FirstContactSequence", menuName = "ScreamJam/Sequence/2_FirstContactSequence")]
public class FirstContactSequence : Sequence
{
    private bool dialogueEnded = false;
    private bool radioDialogueStarted = false;
    private bool radioDialogueEnded = false;


    [SerializeField] AudioClip music;

    [SerializeField] PlayerDialogue nowWhat;
    [SerializeField] PlayerDialogue radioJump;
    [SerializeField] Conversation firstContact;
    [SerializeField] PlayerDialogue stayAlive;

    public override void Init()
    {
        finished = false;
    }
    public override void Execute(GameManager ctx)
    {
        ctx.StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        PowerManager.Instance.DrainAllSystems();

        SubscribeToEvents();
        HealthManager.Instance.LockOxygen(true);
        HealthManager.Instance.LockTemp(true);
        HealthManager.Instance.LockToxicity(true);

        PlayerDialogueManager.Instance.ignoreNonPriority = true;

        CockpitManager.Instance.SetState(CockpitState.Default);

        yield return new WaitForSeconds(8);
        //MusicManager.Instance.PlayMusic(music, .5f, 0, 0, true, true);

        PlayerDialogueManager.Instance.ForceDialogue(nowWhat);

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;
        PlayerDialogueManager.Instance.ignoreNonPriority = false;

        MissionManager.Instance.SetMission("Assess your situation");

        yield return new WaitForSeconds(20);

        RadioManager.Instance.ReceiveMessage(firstContact);
        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Radio.IncomingLight.transform, 0.2f, 1.5f, true);

        yield return new WaitForSeconds(0.5f);

        PlayerDialogueManager.Instance.ForceDialogue(radioJump);

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;

        TutorialManager.Instance.OpenTutorial(Tutorial.PowerSwitcher);

        yield return new WaitForSeconds(0.1f);

        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.PowerSwitcher.transform, 1f, 1f, false);

        yield return new WaitForSeconds(1.5f);

        TutorialManager.Instance.OpenTutorial(Tutorial.Nodes);

        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.PowerSwitcher.ReservoirNode.transform, 1f, 1f, false);

        yield return new WaitForSeconds(1.5f);

        TutorialManager.Instance.OpenTutorial(Tutorial.Reservoir);

        yield return new WaitForSeconds(0.1f);

        MissionManager.Instance.SetMission("Respond to Command");

        radioDialogueStarted = false;
        bool reservoirEmptied = false;

        while (!radioDialogueStarted)
        {
            if (PowerReservoir.Instance.PowerEmpty && !reservoirEmptied)
            {
                reservoirEmptied = true;
                TutorialManager.Instance.OpenTutorial(Tutorial.EmptyReservoir);
                yield return new WaitForSeconds(0.1f);
            }

            yield return null;
        }

        CockpitManager.Instance.PowerSwitcher.PowerResetLever.Lock(true);

        MissionManager.Instance.ClearMission();

        radioDialogueEnded = false;
        while (!radioDialogueEnded) yield return null;

        PlayerDialogueManager.Instance.ForceDialogue(stayAlive);
        CockpitManager.Instance.PowerSwitcher.PowerResetLever.Lock(false);

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;


        UnsubscribeToEvents();
        finished = true;

    }

    private void SubscribeToEvents()
    {
        PlayerDialogueManager.Instance.OnPriorityDialogueEnd.AddListener(OnDialogueEnd);
        RadioManager.Instance.OnRadioDialogueStart.AddListener(OnRadioDialogueStart);
        RadioManager.Instance.OnRadioDialogueEnd.AddListener(OnRadioDialogueEnd);
    }

    private void UnsubscribeToEvents()
    {
        PlayerDialogueManager.Instance.OnPriorityDialogueEnd.RemoveListener(OnDialogueEnd);
        RadioManager.Instance.OnRadioDialogueStart.RemoveListener(OnRadioDialogueStart);
        RadioManager.Instance.OnRadioDialogueEnd.RemoveListener(OnRadioDialogueEnd);
    }

    private void OnDialogueEnd()
    {
        dialogueEnded = true;
    }

    private void OnRadioDialogueStart()
    {
        radioDialogueStarted = true;
    }
    private void OnRadioDialogueEnd()
    {
        radioDialogueEnded = true;
    }
}
