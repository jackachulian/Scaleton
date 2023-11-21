using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    [SerializeField] private Menu emptyMenuPrefab;

    [SerializeField] private Button choiceButtonPrefab;

    public PlayerController player {get; private set;}

    public static Dialogue globalDialogue {get; private set;}

    [SerializeField] private Dialogue dialogueBox;


    public static List<Menu> openMenus {get; private set;}

    private void Awake() {
        openMenus = new List<Menu>();
        globalDialogue = dialogueBox;
    }

    private void Start() {
        player = FindObjectOfType<PlayerController>();
    }

    public Menu CreateDialogMenu() {
        if (!CanOpenDialog()) {
            Debug.LogWarning("Dialog opened while player has control, or at a time dialog shouldn't be openable");
        }
        Menu menu = Instantiate(emptyMenuPrefab, transform);
        foreach (Transform child in menu.transform) {
            Destroy(child.gameObject);
        }
        menu.Show();
        player.DisableControl();
        return menu;
    }

    public void AddButton(Menu menu, string label, Action onPress) {
        Button button = Instantiate(choiceButtonPrefab, menu.transform);
        button.transform.GetChild(0).GetComponent<TMP_Text>().text = label;
        button.onClick.AddListener(() => {
            onPress.Invoke();
            menu.Close();
        });
    }

    public bool CanOpenDialog() {
        return player.HasControl();
    }

    public static void StartDialogue(string[] lines) {
        HideOpenMenus();
        globalDialogue.StartDialogue(lines);
    }

    public static void HideOpenMenus() {
        for (int i=0; i<openMenus.Count; i++) {
            var menu = openMenus[i];
            if (menu) {
                menu.Hide();
            } else {
                Debug.LogWarning("Non-menu found in open menus");
                openMenus.Remove(menu);
            };
        }
    }

    public static void ShowHiddenMenus() {
        for (int i = 0; i < openMenus.Count; i++) {
            openMenus[i].Show();
        }
    }
}