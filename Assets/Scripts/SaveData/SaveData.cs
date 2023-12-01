using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveData {
    public static List<InventoryItem> inventory {get; private set;}

    public static int scrollsCollected;
    public static int fragmentsCollected;
    public static float startTime;

    public static void LoadInventory() {
        // todo: in the future, get data from playerprefs instead of resetting inventory every load
        inventory = new List<InventoryItem>();
        scrollsCollected = 0;
        fragmentsCollected = 0;
    }

    public static void ClearInventory() {
        inventory.Clear();
        scrollsCollected = 0;
        fragmentsCollected = 0;
    }

    public static string TotalTimeString() {
        int totalSeconds = Mathf.FloorToInt(Time.time - SaveData.startTime);
        float seconds = totalSeconds % 60;
        float minutes = totalSeconds/60 % 60;
        float hours = totalSeconds / 3600;
        if (hours > 0) {
            return hours+":"+minutes.ToString("D2")+":"+seconds.ToString("D2");
        } else {
            return minutes.ToString("D2")+":"+seconds.ToString("D2");
        }
    }
}