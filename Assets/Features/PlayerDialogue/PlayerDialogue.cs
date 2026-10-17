using UnityEngine;


[CreateAssetMenu(fileName = "NewPlayerDialogue", menuName = "ScreamJam/PlayerDialogue")]
public class PlayerDialogue : ScriptableObject
{
    [SerializeField] DialogueLine[] lines;

    public DialogueLine[] GetLines()
    {
        return lines;
    }
}
