using UnityEngine;

public class LaserGate : MonoBehaviour {
    private Animator animator;

    [SerializeField] bool on;

    private void Awake() {
        animator = GetComponent<Animator>();
        animator.SetBool("on", on);
    }
    
    public void Toggle() {
        on = !on;
        animator.SetBool("on", on);
    }
}