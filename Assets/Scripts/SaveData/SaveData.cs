using System.Collections.Generic;

public class SaveData {
    public static List<InventoryItem> inventory {get; private set;}

    public static void LoadInventory() {
        // todo: in the future, get data from playerprefs instead of resetting inventory every load
        inventory = new List<InventoryItem>();
    }

    public static void ClearInventory() {
        inventory.Clear();
    }
}