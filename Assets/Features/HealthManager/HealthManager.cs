using UnityEngine;

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

    [Header("Temperature")]
    [SerializeField] float maxTimeBadTemp = 30f;
    [SerializeField] float timeBeforeTempDialogue = 15f;
    private float tempTimer;


    private bool tempDialoguePlayed = false;
    [SerializeField] PlayerDialogue overheating;
    [SerializeField] PlayerDialogue freezing;

    private bool lockTemp = false;

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

    }

    public void Die(Death death)
    {
        Debug.LogWarning(death.ToString());
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


    public void LockTemp(bool locked)
    {
        lockTemp = locked;
    }

    public void HandleTemp()
    {
        bool freezing = TemperatureManager.Instance.Freezing;
        bool overheating = TemperatureManager.Instance.Overheating;
        bool dying = freezing || overheating;

        if (lockTemp) dying = false;

        if (dying)
        {

            tempTimer += Time.deltaTime;
            if (tempTimer > maxTimeBadTemp)
            {
                if (freezing) Die(Death.Freezing);
                if (overheating) Die(Death.Overheating);
            }

            if (!tempDialoguePlayed && tempTimer > timeBeforeTempDialogue)
            {
                tempDialoguePlayed = true;
                PlayerDialogue dialogue = this.freezing;
                if (overheating) dialogue = this.overheating;

                PlayerDialogueManager.Instance.QueueDialogue(dialogue);
            }
            
        }
        else if (tempTimer > 0)
        {
          
            if (tempDialoguePlayed == true) tempDialoguePlayed = false;
            tempTimer = 0;
        }
    }
}
