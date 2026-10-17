using UnityEngine;

public class VentManager : PoweredSystem
{

    public float closingProgress { get; private set; } = 0f;

    public bool Open => closingProgress < 1f;
    public bool Closed => closingProgress == 1f;

    [SerializeField] float openingSpeedPerPower;

    public bool opening { get; private set; } = true;

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

        closingProgress = 0f;
        opening = true;
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
