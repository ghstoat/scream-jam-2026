using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Death
{
    SelfDestruct,
    Oxygen,
    Overheating,
    Freezing,
    Toxicity
}

public class HealthManager : MonoBehaviour
{
    [Header("Oxygen")]
    [SerializeField] float maxTimeWithoutOxygen = 30f;
    private float oxygenTimer;
    [SerializeField] CanvasGroup oxygenCanvasGroup;
    [SerializeField] PlayerDialogue lightheaded;
    private bool lockOxygen = false;
    
    [Header("Oxygen")]
    [SerializeField] float maxTimeInToxic = 30f;
    private float sickTimer;
    [SerializeField] CanvasGroup sickCanvasGroup;
    [SerializeField] PlayerDialogue sick;
    private bool lockToxicity = false;


    [Header("Temperature")]
    [SerializeField] float maxTimeBadTemp = 30f;
    [SerializeField] float timeBeforeTempDialogue = 15f;
    private float freezeTimer;
    private float overheatingTimer;


    private bool freezeDialoguePlayed = false;
    private bool overheatingDialoguePlayed = false;

    [SerializeField] PlayerDialogue overheating;
    [SerializeField] PlayerDialogue freezing;

    private bool lockTemp = false;

    [Header("Death")]
    [SerializeField] GameObject deathScreen;
    [SerializeField] TextMeshProUGUI deathMessageText;
    [SerializeField] Dictionary<Death, string> deathMessageDict;
    public bool dead {  get; private set; }


    public static HealthManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"!!! Multiple {this.name} found in scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        dead = false;
    }

    public void Die(Death death)
    {
        if (dead) return; 
        TimeManager.Instance.PauseTime(true);

        dead = true;
        deathMessageText.text = deathMessageDict[death];
        deathScreen.SetActive(true);

        Debug.LogWarning(death.ToString());
    }

    public void ContinueFromCheckpoint()
    {
        TimeManager.Instance.PauseTime(false);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    public void ReturnToTitle()
    {
        TimeManager.Instance.PauseTime(false);

        SceneManager.LoadScene("TitleScreen");
    }

    private void Update()
    {
        HandleOxygen();
        HandleTemp();
    }

    public void LockOxygen(bool locked)
    {
        lockOxygen = locked;
    }

    public void HandleOxygen()
    {
        bool suffocating = false;

        if (!lockOxygen) suffocating = OxygenManager.Instance.oxygen <= 0;

        if (suffocating)
        {
            if (oxygenTimer == 0) PlayerDialogueManager.Instance.QueueDialogue(lightheaded);

            oxygenTimer += Time.deltaTime;
            if (oxygenTimer > maxTimeWithoutOxygen)
            {
                Die(Death.Oxygen);
            }



            oxygenCanvasGroup.alpha = oxygenTimer / maxTimeWithoutOxygen;
        }
        else if (oxygenTimer > 0)
        {

            oxygenTimer -= Time.deltaTime;

            if (oxygenTimer <= 0) oxygenCanvasGroup.alpha = 0;
            else oxygenCanvasGroup.alpha = oxygenTimer / maxTimeWithoutOxygen;
        }
    }
    public void LockToxicity(bool locked)
    {
        lockToxicity = locked;
    }

    public void HandleToxicity()
    {
        bool sickBool = false;

        if (!lockToxicity) sickBool = VentManager.Instance.ToxicityFull;



        if (sickBool)
        {
            if (sickTimer == 0) PlayerDialogueManager.Instance.QueueDialogue(sick);

            sickTimer += Time.deltaTime;
            if (sickTimer > maxTimeInToxic)
            {
                Die(Death.Toxicity);
            }



            sickCanvasGroup.alpha = sickTimer / maxTimeInToxic;
        }
        else if (sickTimer > 0)
        {

            sickTimer -= Time.deltaTime;

            if (sickTimer <= 0) sickCanvasGroup.alpha = 0;
            else sickCanvasGroup.alpha = sickTimer / maxTimeInToxic;
        }
    }

    public void LockTemp(bool locked)
    {
        lockTemp = locked;
    }

    public void HandleTemp()
    {
        HandleFreezing();
        HandleOverheating();
    }

    public void HandleFreezing()
    {
        bool freezing = TemperatureManager.Instance.Freezing;

        if (lockTemp) freezing = false;

        if (freezing)
        {

            freezeTimer += Time.deltaTime;
            if (freezeTimer > maxTimeBadTemp)
            {
                Die(Death.Freezing);
            }

            if (!freezeDialoguePlayed && freezeTimer > timeBeforeTempDialogue)
            {
                freezeDialoguePlayed = true;
                PlayerDialogue dialogue = this.freezing;

                PlayerDialogueManager.Instance.QueueDialogue(dialogue);
            }
            
        }
        else if (freezeTimer > 0)
        {
          
            if (freezeDialoguePlayed == true) freezeDialoguePlayed = false;
            freezeTimer = 0;
        }
    }

    public void HandleOverheating()
    {
        bool overheating = TemperatureManager.Instance.Overheating;

        if (lockTemp) overheating = false;

        if (overheating)
        {

            overheatingTimer += Time.deltaTime;
            if (overheatingTimer > maxTimeBadTemp)
            {
                Die(Death.Overheating);
            }

            if (!overheatingDialoguePlayed && overheatingTimer > timeBeforeTempDialogue)
            {
                overheatingDialoguePlayed = true;
                PlayerDialogue dialogue = this.overheating;

                PlayerDialogueManager.Instance.QueueDialogue(dialogue);
            }

        }
        else if (overheatingTimer > 0)
        {

            if (overheatingDialoguePlayed == true) overheatingDialoguePlayed = false;
            overheatingTimer = 0;
        }
    }
}
