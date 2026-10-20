using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "0_Nothing", menuName = "ScreamJam/Sequence/0_Nothing")]
public class EmptySequence : Sequence
{
    public override void Init()
    {
        ready = true;
        finished = false;
    }
    public override void Execute(GameManager ctx)
    {
        ctx.StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        PlayerDialogueManager.Instance.ignoreNonPriority = false;

        CockpitManager.Instance.SetState(CockpitState.Default);
        yield return null;
    }
}