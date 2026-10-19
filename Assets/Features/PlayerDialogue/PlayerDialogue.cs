using UnityEngine;


[CreateAssetMenu(fileName = "NewPlayerDialogue", menuName = "ScreamJam/PlayerDialogue")]
public class PlayerDialogue : ScriptableObject
{
    public bool priority = false;
    [SerializeField] DialogueLine[] lines;

    public DialogueLine[] GetLines()
    {
        return lines;
    }
}
