using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Menu : MonoBehaviour {
    // When canceling out of this menu, if parent is non-null open parent window,
    // otherwise return control to the player
    private Menu parentMenu;

    private CanvasGroup canvasGroup;

    private bool focused;

    private static Menu currentMenu;
    public static Menu CurrentMenu {get{return currentMenu;}}

    [SerializeField] private bool showOnStart = false;

    // If this menu closes when cancel (C) is pressed.
    [SerializeField] private bool cancellable = true;

    // True if this should return control to the player when closed and there is no parent menu to return to
    [SerializeField] private bool returnPlayerControl = true;

    private int rows;
    private int topIndex; // Index of the first button shown that is not disabled due to scrolling

    private MenuItem[] items;

    void SetupIndices() {
        items = transform.GetComponentsInChildren<MenuItem>();
        for (int i = 0; i < items.Length; i++) {
            items[i].SetIndex(i);
        }

        rows = Mathf.FloorToInt((GetComponent<RectTransform>().sizeDelta.y - 16) / 16);
        if (rows < 0) rows = 0;

        // Hide items that initally exceed the row count, will be re-enabled when scrolling to it
        for (int i = rows; i < items.Length; i++) {
            items[i].gameObject.SetActive(false);
        }
    }

    
    
    void Start() {
        if (showOnStart) {
            Show();
        }
    }

    private void Update() {
        if (focused) {
            if (cancellable && (Input.GetButtonDown("Cancel") || Input.GetButtonDown("CancelUI"))) {
                Close();
            }
        }
    }

    public void ScrollToShowItem(int index) {
        if (index < 0 || index >= items.Length) {
            // Debug.LogWarning(name+" Trying to scroll to index outside of range: "+index);
            return;
        }

        bool cursorMoved = false;

        while (topIndex+rows-1 < index) {
            items[topIndex].gameObject.SetActive(false);
            items[topIndex+rows].gameObject.SetActive(true);
            topIndex++;
            cursorMoved = true;
        }

        while (topIndex > index) {
            topIndex--;
            items[topIndex+rows].gameObject.SetActive(false);
            items[topIndex].gameObject.SetActive(true);
            cursorMoved = true;
        }

        if (cursorMoved) {
            StartCoroutine(SelectNextUpdate(items[index].gameObject));
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
        if (gameObject == null) {
            Debug.LogError("Trying to open menu that was destroyed: "+gameObject);
            return;
        }
        currentMenu = this;
        if (!MenuManager.openMenus.Contains(currentMenu)) MenuManager.openMenus.Add(currentMenu);
        gameObject.SetActive(true);
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        Focus();
        SelectFirstItemNextFrame();
    }

    public void SelectFirstItemNextFrame() {
        StartCoroutine(SelectFirstNextUpdate());
    }

    IEnumerator SelectFirstNextUpdate() {
        yield return new WaitForEndOfFrame();
        if (transform.childCount > 0) {
            SetupIndices();
            var selectable = transform.GetComponentInChildren<Selectable>();
            if (selectable) selectable.Select();
        }
    }

    IEnumerator SelectNextUpdate(GameObject selectable) {
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(selectable);
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
        MenuManager.openMenus.Remove(this);
        if (parentMenu) {
            parentMenu.gameObject.SetActive(true);
            parentMenu.StartCoroutine(FocusParentNextFrame());
        } else {
            if (returnPlayerControl) GameObject.Find("Player").GetComponent<PlayerController>().EnableControl();
        }
    }

    // Prevents double-inputs between menus.
    IEnumerator FocusParentNextFrame() {
        yield return new WaitForEndOfFrame();
        parentMenu.Show();
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