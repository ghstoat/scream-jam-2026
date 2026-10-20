using UnityEngine;

public class VentManager : PoweredSystem
{

    public float closingProgress { get; private set; } = 1f;

    public bool Open => closingProgress < 1f;
    public bool Closed => closingProgress == 1f;

    [SerializeField] float openingSpeedPerPower;

    [SerializeField] float maxToxicity;
    [SerializeField] float toxicityPerSecond;
    public float toxicity { get; private set; }
    public bool ToxicityFull => toxicity == maxToxicity;
    public float ToxicityRatio => toxicity / maxToxicity;
    

    public bool opening { get; private set; } = false;

    public static VentManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"!!! Multiple {this.name} found in scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        closingProgress = 1f;
        opening = false;
        toxicity = 0f;
    }

    private void Update()
    {
        if (!opening && closingProgress < 1)
        {
            closingProgress += openingSpeedPerPower * currentPower * Time.deltaTime;
            closingProgress = Mathf.Clamp01(closingProgress);
        }
        else if (opening && closingProgress > 0)
        {
            closingProgress -= openingSpeedPerPower * currentPower * Time.deltaTime;
            closingProgress = Mathf.Clamp01(closingProgress);
        }

        if (Closed)
        {
            toxicity += toxicityPerSecond * Time.deltaTime;
        }
        else
        {
            toxicity -= toxicityPerSecond * Time.deltaTime;
        }

        toxicity = Mathf.Clamp(toxicity, 0, maxToxicity);
    }

    public void TryToOpen()
    {
        opening = true;
    }

    public void TryToClose()
    {
        opening = false;
    }

}
