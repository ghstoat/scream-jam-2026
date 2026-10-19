using NaughtyAttributes;
using UnityEngine;

public class DebugRadioSender : MonoBehaviour
{
    [SerializeField] RadioDialogue[] debugDialogue;

    [Button]
    private void SendDialogueToRadio()
    {
        if (debugDialogue == null) return;

        foreach (RadioDialogue r in debugDialogue)
        {
            RadioManager.Instance.ReceiveMessage(r);
        }
    }
}
