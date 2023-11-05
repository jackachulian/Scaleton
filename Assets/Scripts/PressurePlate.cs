using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour {
    bool pressed;

    [SerializeField] Sprite unpressedSprite;
    [SerializeField] Sprite pressedSprite;
    [SerializeField] private UnityEvent onPress;
    [SerializeField] private UnityEvent onUnpress;
    SpriteRenderer spriteRenderer;
    List<Collider2D> collisions;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        collisions.Add(other);
        UpdatePressure();
    }

    private void OnTriggerExit2D(Collider2D other) {
        collisions.Remove(other);
        UpdatePressure();
    }

    private void UpdatePressure() {
        if (collisions.Count > 0) {
            Press();
        } else {
            Unpress();
        }
    }

    private void Press() {
        if (!pressed) {
            pressed = true;
            spriteRenderer.sprite = pressedSprite;
            onPress.Invoke();
        }
    }

    private void Unpress() {
        if (pressed) {
            pressed = false;
            spriteRenderer.sprite = unpressedSprite;
            onUnpress.Invoke();
        }
    }
}