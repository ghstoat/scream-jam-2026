using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Sequence[] sequenceArray;
    private Queue<Sequence> sequenceQueue = new Queue<Sequence> ();
    public static GameManager Instance { get; private set; }

    public static string Checkpoint { get; private set; } = "Checkpoint"; 

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
        if (sequenceArray.Length == 0) return;

        //if (Application.isEditor)
        //{
        //    ResetSave();
        //}
       


        int checkpoint = PlayerPrefs.GetInt(Checkpoint, 0);



        StartCoroutine(SequenceRoutine(checkpoint));
    }

    [Button]
    public void ResetSave()
    {
        PlayerPrefs.SetInt(Checkpoint, 0);
        PlayerPrefs.Save();
    }

    private IEnumerator SequenceRoutine(int checkpoint)
    {
        if (sequenceArray == null || checkpoint < 0 || checkpoint > sequenceArray.Length)
        {
            Debug.LogError("Invalid checkpoint configuration.");
            yield break;
        }

        for (int i = checkpoint; i < sequenceArray.Length; i++)
        {
            Sequence sequence = sequenceArray[i];

            if (sequence == null)
            {
                Debug.LogError($"Sequence at index {i} is null.");
                yield break;
            }

            sequence.Init();

            yield return new WaitUntil(() => sequence.ready);

            sequence.Execute(this);

            yield return new WaitUntil(() => sequence.finished);

            PlayerPrefs.SetInt(Checkpoint, i + 1);
            PlayerPrefs.Save();
        }
    }

}
