using UnityEngine;

[CreateAssetMenu(fileName = "NewConverstation", menuName = "ScreamJam/Conversation")]
public class Conversation : ScriptableObject
{
    [SerializeField] RadioDialogue[] dialogue;
    public RadioDialogue[] Dialogue => dialogue;
}
