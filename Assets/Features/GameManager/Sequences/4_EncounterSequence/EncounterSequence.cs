using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] Conversation fakeCommand;

    [SerializeField] PlayerDialogue ventGirl;
    [SerializeField] PlayerDialogue girlBlocked;
    [SerializeField] PlayerDialogue girlScare;

    private AsyncOperation asyncLoad;

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
        asyncLoad = SceneManager.LoadSceneAsync("WinScreen", LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        bool reservoirEmptied = false;

        PlayerDialogueManager.Instance.ignoreNonPriority = false;

        CockpitManager.Instance.SetState(CockpitState.Default);

        yield return new WaitForSeconds(3);

        GhostGirlManager.Instance.Appear();

        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Screen.transform, 1f, 1f, true);
        PlayerDialogueManager.Instance.ForceDialogue(encounter);

        flashed = false;

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;

        MissionManager.Instance.SetMission("Investigate the strange figure in the camera");

        float timeBeforeClue = 120;

        while (!flashed)
        {
            if (timeBeforeClue > 0)
            {
                timeBeforeClue -= Time.deltaTime;

                if (timeBeforeClue <= 0)
                {
                    MissionManager.Instance.SetMission("Use the flash to investigate the strange figure in the camera");

                }
            }
            if (PowerReservoir.Instance.PowerEmpty && !reservoirEmptied)
            {
                reservoirEmptied = true;
                TutorialManager.Instance.OpenTutorial(Tutorial.EmptyReservoir);
                yield return new WaitForSeconds(0.1f);
            }


            yield return null;
        }

        flashed = false;

        PlayerDialogueManager.Instance.ignoreNonPriority = true;
        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Screen.transform, .5f, 1f, false);

        GhostGirlManager.Instance.Appear();

        while (!flashed) yield return null;

        flashed = false;

        GhostGirlManager.Instance.Hidden();


        while (!flashed) yield return null;

        flashed = false;

        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Screen.transform, 0.2f, 8f, true);

        GhostGirlManager.Instance.Jump();

        PlayerDialogueManager.Instance.ForceDialogue(scare);

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;

        RadioManager.Instance.ReceiveMessage(commandGirl);        
        
        radioDialogueStarted = false;
        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Radio.IncomingLight.transform, 0.2f, 1f, true);

        yield return new WaitForSeconds(1f);
        GhostGirlManager.Instance.Hidden(); 

        PlayerDialogueManager.Instance.ForceDialogue(radioJump2);

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;

        yield return new WaitForSeconds(0.1f);

        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Screen.transform, 0.2f, 1f, false);

        yield return new WaitForSeconds(0.5f);

        PlayerDialogueManager.Instance.ForceDialogue(missingGirl);

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;
        PlayerDialogueManager.Instance.ignoreNonPriority = false;

        MissionManager.Instance.SetMission("Respond to Command");

        while (!radioDialogueStarted) yield return null;

        MissionManager.Instance.ClearMission();

        radioDialogueEnded = false;

        while (!radioDialogueEnded) yield return null;




        RadioManager.Instance.ReceiveMessage(fakeCommand);

        radioDialogueStarted = false;
        radioDialogueEnded = false;
        PlayerDialogueManager.Instance.ForceDialogue(commandLost);

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;

        PlayerDialogueManager.Instance.ignoreNonPriority = false;
        MissionManager.Instance.SetMission("Respond to Command");

        while (!radioDialogueStarted) yield return null;

        PlayerDialogueManager.Instance.ignoreNonPriority = true;

        while (!radioDialogueEnded) yield return null;

        yield return new WaitForSeconds(1);

        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Vent.transform, 0.2f, 1f, false);

        VentManager.Instance.SomethingInVent(true);

        //play ghost girl entering vent sfx



        PlayerDialogueManager.Instance.ForceDialogue(ventGirl);

        while (!VentManager.Instance.Closed)
        {
            if (PowerReservoir.Instance.PowerEmpty && !reservoirEmptied)
            {
                reservoirEmptied = true;
                TutorialManager.Instance.OpenTutorial(Tutorial.EmptyReservoir);
                yield return new WaitForSeconds(0.1f);
            }


            yield return null;
        }

        //play banging sfx

        yield return new WaitForSeconds(10);
        VentManager.Instance.SomethingInVent(false);


        while (VentManager.Instance.Closed) yield return null;

        PlayerDialogueManager.Instance.ForceDialogue(girlBlocked);

        yield return new WaitForSeconds(15);

        GhostGirl2Manager.Instance.Jump();
        PlayerDialogueManager.Instance.ForceDialogue(girlScare);

        yield return new WaitForSeconds(4);

        asyncLoad.allowSceneActivation = true;

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