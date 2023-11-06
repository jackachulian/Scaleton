using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour {
    // When canceling out of this menu, if parent is non-null open parent window,
    // otherwise return control to the player
    private Menu parentMenu;

    private CanvasGroup canvasGroup;

    private bool focused;

    private static Menu currentMenu;
    public static Menu CurrentMenu {get{return currentMenu;}}

    public readonly static List<Menu> openMenus = new List<Menu>();
    static Menu() {
        openMenus = new List<Menu>();
    }
    

    private void Update() {
        if (focused) {
            if (Input.GetButtonDown("Cancel")) {
                Close();
            }
        }
    }

    private void Reset() {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Open this window from a parent window to return to when this is closed.
    public void Open(Menu parent) {
        parentMenu = parent;
        Show();
    }

    // Show this window on the screen
    public virtual void Show() {
        currentMenu = this;
        if (!openMenus.Contains(currentMenu)) openMenus.Add(currentMenu);
        gameObject.SetActive(true);
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        Focus();
        SelectFirstItem();
    }

    public void SelectFirstItem() {
        transform.GetChild(0).GetComponent<Selectable>().Select();
        StartCoroutine(SelectNextUpdate());
    }

    IEnumerator SelectNextUpdate() {
        yield return new WaitForEndOfFrame();
        transform.GetChild(0).GetComponent<Selectable>().Select();
    }

    // Hide menu without returning to parent.
    public void Hide() {
        gameObject.SetActive(false);
        Unfocus();
    }

    // Dim menu without fully hiding it.
    public void Dim() {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.5f;
    }

    public void Focus() {
        focused = true;
        foreach (Transform child in transform) {
            Button button = child.GetComponent<Button>();
            if (button) button.interactable = true;
        }
    }

    public void Unfocus() {
        focused = false;
        foreach (Transform child in transform) {
            Button button = child.GetComponent<Button>();
            if (button) button.interactable = false;
        }
    }

    // Hide menu and return to parent if any; if not return control to player
    public void Close() {
        Hide();
        openMenus.Remove(this);
        if (parentMenu) {
            parentMenu.Show();
        } else {
            GameObject.Find("Player").GetComponent<PlayerController>().EnableControl();
        }
    }

    // Navigate to a window, setting its parent to this window for when it closes
    public void NavigateToWindowAndHide(Menu child) {
        Hide();
        child.Open(this);
    }

    public void NavigateToWindowAndDim(Menu child) {
        Dim();
        child.Open(this);
    }

    public void NavigateToWindowAndUnfocus(Menu child) {
        Unfocus();
        child.Open(this);
    }
}