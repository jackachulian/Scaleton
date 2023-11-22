using UnityEngine;
using UnityEngine.Events;

public class SwitchInteractable : Interactable
{
    [SerializeField] private UnityEvent onPress;

    public override void Interact()
    {
        onPress.Invoke();
    }
}