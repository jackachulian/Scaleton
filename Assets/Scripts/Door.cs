using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour {
    bool opened;

    [SerializeField] Sprite closedSprite;
    [SerializeField] Sprite openSprite;
    [SerializeField] GameObject activeWhenClosed;
    SpriteRenderer spriteRenderer;
    

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Open() {
        if (!opened) {
            opened = true;
            spriteRenderer.sprite = openSprite;
            activeWhenClosed.SetActive(false);
        }
    }

    public void Close() {
        if (opened) {
            opened = false;
            spriteRenderer.sprite = closedSprite;
            activeWhenClosed.SetActive(true);
        }
    }
}