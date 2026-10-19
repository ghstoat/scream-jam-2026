using UnityEngine;
using UnityEngine.Events;

public class Button : Interactable
{
    public UnityEvent pressed;

    public override void OnDragStart()
    {
        base.OnDragStart();
        pressed?.Invoke();

        OnDragExit();
    }


}
