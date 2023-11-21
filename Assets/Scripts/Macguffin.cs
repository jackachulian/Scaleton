using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Macguffin : InventoryItem
{
    public string[] collectionMessage;
    
    public override void Use()
    {
        for (int i=0; i<Menu.openMenus.Count; i++) {
            var menu = Menu.openMenus[i];
            if (menu) menu.Hide();
        }
        MenuManager.StartDialogue(collectionMessage);
    }

}
