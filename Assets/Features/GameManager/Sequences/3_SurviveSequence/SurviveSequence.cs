using System.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "3_SurviveSequence", menuName = "ScreamJam/Sequence/3_SurviveSequence")]
public class SurviveSequence : Sequence
{

    private bool dialogueEnded = false;
    private bool radioDialogueStarted = false;
    private bool radioDialogueEnded = false;


    [SerializeField] AudioClip music;

    [SerializeField] PlayerDialogue oxygenIntro;
    [SerializeField] PlayerDialogue heaterIntro;
    [SerializeField] PlayerDialogue ventIntro;

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
        HealthManager.Instance.LockToxicity(true);

        OxygenManager.Instance.ChangeOxygen(-OxygenManager.Instance.oxygen);

        PlayerDialogueManager.Instance.ignoreNonPriority = false;

        CockpitManager.Instance.SetState(CockpitState.Default);

        yield return new WaitForSeconds(1);

        MissionManager.Instance.SetMission("Survive till dawn");

        //MusicManager.Instance.PlayMusic(music, .5f, 0, 0, true, true);

        yield return new WaitForSeconds(5);

        // oxygen section
        PlayerDialogueManager.Instance.QueueDialogue(oxygenIntro);

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;


        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.OxygenMeter.transform, 1f, 1f);

        yield return new WaitForSeconds(1.5f);

        TutorialManager.Instance.OpenTutorial(Tutorial.Oxygen);

        yield return new WaitForSeconds(5f);

        HealthManager.Instance.LockOxygen(false);

        bool reservoirEmptied = false;


        while (OxygenManager.Instance.OxygenRatio == 0)
        {
            if (PowerReservoir.Instance.PowerEmpty && !reservoirEmptied)
            {
                reservoirEmptied = true;
                TutorialManager.Instance.OpenTutorial(Tutorial.EmptyReservoir);
            }
            yield return null;
        }

        while (OxygenManager.Instance.OxygenRatio < 0.5)
        {
            yield return null;
        }

        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.OxygenMeter.transform, 1f, 1f);

        // heater section

        PlayerDialogueManager.Instance.QueueDialogue(heaterIntro);

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;


        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Heater.transform, 1f, 1f);

        yield return new WaitForSeconds(1.5f);

        TutorialManager.Instance.OpenTutorial(Tutorial.Heater);

        yield return new WaitForSeconds(0.1f);

        HealthManager.Instance.LockTemp(false);

        float tempTimeNeeded = 10;
        float timer = 0;

        while (timer < tempTimeNeeded)
        {
            if (PowerReservoir.Instance.PowerEmpty && !reservoirEmptied)
            {
                reservoirEmptied = true;
                TutorialManager.Instance.OpenTutorial(Tutorial.EmptyReservoir);
                yield return new WaitForSeconds(0.1f);
            }

            if (TemperatureManager.Instance.Livable)
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer = 0;
            }
            yield return null;

        }

        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Heater.transform, 1f, 1f);

        //vent section

        PlayerDialogueManager.Instance.QueueDialogue(ventIntro);

        dialogueEnded = false;
        while (!dialogueEnded) yield return null;


        PlayerController.Instance.LookAtTransform(CockpitManager.Instance.Vent.transform, 1f, 1f);

        yield return new WaitForSeconds(1.5f);

        TutorialManager.Instance.OpenTutorial(Tutorial.Vent);

        yield return new WaitForSeconds(0.1f);

        HealthManager.Instance.LockToxicity(false); 

        while (!VentManager.Instance.Open)
        {
            if (PowerReservoir.Instance.PowerEmpty && !reservoirEmptied)
            {
                reservoirEmptied = true;
                TutorialManager.Instance.OpenTutorial(Tutorial.EmptyReservoir);
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;

        }

        Debug.Log("Suvive done");

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
