using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "1_OpeningSequence", menuName = "ScreamJam/Sequence/1_OpeningSequence")]
public class OpeningSequence : Sequence
{
    [SerializeField] PlayerDialogue eyesOpen;
    [SerializeField] PlayerDialogue sixtySeconds;

    [SerializeField] PlayerDialogue ejectClue1;
    [SerializeField] PlayerDialogue ejectClue2;

    [SerializeField] PlayerDialogue failedEject;

    [SerializeField] AudioClip openingMusic;

    private bool dialogueEndedThisFrame;
    private bool ejectPressed;
    public override void Init()
    {
        ready = true;
        finished = false;
    }

    public override void Execute(GameManager ctx)
    {
        ctx.StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {

        SubscribeToEvents();

        HealthManager.Instance.LockOxygen(true);
        HealthManager.Instance.LockTemp(true);

        PlayerDialogueManager.Instance.ignoreNonPriority = true;

        InteractableManager.Instance.Lock(true);
        PlayerController.Instance.Lock(true);
        CockpitManager.Instance.PowerSwitcher.PowerResetLever.Lock(true);
        CockpitManager.Instance.FlashMachine.PumpLever.Lock(true);
        CockpitManager.Instance.Vent.Lever.Lock(true);

        yield return new WaitForSeconds(.1f);
        PowerManager.Instance.FillAllSystems();

        MusicManager.Instance.PlayMusic(openingMusic, .5f, 0, 0, true, true); 

        CockpitManager.Instance.SetState(CockpitState.Alarm);

        CockpitManager.Instance.ShakeCockpit(1f, 4f);

        yield return new WaitForSeconds(5);

        PlayerDialogueManager.Instance.ForceDialogue(eyesOpen);

        dialogueEndedThisFrame = false;
        while (!dialogueEndedThisFrame) yield return null;  

        TutorialManager.Instance.OpenTutorial(Tutorial.Look);

        yield return new WaitForSeconds(.5f);

        PlayerController.Instance.Lock(false);

        CockpitManager.Instance.StartSelfDestructRoutine();

        yield return new WaitForSeconds(3f);

        PlayerDialogueManager.Instance.ForceDialogue(sixtySeconds);

        dialogueEndedThisFrame = false;
        while (!dialogueEndedThisFrame) yield return null;

        TutorialManager.Instance.OpenTutorial(Tutorial.Interact);
        yield return new WaitForSeconds(0.1f);

        ejectPressed = false;
        CockpitManager.Instance.EjectButton.Lock(false);
        InteractableManager.Instance.Lock(false);

        MissionManager.Instance.SetMission("Escape before the mech self destructs");

        float timer = 0;
        bool clue1 = false;
        bool clue1Mission = false;
        bool clue2 = false;
        bool clue2Mission = false;

        while (!ejectPressed)
        {
            timer += Time.deltaTime;

            if (!clue1 && timer > 10)
            {
                clue1 = true;
                PlayerDialogueManager.Instance.ForceDialogue(ejectClue1);
                dialogueEndedThisFrame = false;
            }
            else if (clue1  && !clue1Mission && dialogueEndedThisFrame)
            {
                clue1Mission = true;
                //MissionManager.Instance.SetMission("Use the eject button");
            }

            else if (!clue2 && timer > 30)
            {
                clue2 = true;
                PlayerDialogueManager.Instance.ForceDialogue(ejectClue2);
                dialogueEndedThisFrame = false;
            }

            else if (clue2  && !clue2Mission && dialogueEndedThisFrame)
            {
                clue2Mission = true;
                MissionManager.Instance.SetMission("Use the eject button under you");

            }
            yield return null;
        }

        yield return new WaitForSeconds(3f);


        PlayerDialogueManager.Instance.ForceDialogue(failedEject);

        dialogueEndedThisFrame = false;
        while (!dialogueEndedThisFrame) yield return null;

        MissionManager.Instance.SetMission("Purge power from all systems");
        yield return new WaitForSeconds(3f);
        TutorialManager.Instance.OpenTutorial(Tutorial.DrainPower);
        yield return new WaitForSeconds(.1f);


        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.PowerSwitcher.PowerResetLever.transform, 1f, 1f, false);


        CockpitManager.Instance.EjectButton.Lock(true);
        CockpitManager.Instance.Vent.Lever.Lock(false);

        CockpitManager.Instance.PowerSwitcher.PowerResetLever.Lock(false);


        while (CockpitManager.Instance.state != CockpitState.Reset)
        {
            yield return null;
        }

        CockpitManager.Instance.FlashMachine.PumpLever.Lock(false);


        MusicManager.Instance.StopMusic(.1f);

        MissionManager.Instance.ClearMission();

        while (CockpitManager.Instance.state != CockpitState.Default)
        {
            yield return null;
        }

        UnsubscribeToEvents();
        finished = true;




    }

    private void SubscribeToEvents()
    {
        PlayerDialogueManager.Instance.OnPriorityDialogueEnd.AddListener(OnDialogueEndedThisFrame);
        CockpitManager.Instance.EjectButton.pressed.AddListener(OnEjectButtonPushed);

    }

    private void UnsubscribeToEvents()
    {
        PlayerDialogueManager.Instance.OnPriorityDialogueEnd.RemoveListener(OnEjectButtonPushed);
        CockpitManager.Instance.EjectButton.pressed.RemoveListener(OnEjectButtonPushed);

    }

    private void OnEjectButtonPushed()
    {
       ejectPressed = true;
    }

    private void OnDialogueEndedThisFrame()
    {
        dialogueEndedThisFrame = true;  
    }


}
