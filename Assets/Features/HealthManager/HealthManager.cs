using System.Collections.Generic;
using TMPro;
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
    private bool suffocating;
    [SerializeField] float maxTimeWithoutOxygen = 30f;
    private float oxygenTimer;
    [SerializeField] CanvasGroup oxygenCanvasGroup;
    [SerializeField] PlayerDialogue lightheaded;
    private bool lockOxygen = false;

    [Header("Toxic")]
    private bool sickBool;
    [SerializeField] float maxTimeInToxic = 30f;
    private float sickTimer;
    [SerializeField] CanvasGroup sickCanvasGroup;
    [SerializeField] PlayerDialogue sick;
    private bool lockToxicity = false;

    [Header("Temperature")]
    [SerializeField] float maxTimeBadTemp = 30f;
    [SerializeField] float timeBeforeTempDialogue = 15f;

    private bool overheating;
    private bool freezing;

    private float freezeTimer;
    private float overheatingTimer;

    [SerializeField] CanvasGroup freezingCanvasGroup;
    [SerializeField] CanvasGroup overheatingCanvasGroup;

    private bool freezeDialoguePlayed = false;
    private bool overheatingDialoguePlayed = false;

    [SerializeField] PlayerDialogue overheatingDialogue;
    [SerializeField] PlayerDialogue freezingDialogue;

    private bool lockTemp = false;

    [Header("Death")]
    [SerializeField] GameObject deathScreen;
    [SerializeField] TextMeshProUGUI deathMessageText;
    [SerializeField] Dictionary<Death, string> deathMessageDict;

    public bool dead { get; private set; }

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
        if (dead) return;

        HandleOxygen();
        HandleToxicity();
        HandleTemp();
    }

    public void LockOxygen(bool locked)
    {
        lockOxygen = locked;
    }

    public void HandleOxygen()
    {
        suffocating = false;

        if (!lockOxygen) suffocating = OxygenManager.Instance.oxygen <= 0;

        if (suffocating)
        {
            if (oxygenTimer == 0)
                PlayerDialogueManager.Instance.QueueDialogue(lightheaded);

            oxygenTimer += Time.deltaTime;

            if (oxygenTimer > maxTimeWithoutOxygen)
                Die(Death.Oxygen);

            oxygenCanvasGroup.alpha =
                Mathf.Clamp01(oxygenTimer / maxTimeWithoutOxygen);
        }
        else if (oxygenTimer > 0)
        {
            oxygenTimer -= Time.deltaTime;

            if (oxygenTimer <= 0)
            {
                oxygenTimer = 0;
                oxygenCanvasGroup.alpha = 0;
            }
            else
            {
                oxygenCanvasGroup.alpha =
                    Mathf.Clamp01(oxygenTimer / maxTimeWithoutOxygen);
            }
        }
    }

    public void LockToxicity(bool locked)
    {
        lockToxicity = locked;
    }

    public void HandleToxicity()
    {
        sickBool = false;

        if (!lockToxicity || freezing || overheating) sickBool = VentManager.Instance.ToxicityFull;

        if (sickBool)
        {
            if (sickTimer == 0)
                PlayerDialogueManager.Instance.QueueDialogue(sick);

            sickTimer += Time.deltaTime;

            if (sickTimer > maxTimeInToxic)
                Die(Death.Toxicity);

            sickCanvasGroup.alpha =
                Mathf.Clamp01(sickTimer / maxTimeInToxic);
        }
        else if (sickTimer > 0)
        {
            sickTimer -= Time.deltaTime;

            if (sickTimer <= 0)
            {
                sickTimer = 0;
                sickCanvasGroup.alpha = 0;
            }
            else
            {
                sickCanvasGroup.alpha =
                    Mathf.Clamp01(sickTimer / maxTimeInToxic);
            }
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
        bool isFreezing = TemperatureManager.Instance.Freezing;

        if (lockTemp)
            isFreezing = false;

        if (isFreezing)
        {
            freezeTimer += Time.deltaTime;

            if (freezeTimer > maxTimeBadTemp)
                Die(Death.Freezing);

            if (!freezeDialoguePlayed &&
                freezeTimer > timeBeforeTempDialogue)
            {
                freezeDialoguePlayed = true;

                PlayerDialogueManager.Instance.QueueDialogue(freezingDialogue);
            }

            freezingCanvasGroup.alpha =
                Mathf.Clamp01(freezeTimer / maxTimeBadTemp);
        }
        else if (freezeTimer > 0)
        {
            if (freezeDialoguePlayed)
                freezeDialoguePlayed = false;

            freezeTimer = 0;
            freezingCanvasGroup.alpha = 0;
        }
        else
        {
            freezingCanvasGroup.alpha = 0;
        }
    }

    public void HandleOverheating()
    {
        bool isOverheating = TemperatureManager.Instance.Overheating;

        if (lockTemp)
            isOverheating = false;

        if (isOverheating)
        {
            overheatingTimer += Time.deltaTime;

            if (overheatingTimer > maxTimeBadTemp)
                Die(Death.Overheating);

            if (!overheatingDialoguePlayed &&
                overheatingTimer > timeBeforeTempDialogue)
            {
                overheatingDialoguePlayed = true;

                PlayerDialogueManager.Instance.QueueDialogue(overheatingDialogue);
            }

            overheatingCanvasGroup.alpha =
                Mathf.Clamp01(overheatingTimer / maxTimeBadTemp);
        }
        else if (overheatingTimer > 0)
        {
            if (overheatingDialoguePlayed)
                overheatingDialoguePlayed = false;

            overheatingTimer = 0;
            overheatingCanvasGroup.alpha = 0;
        }
        else
        {
            overheatingCanvasGroup.alpha = 0;
        }
    }
}