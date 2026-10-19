using TMPro;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [SerializeField] GameObject missionUI;
    [SerializeField] TextMeshProUGUI text;

    private string currentMission = "";
    public static MissionManager Instance { get; private set; }
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


    private void Start()
    {
        ClearMission();
    }

    public void ClearMission()
    {
        missionUI.SetActive(false);
        currentMission = "";
        text.text = "";
    }

    public void SetMission(string mission)
    {
        ClearMission();
        currentMission = mission;
        text.text = mission;
        missionUI.SetActive(true);

    }


}
