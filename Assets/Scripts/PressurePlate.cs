using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressurePlateSimple : MonoBehaviour {
    bool pressed;
    // if stepoffTime is above 0, this may be false while pressed is true.
    bool stepped;

    [SerializeField] Sprite unpressedSprite;
    [SerializeField] Sprite pressedSprite;
    [SerializeField] private List<Door> doors;
    [SerializeField] private List<Lantern> lanterns;
    [SerializeField] private List<LaserGate> gates;
    [SerializeField] private UnityEvent onPress;
    [SerializeField] private UnityEvent onUnpress;
    SpriteRenderer spriteRenderer;
    List<Collider2D> collisions;

    // Delay between stepping off plate and the plate showing unpresssed and closing connected doors/running unpress
    [SerializeField] float stepoffDelay;
    float stepoffTime = 0f;


    private void Awake() {
        collisions = new List<Collider2D>();
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
            stepped = true;
            Press();
        } else {
            if (stepoffDelay > 0) {
                stepped = false;
                stepoffTime = stepoffDelay;
            } else {
                Unpress();
            }
        }
    }

    private void Press() {
        if (!pressed) {
            pressed = true;
            spriteRenderer.sprite = pressedSprite;
            foreach (var door in doors) door.Open();
            foreach (var lantern in lanterns) lantern.Power();
            foreach (var gate in gates) gate.Toggle();
            onPress.Invoke();
        }
    }

    private void Unpress() {
        
        
        if (pressed) {
            pressed = false;
            spriteRenderer.sprite = unpressedSprite;
            foreach (var door in doors) door.Close();
            foreach (var lantern in lanterns) lantern.Unpower();
            foreach (var gate in gates) gate.Toggle();
            onUnpress.Invoke();
        }
    }

    private void Update() {
        if (pressed && !stepped) {
            if (stepoffTime > 0f) {
                stepoffTime -= Time.deltaTime;
            } else {
                Unpress();
            }
        }
    }
}