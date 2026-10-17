using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class TimeSlowInstance
{
    public string key;
    public float timeScale;
    public float duration;
    public bool useScaledTime;
    public float elapsedTime;
    public TimeSlowInstance(string key, float timeScale, float duration, bool useScaledTime)
    {
        this.key = key;
        this.timeScale = timeScale;
        this.duration = duration;
        this.useScaledTime = useScaledTime;
        this.elapsedTime = 0f;
    }
}

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField] private float lerpSpeed = 10f;

    private List<TimeSlowInstance> timeSlowInstances = new List<TimeSlowInstance>();

    private bool paused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"!!! Multiple {this.name} found in scene.");
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    private void Update()
    {
        UpdateTimeScale();
    }

    public void PauseTime(bool pause)
    {
        paused = pause;

        if (pause)
        {
            Time.timeScale = 0f;
        }
        else
        {
            UpdateTimeScale();
        }
    }

    public void SlowTime(float timeScale, float duration, bool useScaledTime, string key = null)
    {
        if (timeScale <= 0f)
        {
            Debug.LogError("!!! Can only slow time with a timeScale greater than 0");
            return;
        }

        timeSlowInstances.Add(new TimeSlowInstance(key, timeScale, duration, useScaledTime));
    } 

    public void RemoveTimeSlow(string key)
    {
        for (int i = timeSlowInstances.Count - 1; i >= 0; i--)
        {
            if (timeSlowInstances[i].key == key)
            {
                timeSlowInstances.RemoveAt(i);
            }
        }

        UpdateTimeScale();

    }

    private void UpdateTimeScale()
    {

        float slowestTimeScale = 1f;

        for (int i = timeSlowInstances.Count - 1; i >= 0; i--)
        {
            TimeSlowInstance instance = timeSlowInstances[i];

            if (instance.timeScale < slowestTimeScale) slowestTimeScale = instance.timeScale;

            if (instance.useScaledTime) instance.elapsedTime += Time.deltaTime;
            else instance.elapsedTime += Time.unscaledDeltaTime;

            if (instance.elapsedTime >= instance.duration) timeSlowInstances.RemoveAt(i);  
        }

        if (paused) return;
        else if (Time.timeScale == 0 && slowestTimeScale != 0f) Time.timeScale = slowestTimeScale;
        else if (slowestTimeScale == 0f && Time.timeScale != 0f) Time.timeScale = 0f;
        else LerpToTimeScale(slowestTimeScale);


    }

    private void LerpToTimeScale(float targetTimeScale)
    {
        Time.timeScale = Mathf.MoveTowards(
            Time.timeScale,
            targetTimeScale,
            lerpSpeed * Time.unscaledDeltaTime
        );
    }

    [Button]
    private void DebugTimeScale()
    {
        Debug.Log($"Current Time Scale: {Time.timeScale}");
    }
}
