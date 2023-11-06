using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Scroll : InventoryItem
{
    public string[] lore;

    public static Dialogue scrollDialogue;

    public override void Use()
    {
        foreach (var menu in Menu.openMenus) menu.Hide();
        scrollDialogue.StartDialogue(lore);
    }
}
