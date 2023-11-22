using UnityEngine;
using UnityEngine.Events;

public class SwitchInteractable : Interactable
{
    [SerializeField] private UnityEvent onPress;
    bool on;

    public override void Interact()
    {
        on = !on;
        onPress.Invoke();
    }
}