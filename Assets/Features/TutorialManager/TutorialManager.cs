using System.Collections.Generic;
using UnityEngine;

public enum Tutorial
{
    None,
    Look,
    Interact,
    DrainPower,
    PowerSwitcher,
    Nodes,
    Oxygen,
    Heater,
    EmptyReservoir,
    Vent,
    Reservoir
}


public class TutorialManager : MonoBehaviour
{

    public Tutorial tutorial { get; private set; }
    [SerializeField] GameObject tutorialScreen;
    
    public bool ShowingTutorial => tutorial != Tutorial.None;

    [SerializeField] Dictionary<Tutorial, GameObject> tutorialDict;

    public static TutorialManager Instance { get; private set; }
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
        CloseTutorial();
    }

    public void CloseTutorial()
    {
        if (ShowingTutorial) TimeManager.Instance.PauseTime(false);

        tutorial = Tutorial.None;
        tutorialScreen.SetActive( false );

        foreach (GameObject tut in tutorialDict.Values)
        {
            tut.SetActive(false);
        }
    }

    public void OpenTutorial(Tutorial newTutorial)
    {
        if (ShowingTutorial) return;

        if (!tutorialDict.ContainsKey(newTutorial)) return;

        if (PauseManager.Instance.paused) PauseManager.Instance.TogglePause();

        TimeManager.Instance.PauseTime(true);
        tutorial = newTutorial;
        tutorialDict[tutorial].SetActive(true);
        tutorialScreen.SetActive(true);
    }


}
