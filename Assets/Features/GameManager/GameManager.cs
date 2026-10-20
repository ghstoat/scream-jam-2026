using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Sequence[] sequenceArray;
    private Queue<Sequence> sequenceQueue = new Queue<Sequence> ();
    public static GameManager Instance { get; private set; }
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

        sequenceQueue.Clear ();
        foreach (Sequence seq in sequenceArray)
        {
            sequenceQueue.Enqueue(seq);
        }

        StartCoroutine(SequenceRoutine());
    }

    public bool SkipToSequence(Sequence target)
    {
        Queue<Sequence> newQueue = new Queue<Sequence>();


        bool found = false; 
        foreach(var sequence in sequenceQueue)
        {
            if (sequence == target)
            {
                found = true;
            }

            if (found) newQueue.Enqueue(sequence);
        }

        if (!found) return false;

        sequenceQueue = newQueue;
        return true;
    }
    private IEnumerator SequenceRoutine()
    {
        while (true)
        {
            if (sequenceQueue.Count <= 0)
            {
                yield return null;
                continue;
            }

            Sequence sequence = sequenceQueue.Dequeue();

            sequence.Init();

            yield return new WaitForSeconds(sequence.delay);

            while (!sequence.ready)
            {
                yield return null;
            }

            sequence.Execute(this);

            while (!sequence.finished)
            {
                yield return null;
            }



            yield return null;
        }
    }

}
