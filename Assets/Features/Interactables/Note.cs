using UnityEngine;
using UnityEngine.Events;

public class Note : Interactable
{
    [SerializeField] Sprite noteSprite;

    public override void OnDragStart()
    {
        base.OnDragStart();

        if (!NoteManager.Instance.open) NoteManager.Instance.OpenNote(noteSprite);

        OnDragExit();
    }

}
