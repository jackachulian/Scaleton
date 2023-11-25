using UnityEngine;

public class Door : Respawnable {
    bool opened;
    int power; // amount of things powering this door to open. will open if 1 or higher and close if 0 or somehow lower

    [SerializeField] Sprite closedSprite;
    [SerializeField] Sprite openSprite;
    [SerializeField] GameObject activeWhenClosed;
    SpriteRenderer spriteRenderer;


    AudioSource audioSource;
    

    protected override void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Open() {
        power++;

        if (power > 0 && !opened) {
            opened = true;
            spriteRenderer.sprite = openSprite;
            activeWhenClosed.SetActive(false);
            if (Time.time > 1f) SoundManager.PlaySound(audioSource, "door_open");
        }
    }

    public void Close() {
        power--;

        if (power == 0 && opened) {
            opened = false;
            spriteRenderer.sprite = closedSprite;
            activeWhenClosed.SetActive(true);
            if (Time.time > 1f) SoundManager.PlaySound(audioSource, "door_close");
        }
    }

    public override void Respawn()
    {
        Close();
        power++;
    }
}