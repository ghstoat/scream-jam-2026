using UnityEngine;

public abstract class Sequence : ScriptableObject
{
    public float delay;

    public bool ready = false;

    public bool finished = false;

    public abstract void Init();

    public abstract void Execute(GameManager ctx);
}
