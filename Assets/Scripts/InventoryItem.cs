using UnityEngine;


public abstract class InventoryItem : ScriptableObject {
    public string title;

    public abstract void Use();
}