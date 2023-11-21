using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Scroll : InventoryItem
{
    public string[] lore;

    public override void Use()
    {
        MenuManager.StartDialogue(lore);
    }
}
