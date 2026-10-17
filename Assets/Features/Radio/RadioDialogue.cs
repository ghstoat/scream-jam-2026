using UnityEngine;

[CreateAssetMenu(fileName = "NewRadioDialogue", menuName = "ScreamJam/RadioDialogue")]
public class RadioDialogue : ScriptableObject
{
    [SerializeField] DialogueLine[] lines;

    public int frequency;

    public PlayerDialogue playerDialogue;

    public DialogueLine[] GetLines()
    {
        return lines;
    }
}
