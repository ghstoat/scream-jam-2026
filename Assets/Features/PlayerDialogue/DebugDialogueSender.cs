using NaughtyAttributes;
using UnityEngine;

public class DebugDialogueSender : MonoBehaviour
{
    [SerializeField] PlayerDialogue debugDialogue;

    [Button]
    private void SendDialogueToPlayer()
    {
        if (debugDialogue == null) return;

        PlayerDialogueManager.Instance.QueueDialogue(debugDialogue);
    }
}
