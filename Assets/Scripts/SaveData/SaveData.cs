using System.Collections.Generic;

public class SaveData {
    public static List<InventoryItem> inventory {get; private set;}

    static SaveData() {
        inventory = new List<InventoryItem>();
    }

    public static void ClearInventory() {
        inventory.Clear();
    }
}