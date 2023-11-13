using UnityEngine;

public class Door : MonoBehaviour {
    bool opened;
    int power; // amount of things powering this door to open. will open if 1 or higher and close if 0 or somehow lower

    [SerializeField] Sprite closedSprite;
    [SerializeField] Sprite openSprite;
    [SerializeField] GameObject activeWhenClosed;
    SpriteRenderer spriteRenderer;
    

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Open() {
        power++;

        if (power > 0 && !opened) {
            opened = true;
            spriteRenderer.sprite = openSprite;
            activeWhenClosed.SetActive(false);
        }
    }

    public void Close() {
        power--;

        if (power == 0 && opened) {
            opened = false;
            spriteRenderer.sprite = closedSprite;
            activeWhenClosed.SetActive(true);
        }
    }
}