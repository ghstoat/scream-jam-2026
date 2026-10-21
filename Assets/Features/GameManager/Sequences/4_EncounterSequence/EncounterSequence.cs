using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "4_EncounterSequence", menuName = "ScreamJam/Sequence/4_EncounterSequence")]
public class EncounterSequence : Sequence
{
    private bool dialogueEnded = false;
    private bool radioDialogueStarted = false;
    private bool radioDialogueEnded = false;
    private bool flashed = false;

    [SerializeField] PlayerDialogue encounter;
    [SerializeField] PlayerDialogue scare;
    [SerializeField] PlayerDialogue radioJump2;
    [SerializeField] PlayerDialogue missingGirl;

    [SerializeField] Conversation commandGirl;
    [SerializeField] PlayerDialogue commandLost;

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
        SubscribeToEvents();

        PlayerDialogueManager.Instance.ignoreNonPriority = false;

        CockpitManager.Instance.SetState(CockpitState.Default);

        yield return new WaitForSeconds(3);

        //spawn girl here

        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Screen.transform, 1f, 1f, true);
        PlayerDialogueManager.Instance.ForceDialogue(encounter);

        flashed = false;

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;

        MissionManager.Instance.SetMission("Investigate");

        while (!flashed) yield return null;

        flashed = false;

        PlayerDialogueManager.Instance.ignoreNonPriority = true;
        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Screen.transform, .5f, 1f, false);

        //anim of girl moving away

        while (!flashed) yield return null;

        flashed = false;

        //girl gone


        while (!flashed) yield return null;

        flashed = false;

        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Screen.transform, 0.2f, 2f, true);

        //girl jumpscare

        PlayerDialogueManager.Instance.ForceDialogue(scare);

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;

        radioDialogueStarted = false;
        RadioManager.Instance.ReceiveMessage(commandGirl);
        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Radio.IncomingLight.transform, 0.2f, 1f, true);

        yield return new WaitForSeconds(0.5f);

        PlayerDialogueManager.Instance.ForceDialogue(radioJump2);

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;

        yield return new WaitForSeconds(0.5f);

        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Screen.transform, 0.2f, 1f, false);

        yield return new WaitForSeconds(0.5f);

        PlayerDialogueManager.Instance.ForceDialogue(missingGirl);

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;
        PlayerDialogueManager.Instance.ignoreNonPriority = false;

        MissionManager.Instance.SetMission("Respond to Command");

        while (!radioDialogueStarted)yield return null;
       
        MissionManager.Instance.ClearMission();

        radioDialogueEnded = false; 
        while (!radioDialogueEnded) yield return null;

        PlayerDialogueManager.Instance.ForceDialogue(commandLost);

        MissionManager.Instance.SetMission("Respond to Command");


        UnsubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        PlayerDialogueManager.Instance.OnPriorityDialogueEnd.AddListener(OnDialogueEnd);
        RadioManager.Instance.OnRadioDialogueStart.AddListener(OnRadioDialogueStart);
        RadioManager.Instance.OnRadioDialogueEnd.AddListener(OnRadioDialogueEnd);
        FlashManager.Instance.onFlash.AddListener(OnFlash);
    }

    private void UnsubscribeToEvents()
    {
        PlayerDialogueManager.Instance.OnPriorityDialogueEnd.RemoveListener(OnDialogueEnd);
        RadioManager.Instance.OnRadioDialogueStart.RemoveListener(OnRadioDialogueStart);
        RadioManager.Instance.OnRadioDialogueEnd.RemoveListener(OnRadioDialogueEnd);
        FlashManager.Instance.onFlash.RemoveListener(OnFlash);
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

    private void OnFlash()
    {
        flashed = true; 
    }
}