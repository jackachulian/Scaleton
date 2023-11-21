using UnityEngine;

[CreateAssetMenu]
public class Macguffin : InventoryItem
{
    public string[] collectionMessage;
    
    public override void Use()
    {
        MenuManager.StartDialogue(collectionMessage);
    }

}
